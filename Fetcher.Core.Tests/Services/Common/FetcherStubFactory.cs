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
        public static FetcherWebResponse FetcherWebResponseFactory(string response = "FetcherRepositoryServiceTests response")
        {
            return new FetcherWebResponse() { Body = response, HttpStatusCode = 200 };
        }
    }
}
