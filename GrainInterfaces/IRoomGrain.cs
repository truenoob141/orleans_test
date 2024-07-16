namespace GrainInterfaces;

public interface IRoomGrain : IGrainWithGuidKey
{
    Task AddPlayer(Guid playerId, IPlayerObserver observer);
    Task SubmitNumber(Guid playerId, int playerNumber);
}