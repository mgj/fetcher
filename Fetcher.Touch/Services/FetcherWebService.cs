using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Foundation;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace artm.Fetcher.Touch.Services
{
    public class FetcherWebService : IFetcherWebService
    {
        public FetcherWebResponse DoPlatformRequest(Uri uri, HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<FetcherWebResponse>();



            var mutableRequest = new NSMutableUrlRequest(uri);

            PrepareMethod(request, mutableRequest);
            PrepareHeaders(request, mutableRequest);

            NSUrlSessionDataTask task = CreateUrlSessionDataTask(tcs, mutableRequest);
            task.Resume();

            return tcs.Task.Result;
        }

        private void PrepareHeaders(HttpWebRequest request, NSMutableUrlRequest mutableRequest)
        {
            if (request == null || request.Headers == null || request.Headers.AllKeys == null) return;

            var dictionary = new System.Collections.Generic.Dictionary<string, string>();

            for (int i = 0; i < request.Headers.AllKeys.Length; i++)
            {
                var headKey = request.Headers.GetKey(i);
                var headValues = request.Headers.GetValues(i);
                var headValue = string.Empty;
                for (int j = 0; j < headValues.Length; j++)
                {
                    headValue += headValues[j];
                }
                dictionary.Add(headKey, headValue);
            }

            mutableRequest.Headers = NSDictionary.FromObjectsAndKeys(dictionary?.Values?.ToArray()
                                               , dictionary?.Keys?.ToArray());
        }

        private void PrepareMethod(HttpWebRequest request, NSMutableUrlRequest mutableRequest)
        {
            if (request == null || mutableRequest == null) return;

            mutableRequest.HttpMethod = request.Method;
        }

        public FetcherWebResponse DoPlatformWebRequest(Uri uri)
        {
            var tcs = new TaskCompletionSource<FetcherWebResponse>();

            var request = new NSMutableUrlRequest(uri);
            request.HttpMethod = "GET";
            NSUrlSessionDataTask task = CreateUrlSessionDataTask(tcs, request);

            task.Resume();
            return tcs.Task.Result;
        }

        private static NSUrlSessionDataTask CreateUrlSessionDataTask(TaskCompletionSource<FetcherWebResponse> tcs, NSMutableUrlRequest request)
        {
            return NSUrlSession.SharedSession.CreateDataTask(request,
                            (data, response, error) =>
                            {
                                var resp = response as NSHttpUrlResponse;
                                tcs.SetResult(new FetcherWebResponse()
                                {
                                    IsSuccess = error == null,
                                    Error = new Exception(error?.ToString()),
                                    Body = data?.ToString()
                                });
                            });
        }
    }
}
