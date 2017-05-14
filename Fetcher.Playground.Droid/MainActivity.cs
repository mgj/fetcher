﻿using Android.App;
using Android.Widget;
using Android.OS;
using artm.Fetcher.Droid.Services;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Entities;
using System.Threading.Tasks;
using System.Diagnostics;
using artm.Fetcher.Core.Services.Tosser;
using System;
using System.Net;

namespace Fetcher.Playground.Droid
{
    [Activity(Label = "Fetcher.Playground.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private FetcherRepositoryStoragePathService _path;
        private FetcherRepositoryService _repository;
        private FetcherWebService _web;
        private FetcherService _fetcher;
        private TosserService _tosser;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            PrepareFetcher();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //DoFetch();
            Task.Run(() => DoToss());
        }

        private async Task DoToss()
        {
            var url = new Uri("http://requestb.in/1mjfqsz1");

            try
            {
                var response = _tosser.Toss(url);
                var debug = 42;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void PrepareFetcher()
        {
            _path = new FetcherRepositoryStoragePathService();
            _repository = new FetcherRepositoryService(_path);
            _web = new FetcherWebService();
            _fetcher = new FetcherService(_web, _repository);
            _tosser = new TosserService(_web);
        }

        private async Task DoFetch()
        {
            try
            {
                var url = new System.Uri("https://www.google.com");
                //_fetcher.Preload(url, "<html>Hello world!</html>");

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

