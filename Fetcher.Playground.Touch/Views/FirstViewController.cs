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
        private FetcherLoggerService _logger;

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
            return new SQLiteConnectionWithLock(new SQLitePlatformIOS(), str);
        }

        private async Task DoFetch()
        {
            await ((FetcherRepositoryService)_repository).Initialize();

            var url = new System.Uri("https://services.coop.dk/restgrundsortiment/api/Vare/24003");
            //_fetcher.Preload(url, "<html>Hello world!</html>");
            try
            {

                IUrlCacheInfo response = await _fetcher.FetchAsync(url);
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
