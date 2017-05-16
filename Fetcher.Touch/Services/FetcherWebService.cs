﻿using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Foundation;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using artm.Fetcher.Core.Services.Fetcher;
using artm.Fetcher.Core.Entities;

namespace artm.Fetcher.Touch.Services
{
    public class FetcherWebService : FetcherWebServiceBase, IFetcherWebService
    {
        private NSMutableUrlRequest _mutableRequest;

        public override IFetcherWebResponse DoPlatformRequest(IFetcherWebRequest request)
        {
            var tcs = new TaskCompletionSource<IFetcherWebResponse>();

            _mutableRequest = new NSMutableUrlRequest(request.Url);

            PrepareMethod(request);
            PrepareHeaders(request);
            PrepareBody(request);

            NSUrlSessionDataTask task = CreateUrlSessionDataTask(tcs, _mutableRequest);
            task.Resume();
            return tcs.Task.Result;
        }

        private void PrepareBody(IFetcherWebRequest request)
        {
            if(string.IsNullOrEmpty(request.Body) == false)
            {
                _mutableRequest.Body = request.Body;
            }
        }

        protected override void AddHeader(string key, string value)
        {
            _mutableRequest.SetValueForKey(new NSString(key), new NSString(value));
        }

        private void PrepareMethod(IFetcherWebRequest request)
        {
            if (request == null || _mutableRequest == null) return;
            _mutableRequest.HttpMethod = request.Method;

            PrepareContentType(request);
        }

        private void PrepareContentType(IFetcherWebRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ContentType)) return;

            _mutableRequest.SetValueForKey(new NSString("content-type"), new NSString(request.ContentType));
        }

        private static NSUrlSessionDataTask CreateUrlSessionDataTask(TaskCompletionSource<IFetcherWebResponse> tcs, NSMutableUrlRequest request)
        {
            return NSUrlSession.SharedSession.CreateDataTask(request,
                            (data, response, error) =>
                            {
                                var resp = response as NSHttpUrlResponse;
                                tcs.SetResult(new FetcherWebResponse()
                                {
                                    HttpStatusCode = (int)resp?.StatusCode,
                                    Error = new Exception(error?.ToString()),
                                    Body = data?.ToString()
                                });
                            });
        }
    }
}
