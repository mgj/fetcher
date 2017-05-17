using artm.Fetcher.Core.Models;
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
            WebServiceMock = FetcherMockFactory.IFetcherWebServiceInternetOn();
            WebService = WebServiceMock.Object;
        }

        public FetcherServiceStub(IFetcherRepositoryService repository) : base(null, repository, Mock.Of<IFetcherLoggerService>())
        {
            WebServiceMock = FetcherMockFactory.IFetcherWebServiceInternetOn();
            WebService = WebServiceMock.Object;
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

        public FetcherServiceStub(Mock<IFetcherWebService> web) : base(web.Object, new FetcherRepositoryServiceStub(), Mock.Of<IFetcherLoggerService>())
        {
            WebServiceMock = web;
            base.WebService = WebServiceMock.Object;
        }
        
        public FetcherServiceStub(Mock<IFetcherRepositoryService> repository) : base(null, repository.Object, Mock.Of<IFetcherLoggerService>())
        {
            WebServiceMock = FetcherMockFactory.IFetcherWebServiceInternetOn();
            WebService = WebServiceMock.Object;

            RepositoryMock = repository;
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
