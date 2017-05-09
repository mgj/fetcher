using artm.Fetcher.Core.Services;
using Foundation;
using System;
using System.IO;

namespace artm.Fetcher.Touch.Services
{
    public class FetcherRepositoryStoragePathService : IFetcherRepositoryStoragePathService
    {
        public string GetPath(string filename = "fetcher.db3")
        {
            //var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //var cache = Path.Combine(documents, "..", "Library", "Caches");
            //var fullPath = Path.Combine(cache, filename);

            var fullPath = Path.Combine(Path.GetTempPath(), filename);
            if (File.Exists(fullPath))
            {
                NSFileManager.SetSkipBackupAttribute(fullPath, true);
            }

            return fullPath;
        }
    }
}
