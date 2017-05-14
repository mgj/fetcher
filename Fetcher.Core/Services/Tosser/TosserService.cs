using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services.Tosser
{
    public class TosserService : ITosserService
    {
        private readonly IFetcherWebService _web;

        public TosserService(IFetcherWebService webService)
        {
            _web = webService;
        }

        public FetcherWebResponse Toss(Uri url, HttpWebRequest request)
        {
            FetcherWebResponse result = null;
            try
            {
                result = _web.DoPlatformRequest(url, request);
            }
            catch (Exception ex )
            {

                throw;
            }
            return result;
        }
    }
}
