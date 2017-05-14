using System;
using artm.Fetcher.Core.Services;
using Square.OkHttp;
using artm.Fetcher.Core.Models;
using System.Net;

namespace artm.Fetcher.Droid.Services
{
    public class FetcherWebService : IFetcherWebService
    {
        private OkHttpClient _client;

        protected OkHttpClient Client
        {
            get
            {
                if (_client == null) _client = new OkHttpClient();
                return _client;
            }
        }

        public FetcherWebResponse DoPlatformWebRequest(Uri uri)
        {
            Request request = null;
            Response response = null;

            try
            {
                request = new Request.Builder().Url(uri.OriginalString).Build();
                response = Client.NewCall(request).Execute();
            }
            catch (Exception)
            {
                return CreateFetcherWebResponseError();
            }

            if (response == null)
            {
                return CreateFetcherWebResponseError();
            }
            else
            {
                return new FetcherWebResponse()
                {
                    IsSuccess = response.IsSuccessful,
                    Body = response.Body().String()
                };
            }
        }

        private static FetcherWebResponse CreateFetcherWebResponseError()
        {
            return new FetcherWebResponse()
            {
                IsSuccess = false,
                Body = string.Empty
            };
        }

        public FetcherWebResponse DoPlatformRequest(Uri uri, HttpWebRequest request)
        {
            Request.Builder builder = new Request.Builder();
            builder.Url(uri.OriginalString);
            PrepareHeaders(request, builder);
            PrepareMethod(request, builder);

            try
            {
                var response = Client.NewCall(builder.Build()).Execute();
                return new FetcherWebResponse()
                {
                    IsSuccess = response.IsSuccessful,
                    Body = response.Body().String()
                };
            }
            catch (Exception)
            {
                return CreateFetcherWebResponseError();
            }
        }

        private static void PrepareMethod(HttpWebRequest request, Request.Builder builder)
        {
            var requestBody = RequestBody.Create(MediaType.Parse(request.ContentType), new byte[0]);
            switch (request.Method)
            {
                case "GET":
                    builder.Method("GET", requestBody)
                        .Get();
                    break;
                case "POST":
                    builder.Method("POST", requestBody)
                        .Post(requestBody);
                    break;
                default:
                    break;
            }
        }

        private static void PrepareHeaders(HttpWebRequest request, Request.Builder builder)
        {
            var headerBuilder = new Headers.Builder();

            for (int i = 0; i < request.Headers.AllKeys.Length; i++)
            {
                var headKey = request.Headers.GetKey(i);
                var headValues = request.Headers.GetValues(i);
                var headValue = string.Empty;
                for (int j = 0; j < headValues.Length; j++)
                {
                    headerBuilder.Add(headKey, headValues[j]);
                }
            }
            builder.Headers(headerBuilder.Build());
        }
    }
}