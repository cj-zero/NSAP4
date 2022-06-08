using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using IdentityServer4.AccessTokenValidation;
using Infrastructure;
using Infrastructure.AutoMapper;
using Infrastructure.Exceptions;
using Infrastructure.Extensions.AutofacManager;
using Infrastructure.HuaweiOCR;
using Infrastructure.MQTT;
using Infrastructure.TecentOCR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Neware.Cap.DependencyInjection;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.HostedService;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali;
using OpenAuth.App.SignalR;
using OpenAuth.Repository;
using OpenAuth.Repository.Extensions;
using OpenAuth.WebApi.Model;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace OpenAuth.WebApi
{
    public class Startup
    {
        public IHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        private static MqttNetClient _mqttNetClient;
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //配置autoMapper
            services.AddAutoMapper();
            services.AddSingleton(provider =>
            {
                var service = provider.GetRequiredService<ILogger<StartupLogger>>();
                return new StartupLogger(service);
            });
            var logger = services.BuildServiceProvider().GetRequiredService<StartupLogger>();

            var identityServer = ((ConfigurationSection)Configuration.GetSection("AppSetting:IdentityServerUrl")).Value;
            if (!string.IsNullOrEmpty(identityServer))
            {
                services.AddAuthorization();

                services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = identityServer;
                        options.RequireHttpsMetadata = false;  // 指定是否为HTTPS
                        options.Audience = "openauthapi";
                    });
            }
            services.AddSwaggerGen(option =>
            {
                foreach (var controller in GetControllers())
                {
                    option.SwaggerDoc(controller, new OpenApiInfo
                    {
                        Version = controller,
                        Title = " NSAP4 API",
                        Description = "By Neware-R7"
                    });
                }


                logger.LogInformation($"api doc basepath:{AppContext.BaseDirectory}");
                foreach (var name in Directory.GetFiles(AppContext.BaseDirectory, "*.*",
                    SearchOption.AllDirectories).Where(f => Path.GetExtension(f).ToLower() == ".xml"))
                {
                    option.IncludeXmlComments(name, includeControllerXmlComments: true);
                    logger.LogInformation($"find api file{name}");
                }

                option.OperationFilter<GlobalHttpHeaderOperationFilter>(); // 添加httpHeader参数

                if (!string.IsNullOrEmpty(identityServer))
                {
                    //接入identityserver
                    option.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Description = "OAuth2登陆授权",
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"{identityServer}/connect/authorize"),
                                Scopes = new Dictionary<string, string>
                                {
                                    { "openauthapi", "同意openauth.webapi 的访问权限" }//指定客户端请求的api作用域。 如果为空，则客户端无法访问
                                }
                            }
                        }
                    });
                    option.OperationFilter<AuthResponsesOperationFilter>();
                }


            });
            services.Configure<AppSetting>(Configuration.GetSection("AppSetting"));
            services.AddControllers(option =>
            {
                option.Filters.Add<OpenAuthFilter>();
                option.Filters.Add<ExceptionFilter>();
                //option.Filters.Add<RequestActionFilter>();
            }).AddNewtonsoftJson(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不使用驼峰样式的key
                //options.SerializerSettings.ContractResolver = new DefaultContractResolver();    
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });

            var redisConnectionString = Configuration.GetValue<string>("AppSetting:Cache:Redis");
            if (string.IsNullOrWhiteSpace(redisConnectionString))
                services.AddMemoryCache();
            else
            {
                var csredis = new CSRedis.CSRedisClient(redisConnectionString); RedisHelper.Initialization(csredis);
                services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
            }
            services.AddCors();
            //          todo:如果正式 环境请用下面的方式限制随意访问跨域
            //            var origins = new []
            //            {
            //                "http://localhost:1803",
            //                "http://localhost:52789"
            //            };
            //            if (Environment.IsProduction())
            //            {
            //                origins = new []
            //                {
            //                    "http://demo.openauth.me:1803",
            //                    "http://demo.openauth.me:52789"
            //                };
            //            }
            //            services.AddCors(option=>option.AddPolicy("cors", policy =>
            //                policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(origins)));
            //在startup里面只能通过这种方式获取到appsettings里面的值，不能用IOptions😰
            //var dbType = ((ConfigurationSection)Configuration.GetSection("AppSetting:DbType")).Value;
            //if (dbType == Define.DBTYPE_SQLSERVER)
            //{
            //    services.AddDbContext<OpenAuthDBContext>(options =>
            //        options.UseSqlServer(Configuration.GetConnectionString("OpenAuthDBContext")));
            //}
            //else  //mysql
            //{
            //    services.AddDbContext<OpenAuthDBContext>(options =>
            //        options.UseMySql(Configuration.GetConnectionString("OpenAuthDBContext")));
            //}
            services.AddDbContexts();
            services.AddHttpClient();

            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Configuration["DataProtection"]));

            //设置定时启动的任务
            services.AddHostedService<QuartzService>();

            //SignalR
            services.AddNsapSignalR(Configuration);

            //SAP
            //services.AddSap();

            //CAP
            services.AddNewareCAP(Configuration);
            // 注册grpc服务
            services.AddGrpcClient<EdgeAPI.DataService.DataServiceClient>(c =>
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                c.Address = new Uri($"{ Configuration.GetSection("GrpcApi:url").Value }");
            });

            services.AddHttpClient("NsapApp", c =>
            {
                var appServerUrl = Configuration.GetValue<string>("AppSetting:AppServerUrl");
                c.BaseAddress = new Uri(appServerUrl);
                c.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
            });
            services.AddSingleton<HttpClienService>();
            services.AddSingleton<TecentOCR>();
            services.AddSingleton<HuaweiOCR>();
            services.AddScoped<CertAuthFilter>();
            //解决文件上传Request body too large
            services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = 1073741824;
            });

            #region MQTT
            var mqttConfig = new MqttConfig
            {
                Server = Configuration.GetValue<string>("MqttOption:HostIp"),
                Port = int.Parse(Configuration.GetValue<string>("MqttOption:HostPort")),
                Username = Configuration.GetValue<string>("MqttOption:UserName"),
                Password = Configuration.GetValue<string>("MqttOption:Password"),
                ClientIdentify = Configuration.GetValue<string>("MqttOption:ClientIdentify")
            };
            _mqttNetClient = new MqttNetClient(mqttConfig, MessageReceived);
            services.AddSingleton(_mqttNetClient);
            #endregion
        }
        private List<string> GetControllers()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var controlleractionlist = asm.GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type)).Select(a => a.CustomAttributes.LastOrDefault()?.NamedArguments.FirstOrDefault().TypedValue.Value.ToString()).Distinct().ToList();
            //var controlleractionlist = asm.GetTypes()
            //    .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            //    .OrderBy(x => x.Name).ToList();
            return controlleractionlist;
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacExt.InitAutofac(builder, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            ServiceLocator.ApplicationBuilder = app;
            ServiceLocator.serviceProvider = app.ApplicationServices;
            //.SetServices(serviceProvider);
            // 配置静态autoMapper
            AutoMapperHelper.UseStateAutoMapper(app);
            //配置ServiceProvider
            AutofacContainerModule.ConfigServiceProvider(app.ApplicationServices);
            //可以访问根目录下面的静态文件
            var staticfile = new StaticFileOptions { FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory) };
            app.UseStaticFiles(staticfile);

            //todo:测试可以允许任意跨域，正式环境要加权限
            app.UseCors(builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseStaticFiles();
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Templates")),
                    RequestPath = "/Templates"
                });

            //允许HttpContext.Request.Body被重复读取
            //app.Use((context, next) =>
            //{
            //	context.Request.EnableBuffering();
            //	return next();
            //});

            app.UseRouting();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMessageHub();
                endpoints.MapControllers();
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                //c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("OpenAuth.WebApi.index.html");

                foreach (var controller in GetControllers())
                {
                    c.SwaggerEndpoint($"/swagger/{controller}/swagger.json", controller);
                }
                c.DocumentTitle = "NSAP4 API";
                //c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                c.DocExpansion(DocExpansion.None);
                c.OAuthClientId("OpenAuth.WebApi");  //oauth客户端名称
                c.OAuthAppName("开源版webapi认证"); // 描述
            });
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var _mqttSubscribeApp = ServiceLocator.serviceProvider.GetService(typeof(MqttSubscribeApp)) as MqttSubscribeApp;
            _mqttSubscribeApp.SubscribeAsyncResult(topic, e.ApplicationMessage.Payload);
        }
    }
}
