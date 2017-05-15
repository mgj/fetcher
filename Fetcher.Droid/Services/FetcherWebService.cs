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

        public static readonly MediaType JSON = MediaType.Parse("application/json; charset=utf-8");

        protected OkHttpClient Client
        {
            get
            {
                if (_client == null) _client = new OkHttpClient();
                return _client;
            }
        }

        //public override FetcherWebResponse DoPlatformWebRequest(Uri uri)
        //{
        //    Request request = null;
        //    Response response = null;

        //    try
        //    {
        //        request = new Request.Builder().Url(uri.OriginalString).Build();
        //        response = Client.NewCall(request).Execute();
        //    }
        //    catch (Exception ex)
        //    {
        //        return CreateFetcherWebResponseError(ex);
        //    }

        //    if (response == null)
        //    {
        //        return CreateFetcherWebResponseError("Could not get a response, even though nothing exceptional happened");
        //    }
        //    else
        //    {
        //        return new FetcherWebResponse()
        //        {
        //            HttpStatusCode = response.Code(),
        //            Error = new Exception(response.Message()),
        //            Body = response.Body().String()
        //        };
        //    }
        //}

        private static FetcherWebResponse CreateFetcherWebResponseError(string message)
        {
            return CreateFetcherWebResponseError(new Exception(message));
        }

        private static FetcherWebResponse CreateFetcherWebResponseError(Exception exception)
        {
            return new FetcherWebResponse()
            {
                HttpStatusCode = 500,
                Error = exception,
                Body = string.Empty
            };
        }

        public override FetcherWebResponse DoPlatformRequest(FetcherWebRequest request)
        {
            var requestBuilder = new Request.Builder();
            requestBuilder.Url(request.Url.OriginalString);

            PrepareMethod(request, requestBuilder);

            _headerBuilder = new Headers.Builder();
            PrepareHeaders(request);
            requestBuilder.Headers(_headerBuilder.Build());
            PrepareBody(request, requestBuilder);

            try
            {
                var response = Client.NewCall(requestBuilder.Build()).Execute();
                return new FetcherWebResponse()
                {
                    HttpStatusCode = response.Code(),
                    Body = response.Body().String()
                };
            }
            catch (Exception ex)
            {
                return CreateFetcherWebResponseError(ex);
            }
        }

        private void PrepareBody(FetcherWebRequest request, Request.Builder requestBuilder)
        {
            if (request == null || requestBuilder == null || request.Body == null) return;

            RequestBody body = RequestBody.Create(JSON, request.Body);
            requestBuilder.Post(body);
        }

        private static void PrepareMethod(FetcherWebRequest request, Request.Builder builder)
        {
            if (request == null || builder == null) return;
            MediaType contentType = PrepareContentType(request);

            var requestBody = RequestBody.Create(contentType, new byte[0]);
            builder.Method(request.Method, requestBody);
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