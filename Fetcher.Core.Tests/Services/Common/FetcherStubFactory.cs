using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Common
{
    public static class FetcherStubFactory
    {
        public static FetcherWebResponse FetcherWebResponseSuccessFactory(string response = "FetcherRepositoryServiceTests response", int id = 0)
        {
            return new FetcherWebResponse()
            {
                Id = id,
                Body = response,
                HttpStatusCode = 200
            };
        }

        public static FetcherWebRequest FetcherWebRequestGetFactory(Uri url, int id = 0)
        {
            return new FetcherWebRequest()
            {
                Id = id,
                Url = url.OriginalString,
                Method = "GET"
            };
        }
    }
}
