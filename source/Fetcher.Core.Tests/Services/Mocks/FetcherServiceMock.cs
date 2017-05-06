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
        private static FetcherServiceMock instance;

        private FetcherServiceMock() { }

        public static new FetcherServiceMock Instance
        {
            get
            {
                return new FetcherServiceMock();
            }
        }

        public string FetchFromWebResponse
        {
            get;
            set;
        }


        public Task<string> FetchFromWebWithSuppliedWebResponse(Uri uri)
        {
            return Task.FromResult(FetchFromWebResponse);
        }
    }
}
