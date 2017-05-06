using artm.Fetcher.Core.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public class FetcherRepositoryService : IFetcherRepositoryService
    {
        private SQLiteConnection _db;
        protected IFetcherRepositoryStoragePathService PathService;

        public FetcherRepositoryService(IFetcherRepositoryStoragePathService pathService)
        {
            PathService = pathService;
            _db = new SQLiteConnection(PathService.GetPath());
            _db.CreateTable<UrlCacheInfo>();
        }

        public IUrlCacheInfo GetEntryForUrl(Uri url)
        {
            UrlCacheInfo data;

            var needle = url.OriginalString;
            data = _db.Table<UrlCacheInfo>().Where(x => x.Url == needle).FirstOrDefault();
            return data;
        }

        public IUrlCacheInfo InsertUrl(Uri uri, string response)
        {
            UrlCacheInfo hero = null;
            if (string.IsNullOrEmpty(response))
            {
                return hero;
            }

            var existing = GetEntryForUrl(uri) as UrlCacheInfo;
            if (existing != null)
            {
                _db.Delete(existing);
            }

            var timestamp = DateTime.UtcNow;

            hero = new UrlCacheInfo()
            {
                Response = response,
                Url = uri.OriginalString,
                Created = timestamp,
                LastUpdated = timestamp,
                LastAccessed = timestamp
            };

            _db.Insert(hero);

            return hero;
        }

        public void UpdateLastAccessed(IUrlCacheInfo hero)
        {
            hero.LastAccessed = DateTimeOffset.UtcNow;
            _db.Update(hero);
        }

        public void UpdateUrl(Uri uri, IUrlCacheInfo hero, string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            hero.Response = response;
            hero.Url = uri.OriginalString;
            hero.LastAccessed = timestamp;
            hero.LastUpdated = timestamp;

            _db.Update(hero);
        }
    }
}
