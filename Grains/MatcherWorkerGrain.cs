using GrainInterfaces;
using Orleans.Concurrency;

namespace Grains;

[StatelessWorker]
public class MatcherWorkerGrain : Grain, IMatcherWorkerGain
{
    private readonly Queue<IGameObserver> _waitingPlayers = new();
    
    public async Task FindGame(IGameObserver gameObserver)
    {
        if (_waitingPlayers.Count == 0)
        {
            _waitingPlayers.Enqueue(gameObserver);
            return;
        }

        var opponentObserver = _waitingPlayers.Dequeue();
        
        var gameGrain = GrainFactory.GetGrain<IGameGrain>(Guid.NewGuid());
        var playerId = await gameObserver.GetPlayerId();
        var opponentId = await opponentObserver.GetPlayerId();

        await gameGrain.AddPlayerToGame(playerId, gameObserver);
        await gameGrain.AddPlayerToGame(opponentId, opponentObserver);

        var gameId = gameGrain.GetPrimaryKey();
        gameObserver.OnGameStarted(gameId);
        opponentObserver.OnGameStarted(gameId);
    }
}