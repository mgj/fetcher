using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Moq;
using System;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherServiceMock : FetcherService
    {
        public FetcherServiceMock() :base(null, new FetcherRepositoryService(GetPathServiceMemory()))
        {
            SetWebserviceDummy();
        }

        public FetcherServiceMock(IFetcherRepositoryService repository) : base(null, repository)
        {
            SetWebserviceDummy();
        }

        public FetcherServiceMock(Mock<IFetcherWebService> web) : base(web.Object, new FetcherRepositoryService(GetPathServiceMemory()))
        {
            WebServiceMock = web;
            base.Webservice = WebServiceMock.Object;
        }

        public FetcherServiceMock(Mock<IFetcherRepositoryService> repository) : base(null, repository.Object)
        {
            SetWebserviceDummy();

            RepositoryMock = repository;
        }

        public static IFetcherRepositoryStoragePathService GetPathServiceMemory()
        {
            var mock = new Mock<IFetcherRepositoryStoragePathService>();
            mock.Setup(x => x.GetPath(It.IsAny<string>())).Returns(() => ":memory:");
            return mock.Object;
        }

        private void SetWebserviceDummy()
        {
            WebServiceMock = new Mock<IFetcherWebService>();
            WebServiceMock.Setup(x => x.DoPlatformWebRequest(It.IsAny<Uri>())).Returns(() => new FetcherWebResponse() { IsSuccess = true, Body = "Default Test Body" });
            base.Webservice = WebServiceMock.Object;
        }

        public Mock<IFetcherWebService> WebServiceMock
        {
            get;
            private set;
        }

        public Mock<IFetcherRepositoryService> RepositoryMock
        {
            get;
            private set;
        }
    }
}
