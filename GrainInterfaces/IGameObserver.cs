namespace GrainInterfaces;

public interface IGameObserver : IGrainObserver
{
    Task<Guid> GetPlayerId();
    void OnGameStarted(Guid gameId);
    void OnGameFinished(Guid gameId, GameOutcome outcome);
}