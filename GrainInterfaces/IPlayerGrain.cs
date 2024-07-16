namespace GrainInterfaces;

public interface IPlayerGrain : IGrainWithGuidKey
{
    Task<int> GetScore();
    Task AddScore(int score);
}
