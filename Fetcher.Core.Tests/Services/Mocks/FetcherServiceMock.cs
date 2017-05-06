using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Moq;
using System;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherServiceMock : FetcherService
    {
        public FetcherServiceMock() :base(null, new FetcherRepositoryServiceMock())
        {
            SetWebserviceDummy();
        }

        public FetcherServiceMock(IFetcherRepositoryService repository) : base(null, repository)
        {
            SetWebserviceDummy();
        }

        public FetcherServiceMock(Mock<IFetcherWebService> web) : base(web.Object, new FetcherRepositoryServiceMock())
        {
            WebServiceMock = web;
            base.Webservice = WebServiceMock.Object;
        }

        private void SetWebserviceDummy()
        {
            WebServiceMock = new Mock<IFetcherWebService>();
            WebServiceMock.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Returns(() => new FetcherWebResponse() { IsSuccess = true, Body = "myBody" });
            base.Webservice = WebServiceMock.Object;
        }

        public Mock<IFetcherWebService> WebServiceMock
        {
            get;
            private set;
        }
    }
}
