# Image Tile Service

This service uses the [LibVips](https://jcupitt.github.io/libvips/) library via [NetVips](https://github.com/kleisauke/net-vips) to take Image based Assets and create [Deep Zoom](https://en.wikipedia.org/wiki/Deep_Zoom) image tile sets.


## Configuration

Configuration of the service is achieved by modifying the ``appsettings.json`` file. These settings are automatically over written by **Environment Variables** (reflection of the JSON hierarcy should be acheived using ``__`` e.g. ``s3Client__Secret``). 

### Configuring Service

You must provide the service with the URL of the asset manager it should register with. Further you should provide the final externally accessible URL of the service (used for callbacks from the Asset Manager). 

```
  "AssetManagerHostUrl" :  "http://localhost:8181" ,
  "ServiceHostUrl" : "http://localhost:8182", 
```

### Configuring S3

In common with all S3 compatible object stores the following three properties are required and should be set as follows in `appsettings.json` or overridden in environment variables as discussed above.

```  
"s3Client": {
    "AccessKey": "secret",
    "Secret": "secret",
    "ServiceURL": "host"
  }
 ```
 
 
