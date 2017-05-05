using artm.Fetcher.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Mocks
{
    public static class FetcherServiceFactory
    {
        public static FetcherRepositoryService FetcherRepositoryService()
        {
            return new FetcherRepositoryService(FetcherRepositoryStoragePathService());
        }

        public static IFetcherRepositoryStoragePathService FetcherRepositoryStoragePathService()
        {
            var result = new Mock<IFetcherRepositoryStoragePathService>();

            result.Setup(x => x.GetPath(It.IsAny<string>())).Returns(() => ":memory:");

            return result.Object;
        }
    }
}
