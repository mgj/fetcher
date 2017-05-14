using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Common;
using Moq;
using System;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public class FetcherServiceStub : FetcherService
    {
        public FetcherServiceStub() : base(null, new FetcherRepositoryService(FetcherMockFactory.IFetcherRepositoryStoragePathServiceMemory().Object))
        {
            WebServiceMock = FetcherMockFactory.IFetcherWebServiceInternetOn();
            WebService = WebServiceMock.Object;
        }

        public FetcherServiceStub(IFetcherRepositoryService repository) : base(null, repository)
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

        public FetcherServiceStub(Mock<IFetcherWebService> web) : base(web.Object, new FetcherRepositoryService(FetcherMockFactory.IFetcherRepositoryStoragePathServiceMemory().Object))
        {
            WebServiceMock = web;
            base.WebService = WebServiceMock.Object;
        }
        
        public FetcherServiceStub(Mock<IFetcherRepositoryService> repository) : base(null, repository.Object)
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
