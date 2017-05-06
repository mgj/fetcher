using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using artm.Fetcher.Core.Services;
using Square.OkHttp;
using artm.Fetcher.Core.Models;

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
    }
}