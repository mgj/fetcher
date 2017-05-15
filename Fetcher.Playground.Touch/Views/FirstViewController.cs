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
using artm.Fetcher.Core.Models;

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
            Task.Run(() => DoToss());
        }

        private void DoToss()
        {
            var url = new Uri("http://requestb.in/1mjfqsz1");
            try
            {
                var response = _tosser.Toss(new FetcherWebRequest() { Url = url });
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void PrepareFetcher()
        {
            var web = new FetcherWebService();
            var tosser = new TosserService(web);
            var response = tosser.Toss(new FetcherWebRequest() { Url = new Uri("http://requestb.in/1mjfqsz1") });

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
