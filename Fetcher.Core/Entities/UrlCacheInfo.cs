using artm.Fetcher.Core.Models;
using SQLite;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;

namespace artm.Fetcher.Core.Entities
{
    public class UrlCacheInfo : IUrlCacheInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(FetcherWebRequest))]
        public int FetcherWebRequestId { get; set; }

        [OneToOne]
        public FetcherWebRequest FetcherWebRequest { get; set; }

        [ForeignKey(typeof(FetcherWebResponse))]
        public int FetcherWebResponseId { get; set; }

        [OneToOne]
        public FetcherWebResponse FetcherWebResponse { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset LastAccessed { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        [Ignore]
        public CacheSourceType FetchedFrom { get; set; }
    }
}
