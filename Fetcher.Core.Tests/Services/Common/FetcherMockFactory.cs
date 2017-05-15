﻿using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Common
{
    public static class FetcherMockFactory
    {
        public static Mock<IFetcherRepositoryStoragePathService> IFetcherRepositoryStoragePathServiceMemory()
        {
            var mock = new Mock<IFetcherRepositoryStoragePathService>();
            mock.Setup(x => x.GetPath(It.IsAny<string>())).Returns(() => ":memory:");
            return mock;
        }

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
    }
}
