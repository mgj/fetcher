using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Moq;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Common
{
    public static class FetcherWebServiceMockFactory
    {
        public static Mock<IFetcherWebService> IFetcherWebServiceInternetOn()
        {
            var mock = new Mock<IFetcherWebService>();
            mock.Setup(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>())).Returns(() => new FetcherWebResponse() { HttpStatusCode = 200, Body = "DoPlatformWebRequest Default Test Body" });
            return mock;
        }

        public static Mock<IFetcherWebService> IFetcherWebServiceInternetOff()
        {
            var mock = new Mock<IFetcherWebService>();
            mock.Setup(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>())).Throws(new Exception("IFetcherWebServiceInternetOff mock web exception"));
            return mock;
        }

        public static Mock<IFetcherWebService> IFetcherWebServiceSlowInternet(int delay)
        {
            var mock = new Mock<IFetcherWebService>();
            mock.Setup(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>()))
                .Returns(() => {
                    Task.Delay(delay).Wait();
                    return new FetcherWebResponse() { HttpStatusCode = 200, Body = "DoPlatformWebRequest Default Test Body" };
                });
            return mock;
        }

        public static Mock<IFetcherWebService> IFetcherWebServiceAlwaysHttpStatus200()
        {
            var mock = new Mock<IFetcherWebService>();
            mock.Setup(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>())).Returns(() => 
            new FetcherWebResponse()
            {
                HttpStatusCode = 200,
                Body = "DoPlatformWebRequest Http Status 200 body"
            });
            return mock;
        }

        public static Mock<IFetcherWebService> IFetcherWebServiceAlwaysHttpStatus500()
        {
            var mock = new Mock<IFetcherWebService>();
            mock.Setup(x => x.DoPlatformRequest(It.IsAny<FetcherWebRequest>())).Returns(() =>
            new FetcherWebResponse()
            {
                HttpStatusCode = 500,
                Body = "DoPlatformWebRequest Http Status 500 body"
            });
            return mock;
        }
    }
}
