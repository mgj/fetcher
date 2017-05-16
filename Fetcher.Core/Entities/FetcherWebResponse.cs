using artm.Fetcher.Core.Entities;
using SQLite;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace artm.Fetcher.Core.Models
{
    public class FetcherWebResponse : IFetcherWebResponse
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int HttpStatusCode { get; set; }

        [Ignore]
        public Exception Error { get; set; }
        public string Body { get; set; }
        [Ignore]
        public Dictionary<string, string> Headers { get; set; }

        [Ignore]
        public bool IsSuccess
        {
            get
            {
                return HttpStatusCode >= 200 && HttpStatusCode < 300;
            }
        }
        
    }
}
