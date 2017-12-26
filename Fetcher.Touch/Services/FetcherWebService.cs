using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Foundation;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using artm.Fetcher.Core.Services.Fetcher;
using artm.Fetcher.Core.Entities;
using System.Text;

namespace artm.Fetcher.Touch.Services
{
    public class FetcherWebService : FetcherWebServiceBase, IFetcherWebService
    {
        private NSMutableUrlRequest _mutableRequest;

        public override IFetcherWebResponse DoPlatformRequest(IFetcherWebRequest request)
        {
            var tcs = new TaskCompletionSource<IFetcherWebResponse>();

            _mutableRequest = new NSMutableUrlRequest(new Uri(request.Url));

            PrepareContentType(request);
            PrepareMethod(request);
            PrepareHeaders(request);
            PrepareBody(request);


            NSUrlSessionDataTask task = CreateUrlSessionDataTask(tcs, _mutableRequest);
            task.Resume();

            return tcs.Task.Result;
        }

        private void PrepareBody(IFetcherWebRequest request)
        {
            if(string.IsNullOrEmpty(request.Body) == false)
            {
                _mutableRequest.Body = request.Body;
            }
        }

        protected override void AddHeader(string key, string value)
        {
            NSMutableDictionary headers = new NSMutableDictionary (_mutableRequest.Headers);
            headers.SetValueForKey(new NSString(value),  new NSString(key));
            _mutableRequest.Headers = headers;
        }

        private void PrepareMethod(IFetcherWebRequest request)
        {
            if (request == null || _mutableRequest == null) return;

            if (string.IsNullOrEmpty(request.Method)) request.Method = "GET";
            _mutableRequest.HttpMethod = request.Method;
        }

        private void PrepareContentType(IFetcherWebRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ContentType)) return;

            if (string.IsNullOrEmpty(request.ContentType))
            {
                AddHeader("content-type", DEFAULT_CONTENTTYPE);
            }
            else
            {
                AddHeader("content-type", request.ContentType);
            }
        }

        private static NSUrlSessionDataTask CreateUrlSessionDataTask(TaskCompletionSource<IFetcherWebResponse> tcs, NSMutableUrlRequest request)
        {
            return NSUrlSession.SharedSession.CreateDataTask(request,
                            (data, response, error) =>
                            {
                                var resp = response as NSHttpUrlResponse;

                                if (error != null)
                                {
                                    tcs.SetException(new Exception(error.ToString()));
                                }
                                else
                                {
                                    byte[] bodyBytes = new byte[data.Length];
                                    if(data != null && data.Bytes != null && data.Count() > 0)
                                    {
                                        System.Runtime.InteropServices.Marshal.Copy(data.Bytes, bodyBytes, 0, Convert.ToInt32(data.Length));
                                    }
                                    
                                    string bodyString = Encoding.UTF8.GetString(bodyBytes);
                                    tcs.SetResult(new FetcherWebResponse()
                                    {
                                        HttpStatusCode = (int)resp?.StatusCode,
                                        Error = new Exception(error?.ToString()),
                                        Body = bodyString,
                                        BodyAsBytes = bodyBytes,
                                        ContentType = response.MimeType
                                    });
                                }
                            });
        }
    }
}
