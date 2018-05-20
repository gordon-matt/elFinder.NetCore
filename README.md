# elFinder.NetCore

## Instructions

1. Install the NuGet package: https://www.nuget.org/packages/elFinder.NetCore/

2. Look at the [demo project](https://github.com/gordon-matt/elFinder.NetCore/tree/master/elFinder.NetCore.Web) for an example on how to integrate it into your own project.

As simple as that!

## Azure Connector

1. In order to use the Azure Connector, modify your Startup.cs file and replace [Name] and [Key] with the appropriate values from your Azure account.
   AzureStorageAPI.AccountName = "[Name]";
   AzureStorageAPI.AccountKey = "[Key]";

2. Change the root directory in your AzureStorage Controller to the name of your Azure File share.

3. Change your FileManager view to point to the AzureStorage Controller.

Start using your Azure Storage account transparently.

## Credits

**elFinder.NetCore** is based on my [fork](https://github.com/gordon-matt/elFinder.Net) of [elFinder.Net](https://github.com/leniel/elFinder.Net) by Leniel Macaferi which itself was based on an [older version](https://github.com/EvgenNoskov/Elfinder.NET) by Yevhen Noskov, which as far as I can tell is the original.

Many thanks also to [Flavio Smirne](https://github.com/fsmirne) for the excellent contribution of an Azure Storage driver and the work done to allow such extensibility.