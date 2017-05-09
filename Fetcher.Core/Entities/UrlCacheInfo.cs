using SQLite;
using System;

namespace artm.Fetcher.Core.Entities
{
    public class UrlCacheInfo : IUrlCacheInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Response { get; set; }

        [Indexed(Name = "Url", Unique = true)]
        public string Url { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastAccessed { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        [Ignore]
        public CacheSourceType FetchedFrom { get; set; }
    }
}
