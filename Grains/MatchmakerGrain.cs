using GrainInterfaces;

namespace Grains;

public class MatchmakerGrain : Grain, IMatchmakerGrain
{
    private readonly Queue<(Guid, IPlayerObserver)> _playerQueue = new();

    public Task AddPlayerToQueue(Guid playerId, IPlayerObserver observer)
    {
        while (_playerQueue.Count > 0)
        {
            var (opponent, opponentObserver) = _playerQueue.Dequeue();
            // TODO Validate opponent?
            var room = GrainFactory.GetGrain<IRoomGrain>(Guid.NewGuid());
            
            return Task.WhenAll(
                room.AddPlayer(playerId, observer),
                room.AddPlayer(opponent, opponentObserver)
            );
        }

        _playerQueue.Enqueue((playerId, observer));
        return Task.CompletedTask;
    }
}