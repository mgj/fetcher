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
using artm.Fetcher.Core.Services;

namespace artm.Fetcher.Droid.Services
{
    public class FetcherLoggerService : IFetcherLoggerService
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}