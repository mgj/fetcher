using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherServiceMock : FetcherService
    {
        public FetcherServiceMock() : base(null, GetRepositoryService())
        {
            SetWebserviceDummy();
        }

        private void SetWebserviceDummy()
        {
            WebServiceMock = new Mock<IFetcherWebService>();
            WebServiceMock.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Returns(() => new FetcherWebResponse() { IsSuccess = true, Body = "myBody" });
            base.Webservice = WebServiceMock.Object;
        }

        public FetcherServiceMock(IFetcherRepositoryService repository) : base(null, repository)
        {
            SetWebserviceDummy();
        }

        private static IFetcherRepositoryService GetRepositoryService()
        {
            var repository = new FetcherRepositoryServiceMock();
            return repository;
        }

        public Mock<IFetcherWebService> WebServiceMock
        {
            get;
            set;
        }
    }
}
