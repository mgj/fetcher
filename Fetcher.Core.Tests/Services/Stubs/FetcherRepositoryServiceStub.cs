using artm.Fetcher.Core.Services;
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
        public FetcherRepositoryServiceStub(IFetcherRepositoryStoragePathService pathService) : base(pathService)
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
