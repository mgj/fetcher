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

        public FetcherWebResponse Toss(Uri url)
        {
            if (url == null) throw new NullReferenceException("Url");

            FetcherWebResponse result = null;
            FetcherWebRequest request = new FetcherWebRequest()
            {
                Method = "POST",
                Url = url
            };

            try
            {
                result = _web.DoPlatformRequest(url, request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to toss to url: " + url.OriginalString);
                throw;
            }
            return result;
        }
    }
}
