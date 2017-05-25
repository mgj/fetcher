using System;
using artm.Fetcher.Core.Services;
using Square.OkHttp;
using artm.Fetcher.Core.Models;
using System.Net;
using System.Linq;
using artm.Fetcher.Core.Services.Fetcher;
using artm.Fetcher.Core.Entities;

namespace artm.Fetcher.Droid.Services
{
    public class FetcherWebService : FetcherWebServiceBase, IFetcherWebService
    {
        private OkHttpClient _client;
        private Headers.Builder _headerBuilder;
        public const string BODY_FORMAT = "application/json; charset=utf-8";

        protected OkHttpClient Client
        {
            get
            {
                if (_client == null) _client = new OkHttpClient();
                return _client;
            }
        }

        private static IFetcherWebResponse CreateFetcherWebResponseError(string message)
        {
            return CreateFetcherWebResponseError(new Exception(message));
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

        public override IFetcherWebResponse DoPlatformRequest(IFetcherWebRequest request)
        {
            var requestBuilder = new Request.Builder();
            requestBuilder.Url(request.Url);

            _headerBuilder = new Headers.Builder();
            PrepareHeaders(request);
            requestBuilder.Headers(_headerBuilder.Build());

            PrepareBody(request, requestBuilder);

            try
            {
                var response = Client.NewCall(requestBuilder.Build()).Execute();
                if(response == null) return CreateFetcherWebResponseError(new Exception("DoPlatformRequest failed"));


                ResponseBody responseBody = response.Body();
                var source = responseBody.Source();
                source.Request(Java.Lang.Long.MaxValue); // Buffer the entire body.
                var buffer = source.Buffer;

                var body = buffer.Clone().ReadString(Java.Nio.Charset.Charset.ForName("UTF-8"));
                var bodyAsBytes = buffer.Clone().ReadByteArray();
                return new FetcherWebResponse()
                {
                    HttpStatusCode = response.Code(),
                    Error = new Exception(response.Message()),
                    Body = body,
                    BodyAsBytes = bodyAsBytes
                };
            }
            catch (Exception ex)
            {
                return CreateFetcherWebResponseError(ex);
            }
        }

        private ResponseBody CloneResponseBody(Response rawResponse)
        {
            ResponseBody responseBody = rawResponse.Body();

            var bufferClone = responseBody.Source().Buffer.Clone();
            return ResponseBody.Create(responseBody.ContentType(), responseBody.ContentLength(), bufferClone);
        }

        private void PrepareBody(IFetcherWebRequest request, Request.Builder requestBuilder)
        {
            if (request == null || requestBuilder == null ) return;

            RequestBody body = null;
            if (string.IsNullOrEmpty(request.Body) == false)
            {
                body = RequestBody.Create(MediaType.Parse(BODY_FORMAT), request.Body);
            }

            requestBuilder.Method(request.Method, body);
        }

        private static MediaType PrepareContentType(IFetcherWebRequest request)
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