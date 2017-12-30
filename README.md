# Fetcher

[![Build status](https://ci.appveyor.com/api/projects/status/github/mgj/fetcher?svg=true)](https://ci.appveyor.com/project/mgj/fetcher)
[![NuGet](https://img.shields.io/nuget/v/artm.fetcher.svg)](https://www.nuget.org/packages/artm.fetcher/)

Simple Network layer for Xamarin apps

## Introduction

Doing network such as downloading a JSON file from a webserver can be difficult on mobile devices

* You need to ensure you're using the platform-specific optimized network api calls to get SPDY, GZIP, HTTP/2, Connection Pooling etc.
  * On iOS this means [NSUrlSession](https://developer.apple.com/reference/foundation/urlsession)
  * On Android this means [OkHttp](http://square.github.io/okhttp/)
* You need to have a retry mechanism if the server is unresponsive
* You need to have a caching layer to
  * Minimize network usage by caching redundant requests, as connections are often metered
  * Improve performance
  * Handle unresponive servers / endpoints / no internet connection
* You need to handle the Cold Start problem: the very first time an app is started and there is no internet available. You should be able to ship your app with preloaded data for a given url.

## Goals
Goals for this project:

 * Simple and easy to use - Prefer reasonable opinionated defaults over complex interfaces or configuration
 * Stable and predictable - Unit tests helps with this, but please open an issue or pull request if you experience crashes or surprising behavior
 * Minimal dependencies - Keep the library as small as possible. Dont add big dependencies (E.g. Realm database backing store) unless we have to
 * Distribution and easy installation - CI builds and publishes the library using NuGet. Include the library in all your projects and you should be good to go

## Install

```
Find the package in your IDE's nuget explorer

or

Install-Package artm.fetcher

or

nuget install artm.fetcher

```

## Example time!

Setup:

```
// Instantiate these on Android / iOS
IFetcherLoggerService loggerService = new FetcherLoggerService();
IFetcherRepositoryStoragePathService path = new FetcherRepositoryStoragePathService();
IFetcherWebService web = new FetcherWebService();
FetcherRepositoryService repository = new FetcherRepositoryService(loggerService, path);

await repository.Initialize();
```

Using:

```
// Primary interface you should use from your Core/PCL project
IFetcherService fetcher = new FetcherService(web, repository);
Uri url = new System.Uri("https://www.google.com");

// Try our hardest to give you *some* response for a given url. 
// If an url has been recently created or updated we get the response from the local cache.
// If an url has NOT recently been created or updated we try to update 
// the response from the network. 
// If we cannot get the url from the network, and no cached data is available, we try to use preloaded data.
IUrlCacheInfo response = await fetcher.FetchAsync(url);

// (Optional) Cold start: You can ship with preloaded data, and thus avoid
// an initial requirement for an active internet connection
await fetcher.PreloadAsync(url, new FetcherWebResponse() { Body = "<html>Hello world!</html>" });

// Dont like HTTP GET? No problem!
IUrlCacheInfo response = await fetcher.FetchAsync(new FetcherWebRequest()
{
    Url = url,
    Method = "POST",
    Headers = new System.Collections.Generic.Dictionary<string, string>(),
    Body = @"[{ ""myData"": ""data"" }]",
    ContentType = string.Empty
}, TimeSpan.FromDays(1));
```

## Caching rules

A new cache entry will be created (that is, a FetchAsync call is considered unique) if the FetcherWebRequest and accompanying UrlCacheInfo does not already exist. You can search through the cached items using the `IFetcherRepositoryService` interface. Example:

```
// Search cached entries by Url...
IEnumerable<IUrlCacheInfo> info = await repository.GetUrlCacheInfoForRequest(new FetcherWebRequest
{
    Url = "https://www.google.com"
});

// ... or anything else
IEnumerable<IUrlCacheInfo> info2 = await repository.GetUrlCacheInfoForRequest(new FetcherWebRequest
{
    Url = "https://www.google.com",
    Method = "POST",
    Headers = new Dictionary<string, string>
    {
        {"X-ZUMO-APPLICATION" , "Hello world!" }
    }
});
```

Finally you can use the ID's from the underlying SQLite database:

```
IUrlCacheInfo info = await repository.GetUrlCacheInfoForId(15);
```

If these are not sufficient, you can retrieve all cached items and do the search yourself:

```
IEnumerable<IUrlCacheInfo> info = await repository.GetAllUrlCacheInfo();
IEnumerable<IFetcherWebRequest> request = await repository.GetAllWebRequests();
IEnumerable<IFetcherWebResponse> response = await repository.GetAllWebResponses();
```

Cleanup:

```
await repository.DeleteEntriesOlderThan(14); // Days
```

## FAQ

On android if you get an error popup about denied access to local libraries, add a reference to System.Data.SQLite in your Android project

## For contributors

Consider using the Playground.Touch and Playground.Droid projects if you want to experiment with the code. Both projects are set up with dependencies and references so that you can get started quicker - Pull requests are very welcome!


## Get it here

GitHub: [https://github.com/mgj/fetcher](https://github.com/mgj/fetcher)

NuGet: [https://www.nuget.org/packages/artm.fetcher/](https://www.nuget.org/packages/artm.fetcher/)

(Optional) MvvmCross Plugin GitHub: [https://github.com/mgj/MvvmCross-Plugins](https://github.com/mgj/MvvmCross-Plugins)

(Optional) MvvmCross Plugin NuGet: [https://www.nuget.org/packages/artm.mvxplugins.fetcher/](https://www.nuget.org/packages/artm.mvxplugins.fetcher/)


## License

[Apache 2.0][apache]

[apache]: https://www.apache.org/licenses/LICENSE-2.0.html