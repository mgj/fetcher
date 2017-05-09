using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Foundation;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Touch.Services
{
    public class FetcherWebService : IFetcherWebService
    {
        public FetcherWebResponse DoPlatformWebRequest(Uri uri)
        {
            var tcs = new TaskCompletionSource<FetcherWebResponse>();

            var request = new NSMutableUrlRequest(uri);
            var session = NSUrlSession.SharedSession;
            request.HttpMethod = "GET";

            var task = session.CreateDataTask(request,
                (data, response, error) =>
                {
                    var resp = response as NSHttpUrlResponse;
                    tcs.SetResult(new FetcherWebResponse()
                    {
                        IsSuccess = error == null,
                        Body = data?.ToString()
                    });
                });

            task.Resume();
            return tcs.Task.Result;
        }
    }
}
