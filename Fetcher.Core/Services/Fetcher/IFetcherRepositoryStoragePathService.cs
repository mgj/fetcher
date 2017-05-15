namespace artm.Fetcher.Core.Services
{
    public interface IFetcherRepositoryStoragePathService
    {
        string GetPath(string filename = "fetcher.db3");
    }
}
