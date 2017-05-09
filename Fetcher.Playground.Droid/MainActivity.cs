using Android.App;
using Android.Widget;
using Android.OS;
using artm.Fetcher.Droid.Services;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Entities;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Fetcher.Playground.Droid
{
    [Activity(Label = "Fetcher.Playground.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private FetcherRepositoryStoragePathService _path;
        private FetcherRepositoryService _repository;
        private FetcherWebService _web;
        private FetcherService _fetcher;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            PrepareFetcher();
        }

        private async Task PrepareFetcher()
        {
            try
            {
                _path = new FetcherRepositoryStoragePathService();
                _repository = new FetcherRepositoryService(_path);
                _web = new FetcherWebService();

                _fetcher = new FetcherService(_web, _repository);

                var url = new System.Uri("https://www.google.com");
                _fetcher.Preload(url, "<html>Hello world!</html>");

                IUrlCacheInfo response = await _fetcher.Fetch(url);
                var debug = 42;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}

