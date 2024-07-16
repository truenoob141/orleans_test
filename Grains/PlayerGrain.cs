using GrainInterfaces;

namespace Grains;

public class PlayerGrain : Grain, IPlayerGrain
{
    private readonly IPersistentState<PlayerState> _playerState;

    public PlayerGrain(
        [PersistentState("player", "playerStore")]
        IPersistentState<PlayerState> playerState)
    {
        _playerState = playerState;
    }

    public override Task OnActivateAsync(CancellationToken token)
    {
        return base.OnActivateAsync(token);
    }

    public Task<int> GetScore()
    {
        return Task.FromResult(_playerState.State.score);
    }

    public Task AddScore(int score)
    {
        _playerState.State.score += score;
        return _playerState.WriteStateAsync();
    }
}