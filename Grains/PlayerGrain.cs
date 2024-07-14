using GrainInterfaces;

namespace Grains;

public class PlayerGrain : Grain, IPlayerGrain
{
    private int _score;

    public override Task OnActivateAsync(CancellationToken token)
    {
        _score = 0;

        return base.OnActivateAsync(token);
    }

    public Task<int> GetScore()
    {
        return Task.FromResult(_score);
    }
}