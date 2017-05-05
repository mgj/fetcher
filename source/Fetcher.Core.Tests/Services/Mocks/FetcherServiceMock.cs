using artm.Fetcher.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherServiceMock : FetcherService
    {
        public FetcherServiceMock(IFetcherRepositoryService repository, IFetcherWebService webService)
            : base(repository, webService)
        {
        }

        public string FetchFromWebResponse
        {
            get;
            set;
        }


        protected override Task<string> FetchFromWeb(Uri uri)
        {
            return Task.FromResult(FetchFromWebResponse);
        }
    }
}
