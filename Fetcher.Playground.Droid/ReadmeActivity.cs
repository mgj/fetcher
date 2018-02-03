using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Policies;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Droid.Services;

namespace Fetcher.Playground.Droid
{
    [Activity(Label = "ReadmeActivity")]
    public class ReadmeActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            Init();
        }

        private async Task Init()
        {
            // 1. Instantiate these on Android / iOS. If using dependency injection, you can register them as singletons.
            IFetcherLoggerService loggerService = new FetcherLoggerService();
            IFetcherRepositoryStoragePathService path = new FetcherRepositoryStoragePathService();
            IFetcherWebService webService = new FetcherWebService();
            IFetcherRepositoryService repository = new FetcherRepositoryService(loggerService, path);

            // 2. Initialize the repository. This creates the underlying database and is safe to call on every app startup
            await ((FetcherRepositoryService)repository).Initialize();

            // 3. Primary interface you should use from your Core/PCL project
            IFetcherService fetcher = new FetcherService(webService, repository, loggerService);

            // --------------------------------------------------------------------

            String url = "https://www.google.com";

            // Try our hardest to give you *some* response for a given url. 
            // If an url has been recently created or updated we get the response from the local cache.
            // If an url has NOT recently been created or updated we try to update 
            // the response from the network. 
            // If we cannot get the url from the network, and no cached data is available, we try to use preloaded data.
            IUrlCacheInfo response = await fetcher.FetchAsync(new Uri(url));

            // (Optional) Cold start: You can ship with preloaded data, and thus avoid
            // an initial requirement for an active internet connection
            await fetcher.PreloadAsync(new FetcherWebRequest()
            {
                Url = url
            }, new FetcherWebResponse() { Body = "<html>Hello world!</html>" });

            // Dont like HTTP GET? No problem!
            IUrlCacheInfo postResponse = await fetcher.FetchAsync(new FetcherWebRequest()
            {
                Url = url,
                Method = "POST",
                Headers = new System.Collections.Generic.Dictionary<string, string>(),
                Body = @"[{ ""myData"": ""data"" }]",
                ContentType = "application/json; charset=utf-8"
            }, TimeSpan.FromDays(14));

            // --------------------------------------------------------------------

            IFetcherService fetcher2 = new FetcherService(webService, repository, loggerService, new AlwaysCachePolicy());

            // --------------------------------------------------------------------

            // Search cached entries by Url...
            IEnumerable<IUrlCacheInfo> infos = await repository.GetAllUrlCacheInfo();
            IEnumerable<IFetcherWebRequest> request = await repository.GetAllWebRequests();
            IEnumerable<IFetcherWebResponse> responses = await repository.GetAllWebResponses();


            IEnumerable<IUrlCacheInfo> info = await repository.GetUrlCacheInfoForRequest(new FetcherWebRequest
            {
                Url = "https://www.google.com"
            });

            // ... or anything else
            IEnumerable<IUrlCacheInfo> info2 = await repository.GetUrlCacheInfoForRequest(new FetcherWebRequest
            {
                Url = "https://www.google.com",
                Method = "POST",
                Headers = new Dictionary<string, string>
                {
                    {"X-ZUMO-APPLICATION" , "Hello world!" }
                }
            });

            IUrlCacheInfo info3 = await repository.GetUrlCacheInfoForId(15);
            
        }
    }
}