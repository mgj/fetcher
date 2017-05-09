using artm.Fetcher.Core.Services;

namespace artm.Fetcher.Droid.Services
{
    public class FetcherRepositoryStoragePathService : IFetcherRepositoryStoragePathService
    {
        public string GetPath(string filename = "fetcher.db3")
        {
            var fullpath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
            return fullpath;
        }
    }
}