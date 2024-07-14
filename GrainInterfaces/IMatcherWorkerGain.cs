namespace GrainInterfaces;

public interface IMatcherWorkerGain : IGrainWithGuidKey
{
    Task FindGame(IGameObserver gameObserver);
}