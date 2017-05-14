using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services.Tosser
{
    public class TosserService : ITosserService
    {
        public TosserService(IFetcherWebService webService)
        {

        }

        public async Task<string> Toss()
        {
            return string.Empty;
        }
    }
}
