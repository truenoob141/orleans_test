using GrainInterfaces;

namespace Client;

public class GameController : IGameObserver
{
    private readonly IGrainFactory _grainFactory;

    private TaskCompletionSource<Guid> _waitGame;
    private TaskCompletionSource<GameOutcome> _waitResult;

    private readonly Guid _playerId = Guid.NewGuid();

    public GameController(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public void OnGameStarted(Guid gameId)
    {
        Console.WriteLine($"Game Started {gameId}");
        _waitGame?.SetResult(gameId);
    }

    public void OnGameFinished(Guid gameId, GameOutcome outcome)
    {
        Console.WriteLine($"Game Finished {gameId}, Result {outcome}");
        _waitResult?.SetResult(outcome);
    }

    // public async Task<Guid> FindGame()
    // {
    //     if (_waitGame != null)
    //         throw new ApplicationException("Already waiting for a game");
    //
    //     _waitGame = new TaskCompletionSource<Guid>();
    //     var matcherGrain = _grainFactory.GetGrain<IMatcherWorkerGain>(Guid.Empty);
    //     await matcherGrain.FindGame(this);
    //     return await _waitGame.Task;
    // }
    
    public async Task<Guid> FindGame()
    {
        if (_waitGame != null)
            throw new ApplicationException("Already waiting for a game");

        _waitGame = new TaskCompletionSource<Guid>();

        // Создание ссылки на наблюдателя
        var observerReference = _grainFactory.CreateObjectReference<IGameObserver>(this);

        var matcherGrain = _grainFactory.GetGrain<IMatcherWorkerGain>(Guid.Empty);
        await matcherGrain.FindGame(observerReference);
        return await _waitGame.Task;
    }

    public Task<int> GetScore()
    {
        var player = _grainFactory.GetGrain<IPlayerGrain>(_playerId);
        return player.GetScore();
    }

    public async Task<GameOutcome> MakeMove(Guid id, int value)
    {
        if (_waitResult != null)
            throw new ApplicationException("Already move");

        _waitResult = new TaskCompletionSource<GameOutcome>();

        var game = _grainFactory.GetGrain<IGameGrain>(id);
        var move = new GameMove { PlayerId = _playerId, Value = value };
        Console.WriteLine("Wait make move");
        await game.MakeMove(move);

        Console.WriteLine("Wait result");
        return await _waitResult.Task;
    }

    public Task<Guid> GetPlayerId()
    {
        return Task.FromResult(_playerId);
    }
}