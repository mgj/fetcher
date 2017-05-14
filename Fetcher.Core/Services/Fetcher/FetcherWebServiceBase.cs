using artm.Fetcher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services.Fetcher
{
    public abstract class FetcherWebServiceBase : IFetcherWebService
    {
        protected virtual void PrepareHeaders(FetcherWebRequest request)
        {
            if (request == null || request.Headers == null || request.Headers == null) return;

            for (int i = 0; i < request.Headers.Count; i++)
            {
                var item = request.Headers.ElementAt(i);
                var itemKey = item.Key;
                var itemValue = item.Value;

                AddHeader(itemKey, itemValue);
            }
        }

        protected abstract void AddHeader(string key, string value);

        public abstract FetcherWebResponse DoPlatformWebRequest(Uri uri);

        public abstract FetcherWebResponse DoPlatformRequest(Uri uri, FetcherWebRequest request);
    }
}
