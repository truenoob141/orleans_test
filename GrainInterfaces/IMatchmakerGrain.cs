namespace GrainInterfaces;

public interface IMatchmakerGrain : IGrainWithGuidKey
{
    Task AddPlayerToQueue(Guid playerId, IPlayerObserver observer);
}