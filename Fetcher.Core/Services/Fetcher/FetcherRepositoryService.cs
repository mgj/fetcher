using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using SQLite;
using SQLite.Net;
using SQLite.Net.Async;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public class FetcherRepositoryService : SQLiteAsyncConnection, IFetcherRepositoryService
    {
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
            var dbData = await this.GetAllWithChildrenAsync<UrlCacheInfo>(x => x.Url.Equals(needle));
            var data = dbData.FirstOrDefault();

            if (data != null)
            {
                data.LastAccessed = DateTimeOffset.UtcNow;
                await this.UpdateWithChildrenAsync(data);
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
            if (existing != null)
            {
                await DeleteAsync(existing);
            }

            var theResponse = response as FetcherWebResponse;
            await this.InsertAsync(theResponse);

            hero = new UrlCacheInfo()
            {
                FetcherWebResponseId = theResponse.Id,
                FetcherWebResponse = theResponse,
                Url = uri.OriginalString,
                Created = timestamp,
                LastUpdated = timestamp,
                LastAccessed = timestamp
            };
            await this.InsertWithChildrenAsync(hero, true);

            return hero;
        }


        public async Task UpdateUrlAsync(Uri uri, IUrlCacheInfo hero, IFetcherWebResponse response)
        {
            if (uri == null || hero == null || response == null || response as FetcherWebResponse == null)
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            hero.FetcherWebResponse = response as FetcherWebResponse;
            hero.Url = uri.OriginalString;
            hero.LastUpdated = timestamp;

            await this.UpdateWithChildrenAsync(hero);
        }
    }
}
