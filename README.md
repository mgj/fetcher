# Fetcher

[![Build status](https://ci.appveyor.com/api/projects/status/iysnpswp82ogp4vb?svg=true)](https://ci.appveyor.com/project/mgj/fetcher)
[![NuGet](https://img.shields.io/nuget/v/artm.fetcher.svg)](https://www.nuget.org/packages/artm.fetcher/)

Network layer in Xamarin apps

## Introduction

How to handle network calls in mobile apps is a common problem.

* You need to ensure you're using the platform-specific optimized network api calls to get SPDY, GZIP, etc.
  * On iOS this means NSUrlSession
  * On Android this means OkHttp
* You need to have a retry mechanism if the server is unresponsive
* You need to have a caching layer to
  * Minimize network usage, as connections are often metered
  * Improve performance
  * Handle unresponive servers / endpoints / no internet connection
* You need to handle the Cold Start problem: the very first time an app is started and there is no internet available. You should be able to ship your app with preloaded data for a given url.

## Example time!

Artm Fetcher solves this in a simple and easy to use manner:

```
IFetcherRepositoryStoragePathService path = new FetcherRepositoryStoragePathService();
IFetcherRepositoryService repository = new FetcherRepositoryService(path);
IFetcherWebService web = new FetcherWebService();

// Primary interface you should use
IFetcherService fetcher = new FetcherService(web, repository);

var url = new System.Uri("https://www.google.com");

// (Optional) Cold start: You can ship with preloaded data, and thus avoid
// an initial requirement for an active internet connection
fetcher.Preload(url, "<html>Hello world!</html>");

// Try our hardest to give you *some* response for a given url. 
// If an url has been recently created or updated we get the response from the local cache.
// If an url has NOT recently been created or updated we try to update 
// the response from the network. 
// If we cannot get the url from the network, and no cached data is available, we try to use preloaded data.
IUrlCacheInfo response = await fetcher.Fetch(url); 
```


## Implementation details

Artm Fetcher uses SQLite for storing its cache. The file is called "fetcher.db3" by default and is stored in the apps internal storage.

Artm Fetcher implements an exponential backoff retry mechanism using Polly . Retries 5 times by default.

## Get it here

GitHub: [https://github.com/mgj/fetcher](https://github.com/mgj/fetcher)

NuGet: [https://www.nuget.org/packages/artm.fetcher/](https://www.nuget.org/packages/artm.fetcher/)

MvvmCross Plugin GitHub: [https://github.com/mgj/MvvmCross-Plugins](https://github.com/mgj/MvvmCross-Plugins)

MvvmCross Plugin NuGet: [https://www.nuget.org/packages/artm.mvxplugins.fetcher/](https://www.nuget.org/packages/artm.mvxplugins.fetcher/)


## License

[Apache 2.0][apache]

[apache]: https://www.apache.org/licenses/LICENSE-2.0.html