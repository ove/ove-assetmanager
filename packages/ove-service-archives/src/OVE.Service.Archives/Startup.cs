﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OVE.Service.Archives.Domain;
using OVE.Service.Core.Extensions;
using OVE.Service.Core.FileOperations;
using OVE.Service.Core.FileOperations.S3;
using OVE.Service.Core.Processing.Service;
using OVE.Service.Core.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace OVE.Service.Archives {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        private static string _version = "v1";

        internal static void GetVersionNumber() {
            // read version from package.json
            var packageJson = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"package.json");
            if (File.Exists(packageJson)) {
                var package = JObject.Parse(File.ReadAllText(packageJson));
                _version = package["version"].ToString();
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            //register a cors policy we can later configure to use
            services.AddCors(o => o.AddPolicy("AllowAll", builder => {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            // make upload file size unlimited via gui (+ attribute on method to enable API unlimited)
            services.Configure<FormOptions>(x => {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });

            //start the processor microservice 
            services.AddHostedService<AssetProcessingService<ArchiveProcessor,ArchiveProcessingStates>>();

            // dependency injection of domain classes 
            services.AddSingleton(Configuration);
            services.AddTransient<IAssetFileOperations, S3AssetFileOperations>();

            // use mvc
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions( o=> {
                    o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });

            // set up swagger
            services.AddSwaggerGen(options => {

                options.SwaggerDoc(_version, new Info {
                    Title = "OVE Archive Microservice",
                    Version = _version,
                    Description =
                        "The OVE Archive Microservice is used to unzip archive assets to produce accessible files from them. " +
                        "This works within the OVE (Open Visualization Environment) is an open-source software stack, " +
                        "designed to be used in large scale visualization environments like the [Imperial College](http://www.imperial.ac.uk) " +
                        "[Data Science Institute\'s](http://www.imperial.ac.uk/data-science/) [Data Observatory](http://www.imperial.ac.uk/data-science/data-observatory/). " +
                        "OVE applications are applications designed to work with the OVE core. They are launched and managed within the browser-based OVE environment. " +
                        "Each OVE application exposes a standard control API and in some cases some application specific APIs.\"",
                    TermsOfService = "Terms Of Service",
                    Contact = new Contact {Email = "David.Birch@imperial.ac.uk"},
                    License = new License {Name = "MIT License", Url = "https://opensource.org/licenses/MIT"}


                });
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"OVE.Service.Archives.xml");
                options.IncludeXmlComments(filePath);
                options.DescribeAllEnumsAsStrings();
            });

        }

        /// <summary>
        /// Register this OVE service with the Asset Manager Service 
        /// </summary>
        private async void RegisterServiceWithAssetManager() {

            // get the service description from the AppSettings.json 
            OVEService service = new OVEService();
            Configuration.Bind("Service", service);
            service.ViewIFrameUrl = Configuration.GetValue<string>("ServiceHostUrl").RemoveTrailingSlash() + "/api/ArchiveController/ViewArchive/?id={id}"; 

            // then update the real processing states
            service.ProcessingStates.Clear();
            foreach (var state in Enum.GetValues(typeof(ArchiveProcessingStates))) {
                service.ProcessingStates.Add(((int) state).ToString(), state.ToString());
            }

            // register the service
          
            bool registered = false;
            while (!registered) {
                string url = null;
                try {
                    // permit environmental variables to be updated 
                    url = Configuration.GetValue<string>("AssetManagerHostUrl").RemoveTrailingSlash() +
                          Configuration.GetValue<string>("RegistrationApi");

                    Console.WriteLine("About to register with url " + url + " we are on " + service.ViewIFrameUrl);

                    using (var client = new HttpClient()) {
                        var responseMessage = await client.PostAsJsonAsync(url, service);

                        Console.WriteLine("Result of Registration was " + responseMessage.StatusCode);

                        registered = responseMessage.StatusCode == HttpStatusCode.OK;
                    }
                } catch (Exception e) {
                    Console.WriteLine("Failed to register - exception was" + e);
                    registered = false;
                }

                if (!registered) {
                    Console.WriteLine($"Failed to register with an Asset Manager on {url}- trying again soon");
                    Thread.Sleep(10000);
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {

            RegisterServiceWithAssetManager();

            // error pages
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
            }

            //set default content type
            app.UseStaticFiles(new StaticFileOptions {
                DefaultContentType = "application/json"
            });

            // may not need
            app.UseCookiePolicy();

            // use our cors policy defined earlier
            app.UseCors("AllowAll");

            // use mvc and set up routes for apis 
            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // turn swagger on
            app.UseSwagger()
                .UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/" + _version + "/swagger.json", "Archive Service " + _version);
                    c.RoutePrefix = "api-docs";
                });
        }
    }
}
