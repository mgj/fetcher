﻿using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Foundation;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using artm.Fetcher.Core.Services.Fetcher;

namespace artm.Fetcher.Touch.Services
{
    public class FetcherWebService : FetcherWebServiceBase, IFetcherWebService
    {
        private NSMutableUrlRequest _mutableRequest;

        public override FetcherWebResponse DoPlatformRequest(Uri uri, FetcherWebRequest request)
        {
            var tcs = new TaskCompletionSource<FetcherWebResponse>();

            _mutableRequest = new NSMutableUrlRequest(uri);

            PrepareHeaders(request);
            PrepareMethod(request);

            NSUrlSessionDataTask task = CreateUrlSessionDataTask(tcs, _mutableRequest);
            task.Resume();

            return tcs.Task.Result;
        }

        protected override void AddHeader(string key, string value)
        {
            _mutableRequest.SetValueForKey(new NSString(key), new NSString(value));
        }

        private void PrepareMethod(FetcherWebRequest request)
        {
            if (request == null || _mutableRequest == null) return;

            _mutableRequest.HttpMethod = request.Method;
        }

        public override FetcherWebResponse DoPlatformWebRequest(Uri uri)
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
