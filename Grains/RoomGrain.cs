using GrainInterfaces;

namespace Grains;

public class RoomGrain : Grain, IRoomGrain
{
    private int _winValue;
    
    private Dictionary<Guid, int> _playerNumbers = new();
    private Dictionary<Guid, IPlayerObserver> _observers = new();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _winValue = new Random().Next(0, 100);
        _observers = new();
        _playerNumbers = new();
        
        return base.OnActivateAsync(cancellationToken);
    }
    
    public async Task AddPlayer(Guid playerId, IPlayerObserver observer)
    {
        _playerNumbers.Add(playerId, -1);
        _observers.Add(playerId, observer);
        
        if (_playerNumbers.Count == 2)
        {
            var roomId = this.GetPrimaryKey();
            foreach (var obs in _observers.Values)
                obs.OnGameStarted(roomId);
        }
    }

    public async Task SubmitNumber(Guid playerId, int playerNumber)
    {
        _playerNumbers[playerId] = playerNumber;
        
        if (_playerNumbers.Count == 2 && _playerNumbers.All(p => p.Value >= 0 && p.Value < 100))
        {
            (var opponentId, int opponentNumber) = _playerNumbers.First(p => p.Key != playerId);
            
            // Get deltas
            int playerDelta = Math.Abs(_winValue - playerNumber);
            int opponentDelta = Math.Abs(_winValue - opponentNumber);

            var winnerId = playerDelta < opponentDelta ? playerId : opponentId;
            bool isDraw = playerDelta == opponentDelta;

            if (!isDraw)
            {
                var winner = GrainFactory.GetGrain<IPlayerGrain>(winnerId);
                await winner.AddScore(1);
            }

            var tasks = new List<Task>(_observers.Count);
            foreach (var observer in _observers.Values)
            {
                if (isDraw)
                    observer.OnGameResult(Guid.Empty, _winValue);
                else
                    observer.OnGameResult(winnerId, _winValue);
            }
        }
    }
}