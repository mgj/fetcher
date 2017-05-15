using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Services.Tosser;
using artm.Fetcher.Core.Tests.Services.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Tosser
{
    [TestFixture]
    public class TosserServiceTests
    {
        private readonly Uri URL = new Uri("https://www.google.com");

        [Test]
        public void Toss_Sunshine_IsSuccess()
        {
            var sut = new TosserService(FetcherMockFactory.IFetcherWebServiceInternetOn().Object);
            var response = sut.Toss(new FetcherWebRequest() { Url = URL });

            Assert.IsTrue(response.IsSuccess);
        }
    }
}
