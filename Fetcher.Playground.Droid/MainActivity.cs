using Android.App;
using Android.Widget;
using Android.OS;
using artm.Fetcher.Droid.Services;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Entities;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Net;
using artm.Fetcher.Core.Models;
using SQLite.Net;
using SQLite.Net.Platform.XamarinAndroid;
using System.Collections.Generic;

namespace Fetcher.Playground.Droid
{
    [Activity(Label = "Fetcher.Playground.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private IFetcherRepositoryStoragePathService _path;
        private IFetcherRepositoryService _repository;
        private IFetcherWebService _web;
        private IFetcherService _fetcher;
        private FetcherLoggerService _logger;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            PrepareFetcher();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Task.Run(() => DoFetch());
        }

        private void PrepareFetcher()
        {
            _logger = new FetcherLoggerService();
            _path = new FetcherRepositoryStoragePathService();
            _repository = new FetcherRepositoryService(() => CreateConnection(_path));
            _web = new FetcherWebService();
            _fetcher = new FetcherService(_web, _repository, _logger);
        }

        private static SQLiteConnectionWithLock CreateConnection(IFetcherRepositoryStoragePathService path)
        {
            var str = new SQLiteConnectionString(path.GetPath(), false);
            return new SQLiteConnectionWithLock(new SQLitePlatformAndroid(), str);
        }

        private async Task DoFetch()
        {
            await ((FetcherRepositoryService)_repository).Initialize();
            try
            {
                var url = new System.Uri("http://requestb.in/1b9zkca1");
                //_fetcher.Preload(url, "<html>Hello world!</html>");

                IUrlCacheInfo response = await _fetcher.FetchAsync(new FetcherWebRequest()
                {
                    Url = url.OriginalString,
                    Method = "POST",
                    Headers = new Dictionary<string, string>(),
                    Body = @"[{ ""myData"": ""data"" }]",
                    ContentType = string.Empty
                }, TimeSpan.FromMilliseconds(1));
                var debug = 42;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

