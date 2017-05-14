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
            PrepareMethod(request, builder);
            PrepareHeaders(request, builder);

            try
            {
                var response = Client.NewCall(builder.Build()).Execute();
                return new FetcherWebResponse()
                {
                    IsSuccess = response.IsSuccessful,
                    Body = response.Body().String()
                };
            }
            catch (Exception ex)
            {
                return CreateFetcherWebResponseError();
            }
        }

        private static void PrepareMethod(HttpWebRequest request, Request.Builder builder)
        {
            if (request == null || builder == null) return;

            MediaType contentType = null;
            if (request.ContentType != null)
            {
                contentType = MediaType.Parse(request.ContentType);
            }

            var requestBody = RequestBody.Create(contentType, new byte[0]);
            switch (request.Method)
            {
                case "GET":
                    builder.Method("GET", requestBody);
                    break;
                case "POST":
                    builder.Method("POST", requestBody);
                    break;
                default:
                    break;
            }
        }

        private static void PrepareHeaders(HttpWebRequest request, Request.Builder builder)
        {
            if (request == null || request.Headers == null || request.Headers.AllKeys == null) return;

            var headerBuilder = new Headers.Builder();
            for (int i = 0; i < request?.Headers?.AllKeys?.Length; i++)
            {
                var headKey = request.Headers.GetKey(i);
                var headValues = request.Headers.GetValues(i);
                var headValue = string.Empty;
                for (int j = 0; j < headValues.Length; j++)
                {
                    headValue += headValues[j];

                }
                headerBuilder.Add(headKey, headValue);
            }
            builder.Headers(headerBuilder.Build());
        }
    }
}