//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using MittDevQA.Utils.Mvc;
//using NSwag;
//using NSwag.Generation.Processors;
//using NSwag.Generation.Processors.Contexts;
//using NSwag.Generation.Processors.Security;
//using System.Collections.Generic;

//namespace MittDevQA.Utils.Swagger
//{
//    public static class SwaggerExtenstion
//    {
//        /*
//         * "swagger": {
//            "name": "v1",
//            "title": "Api Management",
//            "version": "v1",
//            "IsActive": true,
//            "BaseUrl": "/swagger/v1/swagger.json",
//            },
//         */

//        public static IServiceCollection AddSwaggerView(this IServiceCollection services)
//        {
//            SwaggerOptions options;
//            using (var serviceProvider = services.BuildServiceProvider())
//            {
//                var configuration = serviceProvider.GetService<IConfiguration>();
//                services.Configure<SwaggerOptions>(configuration.GetSection("swagger"));
//                options = configuration.GetOptions<SwaggerOptions>("swagger");
//            }

//            if (!options.IsActive) return services;

//#if (DEBUG || ONLINEDEBUG)
//            services.AddSwaggerDocument(document =>
//                        {
//                            document.DocumentName = options.Name;
//                            document.Title = options.Title;
//                            document.Version = options.Version;
//                            document.OperationProcessors.Add(new CultureOperatoinProcessor());
//                            document.DocumentProcessors.Add(
//                                new SecurityDefinitionAppender("Bearer", new List<string>(), new OpenApiSecurityScheme()
//                                {
//                                    Type = OpenApiSecuritySchemeType.ApiKey,
//                                    Name = "Authorization",
//                                    Description =
//                                        "Copy 'Bearer ' + valid JWT token (retrieved by using \"/api/Auth/login\" entrypoint) into the field",
//                                    In = OpenApiSecurityApiKeyLocation.Header
//                                })
//                            );

//                            document.OperationProcessors.Add(new OperationSecurityScopeProcessor("Bearer"));
//                        });
//#endif

//            return services;
//        }

//        public static IApplicationBuilder UseSwaggerView(this IApplicationBuilder app)
//        {
//            SwaggerOptions options;

//            using (var serviceScope = app.ApplicationServices.CreateScope())
//            {
//                var configuration = serviceScope.ServiceProvider.GetService<IConfiguration>();
//                options = configuration.GetOptions<SwaggerOptions>("swagger");
//            }
//            if (!options.IsActive) return app;

//#if (DEBUG || ONLINEDEBUG)

//            if (string.IsNullOrEmpty(options.BaseUrl))
//                app.UseOpenApi().UseSwaggerUi3();
//            else
//                app.UseOpenApi(opt => opt.Path = options.BaseUrl).UseSwaggerUi3();
//#endif

//            return app;
//        }
//    }

//    public class CultureOperatoinProcessor : IOperationProcessor
//    {
//        public bool Process(OperationProcessorContext context)
//        {
//            context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
//            {
//                Kind = OpenApiParameterKind.Query,
//                Name = "culture",
//                Description = "Culture of your Request ",
//                Default = "ar-LY",
//                Type = NJsonSchema.JsonObjectType.String,
//            });

//            return true;
//        }
//    }
//}