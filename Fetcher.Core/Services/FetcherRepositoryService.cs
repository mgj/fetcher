using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using Polly;
using SQLite;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly.Retry;
using System.Collections.Generic;

namespace artm.Fetcher.Core.Services
{
    public class FetcherRepositoryService : SQLiteAsyncConnection, IFetcherRepositoryService
    {
        protected IFetcherLoggerService Logger { get; set; }
        
        public FetcherRepositoryService(IFetcherLoggerService loggerService, IFetcherRepositoryStoragePathService pathService) : base(pathService.GetPath(), false)
        {
            Logger = loggerService;
        }

        private void Log(string message)
        {
            Logger.Log("FETCHERREPOSITORYSERVICE: " + message);
        }

        public async Task Initialize()
        {
            await CreateTableAsync<UrlCacheInfo>();
            await CreateTableAsync<FetcherWebResponse>();
            await CreateTableAsync<FetcherWebRequest>();
        }

        public async Task<IUrlCacheInfo> GetEntryForRequestAsync(IFetcherWebRequest request)
        {
            IUrlCacheInfo data = null;
            if (request == null) return data;

            var dbRequest = await this.Table<FetcherWebRequest>()
                    .Where(x => x.Url == request.Url &&
                        x.Method == request.Method &&
                        x.Body == request.Body)
                    .FirstOrDefaultAsync();

            if (dbRequest != null)
            {
                var dbCache = await this.Table<UrlCacheInfo>()
                    .Where(x => x.FetcherWebRequestId == dbRequest.Id)
                    .FirstOrDefaultAsync();
                if (dbCache != null)
                {
                    data = await this.GetWithChildrenAsync<UrlCacheInfo>(dbCache.Id);
                }
            }

            if (data != null)
            {
                data.LastAccessed = DateTimeOffset.UtcNow;
                await this.UpdateWithChildrenAsync(data);
            }

            return data;
        }

        public async Task<IUrlCacheInfo> GetUrlCacheInfoForId(int id)
        {
            return await this.GetWithChildrenAsync<UrlCacheInfo>(id);
        }

        public async Task<IUrlCacheInfo> PreloadUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            var timestamp = DateTimeOffset.UtcNow.AddYears(-1);
            return await InsertUrlAsync(request, response, timestamp);
        }

        public async Task<IUrlCacheInfo> InsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            return await InsertUrlAsync(request, response, DateTimeOffset.UtcNow);
        }

        private async Task<IUrlCacheInfo> InsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response, DateTimeOffset timestamp)
        {
            IUrlCacheInfo hero = null;
            if (response == null || response as FetcherWebResponse == null)
            {
                return hero;
            }

            try
            {
                hero = await DatabaseInsertUrlAsync(request, response, timestamp);
            }
            catch (Exception ex)
            {
                Log("Could not insert entry: " + ex);
                var debug = 42;
                throw;
            }
            finally
            {
                //_lock.Release();
            }

            return hero;
        }

        private async Task<IUrlCacheInfo> DatabaseInsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response, DateTimeOffset timestamp)
        {
            var existingUrlCacheInfo = await GetEntryForRequestAsync(request);
            UrlCacheInfo hero = null;
            await this.RunInTransactionAsync(tran =>
            {
                if (existingUrlCacheInfo != null)
                {
                    tran.Delete<FetcherWebRequest>(existingUrlCacheInfo.FetcherWebRequestId);
                    tran.Delete<IFetcherWebResponse>(existingUrlCacheInfo.FetcherWebResponseId);
                    tran.Delete(existingUrlCacheInfo);
                }

                tran.InsertWithChildren(request, false);
                var theResponse = new FetcherWebResponse()
                {
                    Body = response.Body,
                    BodyAsBytes = response.BodyAsBytes,
                    Error = response.Error,
                    Headers = response.Headers,
                    HttpStatusCode = response.HttpStatusCode,
                };
                tran.InsertWithChildren(theResponse, false);

                hero = new UrlCacheInfo()
                {
                    FetcherWebResponseId = theResponse.Id,
                    FetcherWebResponse = theResponse,
                    FetcherWebRequestId = request.Id,
                    FetcherWebRequest = (FetcherWebRequest)request,
                    Created = timestamp,
                    LastUpdated = timestamp,
                    LastAccessed = timestamp
                };
                tran.InsertWithChildren(hero, true);
            });
            return hero;
        }

        public async Task UpdateUrlAsync(IFetcherWebRequest request, IUrlCacheInfo toBeUpdated, IFetcherWebResponse response)
        {
            if (request == null || toBeUpdated == null || response == null || response as FetcherWebResponse == null)
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            if(response.Id != 0)
            {
                toBeUpdated.FetcherWebResponseId = response.Id;
                toBeUpdated.FetcherWebResponse = response as FetcherWebResponse;
            }
            toBeUpdated.FetcherWebResponse.Body = response.Body;
            toBeUpdated.FetcherWebResponse.BodyAsBytes = response.BodyAsBytes;
            toBeUpdated.FetcherWebResponse.Error = response.Error;
            toBeUpdated.FetcherWebResponse.Headers = response.Headers;
            toBeUpdated.FetcherWebResponse.HttpStatusCode = response.HttpStatusCode;

            if (request.Id != 0)
            {
                toBeUpdated.FetcherWebRequestId = request.Id;
                toBeUpdated.FetcherWebRequest = request as FetcherWebRequest;
            }
            toBeUpdated.FetcherWebRequest.Headers = request.Headers;
            toBeUpdated.FetcherWebRequest.Body = request.Body;
            toBeUpdated.FetcherWebRequest.ContentType = request.ContentType;
            toBeUpdated.FetcherWebRequest.Method = request.Method;

            toBeUpdated.FetcherWebRequest.Url = request.Url;
            toBeUpdated.LastUpdated = timestamp;

            await this.UpdateWithChildrenAsync(toBeUpdated);
        }

        public async Task<bool> DeleteEntry(IUrlCacheInfo hero)
        {
            try
            {
                await this.RunInTransactionAsync(tran =>
                {
                    tran.Delete<FetcherWebRequest>(hero.FetcherWebRequestId);
                    tran.Delete<FetcherWebResponse>(hero.FetcherWebResponseId);
                    tran.Delete<UrlCacheInfo>(hero.Id);
                });
            }
            catch (Exception e)
            {
                Logger.Log("Error deleting entry: " + e);
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteEntriesOlderThan(int days)
        {
            try
            {
                var querylist = await Table<UrlCacheInfo>().ToListAsync();
                var query = (from x in querylist
                             where DaysBetween(x.LastUpdated, DateTime.UtcNow) >= days
                             select x).ToList();

                foreach (var item in query)
                {
                    await DeleteEntry(item);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Error deleting entries older than {days}: {e}");
                return false;
            }
            return true;
        }

        private static int DaysBetween(DateTimeOffset d1, DateTimeOffset d2)
        {
            TimeSpan span = d2.Subtract(d1);
            return (int)span.TotalDays;
        }

        public async Task<IEnumerable<UrlCacheInfo>> GetAllUrlCacheInfo()
        {
            return await this.GetAllWithChildrenAsync<UrlCacheInfo>();
        }

        public async Task<IEnumerable<FetcherWebResponse>> GetAllWebResponses()
        {
            return await this.GetAllWithChildrenAsync<FetcherWebResponse>();
        }

        public async Task<IEnumerable<FetcherWebRequest>> GetAllWebRequests()
        {
            return await this.GetAllWithChildrenAsync<FetcherWebRequest>();
        }

        public async Task<IEnumerable<IUrlCacheInfo>> GetUrlCacheInfoForRequest(FetcherWebRequest needle, bool url = true, bool method = true, bool headers = true, bool contentType = true, bool body = true)
        {
            IEnumerable<IUrlCacheInfo> result = await this.GetAllWithChildrenAsync<UrlCacheInfo>();

            if (url == true)
                result = result.Where(x => x.FetcherWebRequest.Url == needle.Url);
            if (method == true)
                result = result.Where(x => x.FetcherWebRequest.Method == needle.Method);
            if (body == true)
                result = result.Where(x => x.FetcherWebRequest.Body == needle.Body);
            if (headers == true)
                result = result.Where(x => x.FetcherWebRequest.HeadersSerialized == needle.HeadersSerialized);
            if (contentType == true)
                result = result.Where(x => x.FetcherWebRequest.ContentType == needle.ContentType);

            return result;
        }
    }
}
