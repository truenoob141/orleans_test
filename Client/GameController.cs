using System.Security.Cryptography;
using System.Text;
using GrainInterfaces;
using Microsoft.Extensions.Logging;

namespace Client;

public class GameController : IPlayerObserver
{
    private readonly IGrainFactory _grainFactory;

    private TaskCompletionSource<Guid> _waitGameStarted;
    private TaskCompletionSource<(Guid, int)> _waitResult;

    private Guid _playerId;

    public GameController(IGrainFactory grainFactory)
    {
        _playerId = Guid.NewGuid();
        _grainFactory = grainFactory;
    }
    
    public async Task<Guid> FindGame()
    {
        _waitGameStarted?.TrySetCanceled();
        _waitGameStarted = new TaskCompletionSource<Guid>();

        var observerReference = _grainFactory.CreateObjectReference<IPlayerObserver>(this);

        var matcherGrain = _grainFactory.GetGrain<IMatchmakerGrain>(Guid.Empty);
        await matcherGrain.AddPlayerToQueue(_playerId, observerReference);

        // _logger.LogInformation("Wait a game...");
        return await _waitGameStarted.Task;
    }

    public Task<int> GetScore()
    {
        var player = _grainFactory.GetGrain<IPlayerGrain>(_playerId);
        return player.GetScore();
    }

    public async Task<(Guid, int)> SendNumber(Guid roomId, int value)
    {
        _waitResult?.TrySetCanceled();
        _waitResult = new TaskCompletionSource<(Guid, int)>();

        var room = _grainFactory.GetGrain<IRoomGrain>(roomId);
        await room.SubmitNumber(_playerId, value);

        // _logger.LogInformation("Wait result...");
        return await _waitResult.Task;
    }

    public Guid GetPlayerId()
    {
        return _playerId;
    }

    public void SetPlayerId(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        _playerId = new Guid(hash);
    }

    public void OnGameStarted(Guid roomId)
    {
        _waitGameStarted?.TrySetResult(roomId);
    }

    public void OnGameResult(Guid playerId, int winValue)
    {
        _waitResult?.TrySetResult((playerId, winValue));
    }
}