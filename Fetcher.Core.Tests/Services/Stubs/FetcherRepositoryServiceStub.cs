using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Platform.Generic;
using System.IO;
using artm.Fetcher.Core.Entities;

namespace artm.Fetcher.Core.Tests.Services.Stubs
{
    public class FetcherRepositoryServiceStub : FetcherRepositoryService
    {
        public FetcherRepositoryServiceStub() 
            : base(() => new SQLiteConnectionWithLock(
                new SQLitePlatformGeneric(),
                new SQLiteConnectionString(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
                        "FetcherRepository.db3"), 
                    false)
                ))
        {
            ClearAllAsync().Wait();
            base.Initialize().Wait();
        }

        private async Task ClearAllAsync()
        {
            try
            {
                await DropTableAsync<UrlCacheInfo>();
                await DropTableAsync<IFetcherWebResponse>();
            }
            catch (Exception)
            {
            }
        }
    }
}
