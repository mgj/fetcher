using Fetcher.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fetcher.Touch.Services
{
    public class FetcherRepositoryStoragePathService : IFetcherRepositoryStoragePathService
    {
        public string GetPath(string filename = "fetcher.db3")
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), filename);
        }
    }
}
