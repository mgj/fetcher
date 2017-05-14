using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Cirrious.FluentLayouts.Touch;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Touch.Services;
using artm.Fetcher.Core.Entities;
using System.Threading.Tasks;
using artm.Fetcher.Core.Services.Tosser;
using System.Net;

namespace Fetcher.Playground.Touch.Views
{
    public class FirstViewController : UIViewController
    {
        private IFetcherRepositoryStoragePathService _path;
        private IFetcherRepositoryService _repository;
        private IFetcherWebService _web;
        private IFetcherService _fetcher;
        private TosserService _tosser;

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

            //DoFetch();
            DoToss();
        }

        private async Task DoToss()
        {
            var url = new Uri("http://requestb.in/1mjfqsz1");

            var request = new HttpWebRequest(url);
            request.Method = "POST";

            try
            {
                var response = _tosser.Toss(url, request);
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
            var url = new System.Uri("https://www.google.com");
            _fetcher.Preload(url, "<html>Hello world!</html>");

            IUrlCacheInfo response = await _fetcher.Fetch(url);
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
