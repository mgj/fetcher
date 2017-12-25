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
using Foundation;
using SQLite;

namespace Fetcher.Playground.Touch.Views
{
    public class FirstViewController : UIViewController
    {
        private IFetcherRepositoryStoragePathService _path;
        private IFetcherRepositoryService _repository;
        private IFetcherWebService _web;
        private IFetcherService _fetcher;
        private FetcherLoggerService _logger;
        private UIImageView _image;

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
            _image = PrepareImage();

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.AddConstraints(
                _image.AtTopOf(View),
                _image.Width().EqualTo(200),
                _image.Height().EqualTo(200)
                );



            Task.Run(() => DoFetch());
        }

        private void PrepareFetcher()
        {
            _logger = new FetcherLoggerService();
            _path = new FetcherRepositoryStoragePathService();
            _repository = new FetcherRepositoryService(_logger, _path);
            _web = new FetcherWebService();
            _fetcher = new FetcherService(_web, _repository, _logger);
        }

        private UIImageView PrepareImage()
        {
            var image = new UIImageView();
            Add(image);
            return image;
            
        }

        private async Task DoFetch()
        {
            await ((FetcherRepositoryService)_repository).Initialize();

            //_fetcher.Preload(url, "<html>Hello world!</html>");
            try
            {
                var url = "https://www.google.com";
                IUrlCacheInfo response = await _fetcher.FetchAsync(new FetcherWebRequest()
                {
                    Url = url,
                    Method = "DELETE",
                    Body = string.Empty
                }, TimeSpan.FromMilliseconds(1));

                InvokeOnMainThread(() => {
                    using (var data = NSData.FromArray(response.FetcherWebResponse.BodyAsBytes))
                    {
                        _image.Image = UIImage.LoadFromData(data);

                    }
                });

                var debug = 42;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
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
