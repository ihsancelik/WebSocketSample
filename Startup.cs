using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            app.Use(async (ctx, nextMsg) =>
            {
                Console.WriteLine("Web Socket is Listening");
                if (ctx.Request.Path == "/testws")
                {
                    if (ctx.WebSockets.IsWebSocketRequest)
                    {
                        var wSocket = await ctx.WebSockets.AcceptWebSocketAsync();
                        await Talk(ctx, wSocket);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 400;
                    }

                }
                else
                {
                    await nextMsg();
                }

            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private async Task Talk(HttpContext hContext, WebSocket wSocket)
        {
            var bag = new byte[1024];
            WebSocketReceiveResult result = await wSocket.ReceiveAsync(new ArraySegment<byte>(bag), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var inComingMesage = Encoding.UTF8.GetString(bag, 0, result.Count);
                Console.WriteLine("\nClients says that: '{0}'", inComingMesage);
                var rnd = new Random();
                var number = rnd.Next(1, 100);
                string message = string.Format("You luck Number is '{0}'. Dont't remember that", number.ToString());
                byte[] outGoingMeesage = Encoding.UTF8.GetBytes(message);
                await wSocket.SendAsync(new ArraySegment<byte>(outGoingMeesage, 0, outGoingMeesage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await wSocket.ReceiveAsync(new ArraySegment<byte>(bag), CancellationToken.None);

            }

            await wSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

        }
    }
}
