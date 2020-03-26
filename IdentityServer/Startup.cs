using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            bool useInMemoryStores = bool.Parse(Configuration["UseInMemoryStores"]);
            var connectionString = Configuration.GetConnectionString("IdentityServerConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (useInMemoryStores)
                {
                    options.UseInMemoryDatabase("IdentityServerDb");
                } else
                {
                    options.UseSqlServer(connectionString);
                }
            });


            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            // this adds the config data from DB (clients, resources)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = opt =>
                {
                    if (useInMemoryStores)
                    {
                        opt.UseInMemoryDatabase("IdentityServerDb");
                    }
                    else
                    {
                        opt.UseSqlServer(connectionString,
                                    optionsBuilder =>
                                        optionsBuilder.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name));
                    }
                };
            })
            // this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = opt =>
                {
                    if (useInMemoryStores)
                    {
                        opt.UseInMemoryDatabase("IdentityServerDb");
                    }
                    else
                    {
                        opt.UseSqlServer(connectionString,
                                    optionsBuilder =>
                                        optionsBuilder.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name));
                    }
                };

                // this enables automatic token cleanup. this is optional
                options.EnableTokenCleanup = true;
            })
            .AddAspNetIdentity<IdentityUser>();

            if (Environment.IsDevelopment())
            {
                // creates a temporary key for signing tokens
                // It’s OK for development but you need to be replace it with a valid persistent key when moving to production environment
                builder.AddDeveloperSigningCredential();
            } else
            {
                throw new Exception("need to configure key material");
            }

            services.AddAuthentication();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }
    }
}
