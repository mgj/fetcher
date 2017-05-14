using artm.Fetcher.Core.Entities;
using SQLite;
using System;

namespace artm.Fetcher.Core.Services
{
    public class FetcherRepositoryService : IFetcherRepositoryService, IDisposable
    {
        protected IFetcherRepositoryStoragePathService PathService;
        private SQLiteConnection _db;

        public FetcherRepositoryService(IFetcherRepositoryStoragePathService pathService)
        {
            PathService = pathService;

            Db.CreateTable<UrlCacheInfo>();
        }

        protected SQLiteConnection Db
        {
            get
            {
                if (_db == null)
                {
                    _db = new SQLiteConnection(PathService.GetPath());
                }
                return _db;
            }
        }

        public IUrlCacheInfo GetEntryForUrl(Uri url)
        {
            UrlCacheInfo data;
            var needle = url.OriginalString;
            data = Db.Table<UrlCacheInfo>().Where(x => x.Url.Equals(needle)).FirstOrDefault();

            if(data != null)
            {
                data.LastAccessed = DateTimeOffset.UtcNow;
                Db.Update(data);
            }

            return data;
        }

        public IUrlCacheInfo PreloadUrl(Uri uri, string response)
        {
            var timestamp = DateTimeOffset.UtcNow.AddYears(-1);
            return InsertUrl(uri, response, timestamp);
        }

        public IUrlCacheInfo InsertUrl(Uri uri, string response)
        {
            return InsertUrl(uri, response, DateTimeOffset.UtcNow);
        }

        private IUrlCacheInfo InsertUrl(Uri uri, string response, DateTimeOffset timestamp)
        {
            UrlCacheInfo hero = null;
            if (string.IsNullOrEmpty(response))
            {
                return hero;
            }
            hero = new UrlCacheInfo()
            {
                Response = response,
                Url = uri.OriginalString,
                Created = timestamp,
                LastUpdated = timestamp,
                LastAccessed = timestamp
            };

            var existing = GetEntryForUrl(uri) as UrlCacheInfo;
            if (existing != null)
            {
                _db.Delete(existing);
            }

            Db.Insert(hero);

            return hero;
        }
       

        public void UpdateUrl(Uri uri, IUrlCacheInfo hero, string response)
        {
            if (uri == null || hero == null || string.IsNullOrEmpty(response))
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            hero.Response = response;
            hero.Url = uri.OriginalString;
            hero.LastUpdated = timestamp;

            Db.Update(hero);
        }

        public void Dispose()
        {
            Db.Close();
        }
    }
}
