using System;
using artm.Fetcher.Core.Services;
using Square.OkHttp;
using artm.Fetcher.Core.Models;
using System.Net;
using System.Linq;
using artm.Fetcher.Core.Services.Fetcher;

namespace artm.Fetcher.Droid.Services
{
    public class FetcherWebService : FetcherWebServiceBase, IFetcherWebService
    {
        private OkHttpClient _client;
        private Headers.Builder _headerBuilder;

        protected OkHttpClient Client
        {
            get
            {
                if (_client == null) _client = new OkHttpClient();
                return _client;
            }
        }

        public override FetcherWebResponse DoPlatformWebRequest(Uri uri)
        {
            Request request = null;
            Response response = null;

            try
            {
                request = new Request.Builder().Url(uri.OriginalString).Build();
                response = Client.NewCall(request).Execute();
            }
            catch (Exception ex)
            {
                return CreateFetcherWebResponseError(ex);
            }

            if (response == null)
            {
                return CreateFetcherWebResponseError("Could not get a response, even though nothing exceptional happened");
            }
            else
            {
                return new FetcherWebResponse()
                {
                    IsSuccess = response.IsSuccessful,
                    Error = new Exception(response.Message()),
                    Body = response.Body().String()
                };
            }
        }

        private static FetcherWebResponse CreateFetcherWebResponseError(string message)
        {
            return CreateFetcherWebResponseError(new Exception(message));
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

        public override FetcherWebResponse DoPlatformRequest(Uri uri, FetcherWebRequest request)
        {
            var requestBuilder = new Request.Builder();
            requestBuilder.Url(uri.OriginalString);

            PrepareMethod(request, requestBuilder);

            _headerBuilder = new Headers.Builder();
            PrepareHeaders(request);
            requestBuilder.Headers(_headerBuilder.Build());

            try
            {
                var response = Client.NewCall(requestBuilder.Build()).Execute();
                return new FetcherWebResponse()
                {
                    IsSuccess = response.IsSuccessful,
                    Body = response.Body().String()
                };
            }
            catch (Exception ex)
            {
                return CreateFetcherWebResponseError(ex);
            }
        }

        private static void PrepareMethod(FetcherWebRequest request, Request.Builder builder)
        {
            if (request == null || builder == null) return;
            MediaType contentType = PrepareContentType(request);

            var requestBody = RequestBody.Create(contentType, new byte[0]);
            switch (request.Method)
            {
                case "GET":
                    builder.Method("GET", requestBody);
                    break;
                case "POST":
                    builder.Method("POST", requestBody);
                    break;
                case "HEAD":
                    builder.Method("HEAD", requestBody);
                    break;
                case "PUT":
                    builder.Method("PUT", requestBody);
                    break;
                case "DELETE":
                    builder.Method("DELETE", requestBody);
                    break;
                case "OPTIONS":
                    builder.Method("OPTIONS", requestBody);
                    break;
                case "TRACE":
                    builder.Method("TRACE", requestBody);
                    break;
                case "CONNECT":
                    builder.Method("CONNECT", requestBody);
                    break;
                default:
                    break;
            }
        }

        private static MediaType PrepareContentType(FetcherWebRequest request)
        {
            MediaType contentType = null;
            if (request != null && string.IsNullOrEmpty(request.ContentType) == false)
            {
                contentType = MediaType.Parse(request.ContentType);
            }

            return contentType;
        }

        protected override void AddHeader(string key, string value)
        {
            _headerBuilder.Add(key, value);
        }
    }
}