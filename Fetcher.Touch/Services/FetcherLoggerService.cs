using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using artm.Fetcher.Core.Services;
using System.Diagnostics;

namespace artm.Fetcher.Touch.Services
{
    public class FetcherLoggerService : IFetcherLoggerService
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}