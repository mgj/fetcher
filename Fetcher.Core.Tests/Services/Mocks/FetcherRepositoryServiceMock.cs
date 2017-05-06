using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherRepositoryServiceMock : IFetcherRepositoryService
    {
        private List<IUrlCacheInfo> _database = new List<IUrlCacheInfo>();

        public IUrlCacheInfo GetEntryForUrl(Uri url)
        {
            var hero = _database.Where(x => x.Url == url.OriginalString).FirstOrDefault();
            if(hero != null) hero.LastAccessed = DateTimeOffset.UtcNow;
            return hero;
        }

        public IUrlCacheInfo InsertUrl(Uri uri, string response)
        {
            var existing = GetEntryForUrl(uri);
            if (existing != null)
            {
                _database.Remove(existing);
            }

            var timestamp = DateTime.UtcNow;

            var hero = new UrlCacheInfoMock();
            hero.Response = response;
            hero.Url = uri.OriginalString;
            hero.Created = timestamp;
            hero.LastAccessed = timestamp;
            hero.LastUpdated = timestamp;

            _database.Add(hero);

            return hero;
        }

        public void UpdateUrl(Uri uri, IUrlCacheInfo hero, string response)
        {
            var timestamp = DateTime.UtcNow;

            hero.Response = response;
            hero.Url = uri.OriginalString;
            hero.LastAccessed = timestamp;
            hero.LastUpdated = timestamp;
        }
    }
}
