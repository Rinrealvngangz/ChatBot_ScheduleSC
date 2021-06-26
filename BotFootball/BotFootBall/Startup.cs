using BotFootBall.Bots;
using BotFootBall.Dialogs;
using BotFootBall.Dialogs.Schedule;
using BotFootBall.Middleware;
using BotFootBall.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFootBall
{
    public class Startup
    {
        public Startup(IConfiguration configuration , IHostEnvironment env)
        {
            Configuration = configuration;
            ContentRootPath = env.ContentRootPath;
        }

        public IConfiguration Configuration { get; }

        public string ContentRootPath { get; private set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Create the Bot Framework Adapter with error handling enabled.
        //   services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
          

       
            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            services.AddSingleton<UserState>();
            // Create the Conversation state. (Used by the Dialog system itself.)

            services.AddSingleton<ConversationState>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BotFootBall", Version = "v1" });
            });
            var builder = new ConfigurationBuilder().
                SetBasePath(ContentRootPath).AddJsonFile("appsettings.json").AddEnvironmentVariables();
            var configuration = builder.Build();
            services.AddSingleton(configuration);
        
            services.AddBot<DialogBot<MainDialog>>(option =>
            {
                option.CredentialProvider = new ConfigurationCredentialProvider(configuration);
                
                //option.Middleware.Add(new PassthoughtMiddleware());
               
            });
            services.AddSingleton<ScheduleDayDialog>();
            services.AddSingleton<MainDialog>();
            services.AddTransient<IBot, DialogWelcomeBot<MainDialog>>();
            services.AddTransient<ISchedule,ScheduleService>();
            services.AddTransient<IStandingService, StandingService>();
            services.AddTransient<ITeamService, TeamService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles()
            .UseBotFramework()
            .UseHttpsRedirection()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
