using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using Polly;
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
        protected IFetcherWebService WebService { get; set; }

        public TosserService(IFetcherWebService webService)
        {
            WebService = webService;
        }

        public FetcherWebResponse Toss(Uri url)
        {
            if (url == null) throw new NullReferenceException("Url");

            FetcherWebResponse result = CreateFetcherWebResponseError(new Exception("Toss error"));
            FetcherWebRequest request = new FetcherWebRequest()
            {
                Method = "POST",
                Url = url
            };

            try
            {
                var policy = Policy
                .HandleResult<FetcherWebResponse>(r => r.IsSuccess == false)
                .WaitAndRetry(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                return policy.Execute(() => WebService.DoPlatformRequest(url, request));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to toss to url: " + url.OriginalString);
                result = CreateFetcherWebResponseError(ex);
            }
            return result;
        }

        private static FetcherWebResponse CreateFetcherWebResponseError(Exception exception)
        {
            return new FetcherWebResponse()
            {
                IsSuccess = false,
                Error = exception,
                Body = string.Empty
            };
        }
    }
}
