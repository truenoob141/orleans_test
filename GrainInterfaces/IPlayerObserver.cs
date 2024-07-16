namespace GrainInterfaces;

public interface IPlayerObserver : IGrainObserver
{
    void OnGameStarted(Guid roomId);
    void OnGameResult(Guid playerId, int winValue);
    
    
}