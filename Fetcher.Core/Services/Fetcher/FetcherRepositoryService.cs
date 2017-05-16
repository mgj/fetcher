using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using SQLite;
using SQLite.Net;
using SQLite.Net.Async;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public class FetcherRepositoryService : SQLiteAsyncConnection, IFetcherRepositoryService
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        public FetcherRepositoryService(Func<SQLiteConnectionWithLock> mylock) : base(mylock, null, TaskCreationOptions.None)
        {
        }

        public async Task Initialize()
        {
            await CreateTableAsync<UrlCacheInfo>();
            await CreateTableAsync<FetcherWebResponse>();
        }

        public async Task<IUrlCacheInfo> GetEntryForUrlAsync(Uri url)
        {
            var needle = url.OriginalString;
            
            IUrlCacheInfo data = null;

            await _lock.WaitAsync();
            try
            {
                var dbData = await this.GetAllWithChildrenAsync<UrlCacheInfo>(x => x.Url.Equals(needle));
                data = dbData.FirstOrDefault();
            }
            finally
            {
                _lock.Release();
            }

            if (data != null)
            {
                data.LastAccessed = DateTimeOffset.UtcNow;
                await DatabaseUpdate(data);
            }

            return data;
        }

        public async Task<IUrlCacheInfo> PreloadUrlAsync(Uri uri, IFetcherWebResponse response)
        {
            var timestamp = DateTimeOffset.UtcNow.AddYears(-1);
            return await InsertUrlAsync(uri, response, timestamp);
        }

        public async Task<IUrlCacheInfo> InsertUrlAsync(Uri uri, IFetcherWebResponse response)
        {
            return await InsertUrlAsync(uri, response, DateTimeOffset.UtcNow);
        }

        private async Task<IUrlCacheInfo> InsertUrlAsync(Uri uri, IFetcherWebResponse response, DateTimeOffset timestamp)
        {
            UrlCacheInfo hero = null;
            if (response == null || response as FetcherWebResponse == null)
            {
                return hero;
            }

            var existing = await GetEntryForUrlAsync(uri) as UrlCacheInfo;
            await _lock.WaitAsync();
            try
            {
                await this.RunInTransactionAsync(tran =>
                {
                    if (existing != null)
                    {
                        tran.Delete(existing);
                    }

                    var theResponse = new FetcherWebResponse()
                    {
                        Body = response.Body,
                        Error = response.Error,
                        Headers = response.Headers,
                        HttpStatusCode = response.HttpStatusCode,
                    };
                    tran.Insert(theResponse);

                    hero = new UrlCacheInfo()
                    {
                        FetcherWebResponseId = theResponse.Id,
                        FetcherWebResponse = theResponse,
                        Url = uri.OriginalString,
                        Created = timestamp,
                        LastUpdated = timestamp,
                        LastAccessed = timestamp
                    };
                    tran.Insert(hero);
                });
            }
            finally
            {
                _lock.Release();
            }

            return hero;
        }

        private async Task DatabaseInsert(object hero)
        {
            await _lock.WaitAsync();
            try
            {
                await this.InsertWithChildrenAsync(hero, true);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task DatabaseUpdate(object hero)
        {
            await _lock.WaitAsync();
            try
            {
                await this.UpdateWithChildrenAsync(hero);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateUrlAsync(Uri uri, IUrlCacheInfo hero, IFetcherWebResponse response)
        {
            if (uri == null || hero == null || response == null || response as FetcherWebResponse == null)
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            if(response.Id != 0)
            {
                hero.FetcherWebResponseId = response.Id;
                hero.FetcherWebResponse = response as FetcherWebResponse;
            }
            else
            {
                hero.FetcherWebResponse.Body = response.Body;
                hero.FetcherWebResponse.Error = response.Error;
                hero.FetcherWebResponse.Headers = response.Headers;
                hero.FetcherWebResponse.HttpStatusCode = response.HttpStatusCode;
            }
            hero.Url = uri.OriginalString;
            hero.LastUpdated = timestamp;

            await DatabaseUpdate(hero);
        }
    }
}
