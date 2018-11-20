# OVE Asset Manager Services

This collection of microservices provides end users with a simple way of managing and processing content to be displayed using OVE. 

## Asset Manager

The core service is the **Asset Manager**. This enables media and content to be uploaded to an **S3** compatible **object store** and records metadata about its processing in an **SQL database**. A full user friendly website is provided to interact with the Asset Manager. A full RESTful API is provided for advanced users.  

## Asset Services

A variety of other Asset Services are provided to process assets into a form advantageous for display using OVE. These services are generally headless and will automatically register themselves with an Asset Manager via its API. They will then generally automatically process assets which require their services. For example the [**Image Tile Service**](https://github.com/ove/ove-asset-services/blob/master/packages/ove-service-imagetiles/README.MD) will create a Deep Zoom image (.dzi file) and corresponding tileset from each image asset. 

## Scalability

The system is intended to be scalable and many copies of the Asset Manager can be run - the EntityFramework and SQL database concurrency models ensure that the ``ProcessingState`` for each Asset is atomically updated despite running multiple instances. 

The Asset Processing Microservices work on a pull based model - they will each independently regularly request further work. You are encouraged to run multiple copies of each Asset Processing Microservice: these should be configured to communicate with an instance of the Asset Manager. These services will then periodically check for Assets to process. Your database and object store may each scale independently. 

## Storage Model

Assets will be stored on the object store on a one bucket per `Project` model. Every Asset is assigned a ``GUID`` on creation. This is used to identify it in the SQL database and on the object store. Within the Project bucket each Asset is uploaded to the root of a folder named by this ``GUID``. The filename of the asset will be preserved where possible - file names must consist of `a-z`,`A-Z`, `()`, `-` or `_` and be of length less than `1024` characters. Asset processing services are free to place processed files and folders anywhere within the asset directory. 

## Configuration

Configuration of the service is achieved by modifying the ``appsettings.json`` file. These settings are automatically over written by **Environment Variables** (reflection of the JSON hierarcy should be acheived using ``__`` e.g. ``s3Client__Secret``). 

### Configuring S3

In common with all S3 compatible object stores the following three properties are required and should be set as follows in ``appsettings.json`` or overridden by ``environment variables`` as discussed above.

```  
"s3Client": {
    "AccessKey": "key",
    "Secret": "secret",
    "ServiceURL": "host"
  }
 ```
 
### Configuring Maria DB
The [MariaDB](https://mariadb.org/) configuration should be set as follows in `appsettings.json` or overridden by ``Environment Variables`` as discussed above. 

```
  "MariaDB": { 
    "ConnectionString": "Server=SERVER;Port=3306;Database=DATABASE;User=USERNAME;Password=PASSWORD;", 
    "Version" :  "10.3.10"  
  }
```

## Using the Asset Manager

Assets should be uploaded to the asset manager either through the web interface or submitting a HTTP `POST` request to the `/OVEAssetModelController/Create/` API. 
Asset URLs on the object store may be found via the `/OVEAssetModelController/GetAssetURL?project=X&file=Y` API or by ID on  `/OVEAssetModelController/GetAssetURLbyId?id=X`

## Versioning

The Asset Manager supports file versioning. The service will, unless otherwise specified, always provide the most recently uploaded/edited asset version when searching by Project and Asset Name. Previous versions may be found via the `/OVEAssetModelController/ListAssets/Project/` and `/OVEAssetModelController/ListAssets/Project/Name/` API's.

## Processing state

Asset Processing Services will update the ProcessingState of each asset using the ``/OVEAssetModelController/SetProcessingState/{id}/{state}/`` API. Assets in the unprocessed state (`0`) will be returned to processing microservices upon request. To reset the processing state of an asset and retry the processing you should use the ``/OVEAssetModelController/ResetProcessing/{id}`` of the Asset Manager API. A custom set of procesing states may be registered for every Asset Type. 

## Interaction of Processing Services and Asset Manager

Since the Asset Manager has no knowledge of different asset types, how to process them or how to display them each asset processing microservice must register this knowledge with the Asset Manager. Each microservice should be configured with the URI of an asset manager with the `AssetManagerHostUrl` configuration property. These services will then register with the asset manager using the `/api/ServicesRegistry/Register` route to register a service description (see [here](https://github.com/ove/ove-asset-services/blob/master/packages/ove-asset-manager/src/OVE.Service.AssetManager/Domain/OVEService.cs)). 

```
{
  "name": "AService",
  "fileTypes": [
    ".abc",
    ".xyz"
  ],
  "viewIFrameUrl": "full uri",
  "processingStates": {
    "int": "name",
  }
}
```

The `fileTypes` are used for validation of uploads to the asset manager.

The `processingStates` are used to provide meaningful status updates to users.

The `viewIFrameUrl` is used to enable each service to provide a webpage for rendering an asset for display within the asset manager. This **must** include the string ``{id}`` which will be replaced with id of the asset. 

## Asset Metadata

Optionally assets may have JSON metadata attached which can be updated via `GET` / `POST` on `/OVEAssetModelController/AssetMeta/{id}`. This is intended to be entered programmatically by asset processing services. 

## Implementation 

The Asset Manager is implemented in [**C#**](https://github.com/dotnet/roslyn) and [**.net core 2.1**](https://blogs.msdn.microsoft.com/dotnet/2018/05/30/announcing-net-core-2-1/) and so runs cross platform. 

[**OWIN** (Open Web Interface for .Net)](http://owin.org/) is used to decouple the web stack modules with **Dependency Injection** used throughout, precise configuration can be explored in `Program.cs` and `Startup.cs`. Module imported are completed via [**Nuget**](www.nuget.org) and are listed in the `.csproj` files. 

The cross platform lightweight [**Kestrel**](https://github.com/aspnet/KestrelHttpServer) HTTP server is used. 

The opensource [**ASP.Net**](https://github.com/aspnet/AspNetCore) framework is used to create RESTful APIs, further the [**Asp.net MVC**](https://github.com/aspnet/Mvc) framework and is used to coordinate views, controllers and model state. Model state validation is enabled via a [**Validation Attributes**](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1) and is provided server-side and automatically provided client-side with [**JQuery**](https://jquery.com/). 

The [**Entity Framework Core**](https://github.com/aspnet/EntityFrameworkCore) framework is used to manage models. [**Code first migrations and deployment**](https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/migrations-and-deployment-with-the-entity-framework-in-an-asp-net-mvc-application) together with [**Scaffolding**](https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/hands-on-labs/aspnet-mvc-4-entity-framework-scaffolding-and-migrations) were initially used to generate CRUD templates. 

Initialisation, maintenance and updates to database structure are managed via database [**Migrations**](https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/migrations-and-deployment-with-the-entity-framework-in-an-asp-net-mvc-application) which enable automatic in-place database upgrades. 

[**Razor**](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-2.1) is used for View generation. Styling is achieved via [**Bootstrap**](https://getbootstrap.com/docs/4.1/getting-started/introduction/). 

Internal processing microservices are implemented using the [**HostedService**](https://blogs.msdn.microsoft.com/cesardelatorre/2017/11/18/implementing-background-tasks-in-microservices-with-ihostedservice-and-the-backgroundservice-class-net-core-2-x/) pattern.

A variety of [**database adaptors**](https://docs.microsoft.com/en-us/ef/core/providers/) are available for EntityFramwork and can be swapped out easily if required, provided the concurrency restrictions are respected. Currently the Asset Manager uses the [**MariaDB**](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql) adaptor and the [**Amazon S3**](https://www.nuget.org/packages/Amazon.S3/) driver. 

Full API documentation is acheived using code based [**xml documentation**](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc) and [**Swashbuckle**](https://github.com/domaindrivendev/Swashbuckle) to generate [**Swagger**](https://swagger.io/) documentation and ui which can be viewed on `/api-docs/`. 

**Asset processing microservices may be implemented in any language and interact with the asset manager using its APIs**. 
