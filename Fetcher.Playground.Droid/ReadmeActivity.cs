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
using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
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
            // Instantiate these on Android / iOS
            IFetcherLoggerService loggerService = new FetcherLoggerService();
            IFetcherRepositoryStoragePathService path = new FetcherRepositoryStoragePathService();
            IFetcherWebService web = new FetcherWebService();
            FetcherRepositoryService repository = new FetcherRepositoryService(loggerService, path);
            
            // This call will be removed in a future version but for now you still have to call
            await repository.Initialize();

            // Primary interface you should use from your Core/PCL project
            IFetcherService fetcher = new FetcherService(web, repository, loggerService);

            string url = "https://www.google.com";

            // (Optional) Cold start: You can ship with preloaded data, and thus avoid
            // an initial requirement for an active internet connection
            await fetcher.PreloadAsync(new FetcherWebRequest
            {
                Url = url
            }, new FetcherWebResponse
            {
                HttpStatusCode = 200,
                Body = "<html>Hello world!</html>" }
            );

            // Try our hardest to give you *some* response for a given url. 
            // If an url has been recently created or updated we get the response from the local cache.
            // If an url has NOT recently been created or updated we try to update 
            // the response from the network. 
            // If we cannot get the url from the network, and no cached data is available, we try to use preloaded data.
            IUrlCacheInfo response = await fetcher.FetchAsync(new Uri(url));

            // Dont like HTTP GET? No problem!
            IUrlCacheInfo response2 = await fetcher.FetchAsync(new FetcherWebRequest()
            {
                Url = url,
                Method = "POST",
                Headers = new Dictionary<string, string>(),
                Body = @"[{ ""myData"": ""data"" }]",
                ContentType = string.Empty
            }, TimeSpan.FromDays(1));
        }
    }
}