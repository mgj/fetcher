using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Cirrious.FluentLayouts.Touch;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Touch.Services;
using artm.Fetcher.Core.Entities;
using System.Threading.Tasks;
using System.Net;
using artm.Fetcher.Core.Models;
using SQLite.Net;
using SQLite.Net.Platform.XamarinIOS;

namespace Fetcher.Playground.Touch.Views
{
    public class FirstViewController : UIViewController
    {
        private IFetcherRepositoryStoragePathService _path;
        private IFetcherRepositoryService _repository;
        private IFetcherWebService _web;
        private IFetcherService _fetcher;

        public FirstViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;

            PrepareFetcher();

            var label = PrepareDebugLabel();
            var button = PrepareDebugButton();

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.AddConstraints(
                label.AtTopOf(View),
                label.WithSameWidth(View),
                label.Height().EqualTo(50)
                );

            Task.Run(() => DoFetch());
            //Task.Run(() => DoToss());
        }

        private void PrepareFetcher()
        {
            _path = new FetcherRepositoryStoragePathService();
            _repository = new FetcherRepositoryService(() => CreateConnection(_path));
            _web = new FetcherWebService();
            _fetcher = new FetcherService(_web, _repository);
        }

        private static SQLiteConnectionWithLock CreateConnection(IFetcherRepositoryStoragePathService path)
        {
            var str = new SQLiteConnectionString(path.GetPath(), false);
            return new SQLiteConnectionWithLock(new SQLitePlatformIOS(), str);
        }

        private async Task DoFetch()
        {
            await ((FetcherRepositoryService)_repository).Initialize();

            var url = new System.Uri("http://requestb.in/1b9zkca1");
            //_fetcher.Preload(url, "<html>Hello world!</html>");
            try
            {

                IUrlCacheInfo response = await _fetcher.FetchAsync(new FetcherWebRequest()
                {
                    Url = url,
                    Method = "POST",
                    Headers = new Dictionary<string, string>(),
                    Body = @"[{ ""myData"": ""data"" }]",
                    ContentType = string.Empty
                }, TimeSpan.FromDays(1));
                var debug = 42;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private UIButton PrepareDebugButton()
        {
            var result = new UIButton();
            result.TouchUpInside += Button_TouchUpInside;

            Add(result);
            return result;
        }

        private void Button_TouchUpInside(object sender, EventArgs e)
        {
            
        }

        private UILabel PrepareDebugLabel()
        {
            var result = new UILabel()
            {
                Text = "Hello World"
            };

            Add(result);
            return result;
        }
    }
}
