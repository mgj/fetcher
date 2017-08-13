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
using SQLiteNetExtensionsAsync.Extensions;
using artm.Fetcher.Core.Models;
using Moq;

namespace artm.Fetcher.Core.Tests.Services.Stubs
{
    public class FetcherRepositoryServiceStub : FetcherRepositoryService
    {
        public FetcherRepositoryServiceStub(bool recreateDB = true) 
            : base(Mock.Of<IFetcherLoggerService>(), () => new SQLiteConnectionWithLock(
                new SQLitePlatformGeneric(),
                new SQLiteConnectionString(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                        "FetcherRepository.db3"),
                    false)
                ))
        {
            if(recreateDB == true)
            {
                ClearAllAsync().Wait();
                base.Initialize().Wait();
            }
        }

        public async Task<List<UrlCacheInfo>> AllCacheInfo()
        {
            var data = await this.GetAllWithChildrenAsync<UrlCacheInfo>(x => true);
            return data.ToList();
        }

        public async Task<List<FetcherWebResponse>> AllWebResponse()
        {
            var data = await this.GetAllWithChildrenAsync<FetcherWebResponse>(x => true);
            return data.ToList();
        }

        public async Task<List<FetcherWebRequest>> AllWebRequests()
        {
            var data = await this.GetAllWithChildrenAsync<FetcherWebRequest>(x => true);
            return data.ToList();
        }

        private async Task ClearAllAsync()
        {
            await DropTableAsync<UrlCacheInfo>();
            await DropTableAsync<FetcherWebResponse>();
            await DropTableAsync<FetcherWebRequest>();
            await Initialize();
        }
    }
}
