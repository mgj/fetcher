using artm.Fetcher.Core.Services;
using artm.Fetcher.Core.Tests.Services.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Tests.Services.Stubs
{
    public class FetcherRepositoryServiceStub : FetcherRepositoryService
    {
        public FetcherRepositoryServiceStub() : base(FetcherMockFactory.IFetcherRepositoryStoragePathServiceMemory().Object)
        {
        }

        public SQLiteConnection DatabaseConnection
        {
            get
            {
                return Db;
            }
        }
    }
}
