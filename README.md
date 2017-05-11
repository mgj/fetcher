# Fetcher

[![Build status](https://ci.appveyor.com/api/projects/status/iysnpswp82ogp4vb?svg=true)](https://ci.appveyor.com/project/mgj/fetcher)
[![NuGet](https://img.shields.io/nuget/v/artm.fetcher.svg)](https://www.nuget.org/packages/artm.fetcher/)

Network layer in Xamarin apps

## Introduction

Doing network such as downloading a JSON file from a webserver can be difficult on mobile devices

* You need to ensure you're using the platform-specific optimized network api calls to get SPDY, GZIP, HTTP/2, Connection Pooling etc.
  * On iOS this means [NSUrlSession](https://developer.apple.com/reference/foundation/urlsession)
  * On Android this means [OkHttp](http://square.github.io/okhttp/)
* You need to have a retry mechanism if the server is unresponsive
* You need to have a caching layer to
  * Minimize network usage, as connections are often metered
  * Improve performance
  * Handle unresponive servers / endpoints / no internet connection
* You need to handle the Cold Start problem: the very first time an app is started and there is no internet available. You should be able to ship your app with preloaded data for a given url.

## Goals
Goals for this project:

 * Simple and easy to use - Prefer reasonable opinionated defaults over complex interfaces or configuration. We worry about the details so you dont have to. Examples: 
   * We use a SQLite database as backsing store. 
   * We use exponential backoff with polly. 
   * We store data in the temporary directory on iOS and Android. On iOS backup is disabled for the cache.
 * Stable and predictable - Unit tests and CI helps with this, but please open an issue or pull request if you experience crashes or surprising behavior
 * Minimal dependencies - Keep the library as small as possible. Dont add dependencies (E.g. Realm database backing store) unless we have to
 * Distribution and easy installation - CI builds and publishes the library using NuGet. Include the library in all your projects and you should be good to go

## Install

```
nuget install artm.fetcher
```

## Example time!

```
// Instantiate these on Android / iOS
IFetcherRepositoryStoragePathService path = new FetcherRepositoryStoragePathService();
IFetcherRepositoryService repository = new FetcherRepositoryService(path);
IFetcherWebService web = new FetcherWebService();

// Primary interface you should use from your Core/PCL project
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


## For developers

Consider using the Playground.Touch and Playground.Droid projects if you want to experiment with the code. Both projects are set up with dependencies and references so that you can get started quicker - Pull requests are very welcome!


## Get it here

GitHub: [https://github.com/mgj/fetcher](https://github.com/mgj/fetcher)

NuGet: [https://www.nuget.org/packages/artm.fetcher/](https://www.nuget.org/packages/artm.fetcher/)

(Optional) MvvmCross Plugin GitHub: [https://github.com/mgj/MvvmCross-Plugins](https://github.com/mgj/MvvmCross-Plugins)

(Optional) MvvmCross Plugin NuGet: [https://www.nuget.org/packages/artm.mvxplugins.fetcher/](https://www.nuget.org/packages/artm.mvxplugins.fetcher/)


## License

[Apache 2.0][apache]

[apache]: https://www.apache.org/licenses/LICENSE-2.0.html