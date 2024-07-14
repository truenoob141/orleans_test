using GrainInterfaces;
using Orleans.Concurrency;

namespace Grains;

[Reentrant]
public class GameGrain : Grain, IGameGrain
{
    // list of players in the current game
    // for simplicity, player 0 always plays an "O" and player 1 plays an "X"
    //  who starts a game is a random call once a game is started, and is set via indexNextPlayerToMove
    private Dictionary<Guid, IGameObserver> _players = new();

    private GameState _gameState;

    private int _winValue;
    private List<GameMove> _moves = new();

    public override Task OnActivateAsync(CancellationToken token)
    {
        _players = new Dictionary<Guid, IGameObserver>();
        _moves = new List<GameMove>();

        _gameState = GameState.AwaitingPlayers;
        _winValue = Random.Shared.Next(100);

        return base.OnActivateAsync(token);
    }

    public Task<GameState> AddPlayerToGame(Guid player, IGameObserver observer)
    {
        if (_gameState is GameState.Finished) 
            throw new ApplicationException("Can't join game once its over");
        
        if (_gameState is GameState.InPlay)
            throw new ApplicationException("Can't join game once its in play");
        
        _players.Add(player, observer);

        // check if the game is ready to play
        if (_gameState is GameState.AwaitingPlayers && _players.Count is 2)
            _gameState = GameState.InPlay;

        // let user know if game is ready or not
        return Task.FromResult(_gameState);
    }

    // make a move during the game
    public async Task<GameState> MakeMove(GameMove move)
    {
        // check if its a legal move to make
        if (_gameState is not GameState.InPlay)
            throw new ApplicationException("This game is not in play");

        if (!_players.ContainsKey(move.PlayerId))
            throw new ArgumentException("No such playerid for this game", nameof(move));
        
        if (move.Value < 0 || move.Value >= 100)
            throw new ArgumentException("Bad value", nameof(move));

        // record move
        _moves.Add(move);
        
        var playerId = move.PlayerId;
        var opponentId = _players.Keys.First(p => p != playerId);
        
        // check for a winning move
        bool win = false;
        bool draw = false;

        int ours = 0;
        int theirs = 0;
        
        if (_moves.Count == 2)
        {
            var opponentMove = _moves.First(m => m.PlayerId == opponentId);
            ours = Math.Abs(_winValue - move.Value);
            theirs = Math.Abs(_winValue - opponentMove.Value);

            if (ours > theirs)
                win = true;
            else if (ours == theirs)
                draw = true;
        }
        
        if (win || draw)
        {
            _gameState = GameState.Finished;

            var playerObserver = _players[playerId];
            var opponentObserver = _players[opponentId];

            Console.WriteLine($"Game finished:" +
                $"\n  GameId: {this.GetPrimaryKey()}" +
                $"\n  PlayerId: {playerId}, Value {ours}" +
                $"\n  OpponentId: {opponentId}, Value {theirs}" +
                $"\n  Win value : {_winValue}");
            
            // inform this player of outcome
            playerObserver.OnGameFinished(this.GetPrimaryKey(), win ? GameOutcome.Win : GameOutcome.Draw);

            // inform other player of outcome
            opponentObserver.OnGameFinished(this.GetPrimaryKey(), win ? GameOutcome.Lose : GameOutcome.Draw);
            
            return _gameState;
        }
        
        return _gameState;
    }
}