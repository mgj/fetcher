﻿using Android.App;
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
using Android.Graphics;

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
        private ImageView _image;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            PrepareFetcher();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            PrepareImage();

            Task.Run(() => DoFetch());
        }

        private void PrepareImage()
        {
            _image = FindViewById<ImageView>(Resource.Id.my_imageview);

        }

        private void PrepareFetcher()
        {
            _logger = new FetcherLoggerService();
            _path = new FetcherRepositoryStoragePathService();
            _repository = new FetcherRepositoryService(_logger, () => CreateConnection(_path));
            _web = new FetcherWebService();
            _fetcher = new FetcherService(_web, _repository, _logger);
        }

        private static SQLiteConnectionWithLock CreateConnection(IFetcherRepositoryStoragePathService path)
        {
            var str = new SQLiteConnectionString(path.GetPath(), false);
            return new SQLiteConnectionWithLock(new SQLitePlatformAndroidN(), str);
        }

        private async Task DoFetch()
        {
            await ((FetcherRepositoryService)_repository).Initialize();
            try
            {
                var urlCacheInfo = await _fetcher.FetchAsync(new FetcherWebRequest()
                {
                    Url = "https://jsonplaceholder.typicode.com/posts",
                    Method = "POST",
                    Body = string.Empty
                }, TimeSpan.FromTicks(1));

                var url = new System.Uri("https://lorempixel.com/200/400/");
                IUrlCacheInfo response = await _fetcher.FetchAsync(url);
                var bitmap = BitmapFactory.DecodeByteArray(response.FetcherWebResponse.BodyAsBytes, 0, response.FetcherWebResponse.BodyAsBytes.Length);

                RunOnUiThread(() => _image.SetImageBitmap(bitmap));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

