namespace GrainInterfaces;

public interface IGameGrain : IGrainWithGuidKey
{
    Task<GameState> AddPlayerToGame(Guid player, IGameObserver observer);
    Task<GameState> MakeMove(GameMove move);
}

[Serializable]
public enum GameState
{
    AwaitingPlayers,
    InPlay,
    Finished
}

[Serializable]
public enum GameOutcome
{
    Win,
    Lose,
    Draw
}

[GenerateSerializer]
public struct GameMove
{
    [Id(0)]
    public Guid PlayerId { get; set; }

    [Id(1)]
    public int Value { get; set; }
}

[GenerateSerializer]
public struct GameSummary
{
    [Id(0)]
    public GameState State { get; set; }

    [Id(1)]
    public GameOutcome Outcome { get; set; }

    [Id(2)]
    public bool AllowMove { get; set; }

    [Id(3)]
    public Guid GameId { get; set; }

    [Id(4)]
    public string[] Usernames { get; set; }

    [Id(5)]
    public string Name { get; set; }
}