using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Policies;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Common;
using artm.Fetcher.Core.Tests.Services.Stubs;
using Moq;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherServiceStub : FetcherService
    {
        public FetcherServiceStub() : base(null, new FetcherRepositoryServiceStub(), Mock.Of<IFetcherLoggerService>())
        {
            WebServiceMock = FetcherWebServiceMockFactory.IFetcherWebServiceInternetOn();
            WebService = WebServiceMock.Object;
        }

        public FetcherServiceStub(IFetcherRepositoryService repository) : base(null, repository, Mock.Of<IFetcherLoggerService>())
        {
            WebServiceMock = FetcherWebServiceMockFactory.IFetcherWebServiceInternetOn();
            WebService = WebServiceMock.Object;
        }

        public FetcherServiceStub(IFetcherRepositoryService repository, IFetcherWebService web) : base(web, repository, Mock.Of<IFetcherLoggerService>())
        {
        }

        public FetcherServiceStub(Mock<IFetcherWebService> web) : base(web.Object, new FetcherRepositoryServiceStub(), Mock.Of<IFetcherLoggerService>())
        {
            WebServiceMock = web;
            WebService = WebServiceMock.Object;
        }

        public FetcherServiceStub(Mock<IFetcherWebService> web, IFetcherCachePolicy cachePolicy) : base(web.Object, new FetcherRepositoryServiceStub(), Mock.Of<IFetcherLoggerService>(), cachePolicy)
        {
            WebServiceMock = web;
            WebService = WebServiceMock.Object;
        }

        public FetcherServiceStub(Mock<IFetcherRepositoryService> repository) : base(null, repository.Object, Mock.Of<IFetcherLoggerService>())
        {
            WebServiceMock = FetcherWebServiceMockFactory.IFetcherWebServiceInternetOn();
            WebService = WebServiceMock.Object;

            RepositoryMock = repository;
        }

        public IFetcherRepositoryService RepositoryService
        {
            get
            {
                return Repository;
            }
            set
            {
                Repository = value;
            }
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
