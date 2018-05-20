# elFinder.NetCore

## Instructions

1. Install the NuGet package: https://www.nuget.org/packages/elFinder.NetCore/

2. Look at the [demo project](https://github.com/gordon-matt/elFinder.NetCore/tree/master/elFinder.NetCore.Web) for an example on how to integrate it into your own project.

As simple as that!

## Azure Storage Connector

In order to use the Azure Storage Connector

1. Open your **Startup.cs** file and look for the following lines:

```
AzureStorageAPI.AccountName = "[Name]";
AzureStorageAPI.AccountKey = "[Key]";
```

> Replace `[Name]` and `[Key]` with the appropriate values for your Azure account.

2. Change the **root directory** in the `AzureStorageController` from `test` to the name of your Azure file share.

3. Change the `url` parameter in **/Views/FileManager/Index.cshtml** to point to the `AzureStorageController`.

## Credits

**elFinder.NetCore** is based on my [fork](https://github.com/gordon-matt/elFinder.Net) of [elFinder.Net](https://github.com/leniel/elFinder.Net) by Leniel Macaferi which itself was based on an [older version](https://github.com/EvgenNoskov/Elfinder.NET) by Yevhen Noskov, which as far as I can tell is the original.

Many thanks also to [Flavio Smirne](https://github.com/fsmirne) for the excellent contribution of an Azure Storage driver and the work done to allow such extensibility.