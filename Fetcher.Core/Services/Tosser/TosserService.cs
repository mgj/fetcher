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

        public IFetcherWebResponse Toss(IFetcherWebRequest request)
        {
            if (request == null) throw new NullReferenceException("request");
            if(string.IsNullOrEmpty(request.Method) == true)
            {
                request.Method = "POST";
            }
            IFetcherWebResponse result = CreateFetcherWebResponseError(new Exception("Toss error"));
            
            try
            {
                var policy = Policy
                .HandleResult<IFetcherWebResponse>(r => r.IsSuccess == false)
                .WaitAndRetry(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                return policy.Execute(() => WebService.DoPlatformRequest(request));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to toss to url: " + request.Url.OriginalString);
                result = CreateFetcherWebResponseError(ex);
            }
            return result;
        }

        private static IFetcherWebResponse CreateFetcherWebResponseError(Exception exception)
        {
            return new FetcherWebResponse()
            {
                HttpStatusCode = 999,
                Error = exception,
                Body = string.Empty
            };
        }
    }
}
