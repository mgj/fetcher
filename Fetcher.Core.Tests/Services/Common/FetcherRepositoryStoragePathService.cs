using artm.Fetcher.Core.Services;
using System;
using System.IO;

namespace artm.Fetcher.Core.Tests.Services.Common
{
    public class FetcherRepositoryStoragePathService : IFetcherRepositoryStoragePathService
    {
        public string GetPath(string filename = "fetcher.db3")
        {
            return Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                        "fetcher-tests.db3");
        }
    }
}
