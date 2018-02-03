using System;
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