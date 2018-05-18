using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using blogs_mysql;

namespace blog_websocket
{
    public class Startup
    {      
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug(LogLevel.Debug);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });


            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
			await SendInitialDataAsync(webSocket);
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                string receiveString = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);

				JObject jObject = JObject.Parse(receiveString);

				await ReceiveStruturedWebSocketObjectAsync(webSocket, jObject);

				buffer = new byte[1024 * 4];            
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

		public static async Task SendInitialDataAsync(WebSocket webSocket){
			using (blogsContext dbContext = new blogsContext())
			{
                foreach (var blog in dbContext.Blogs)
                {
                    await SendObjectAsync(webSocket, blog, BlogObjects.BLOG);
				}
                foreach (var author in dbContext.People)
                {
                    await SendObjectAsync(webSocket, author, BlogObjects.AUTHOR);
				}
                foreach (var post in dbContext.Blogposts)
                {
                    await SendObjectAsync(webSocket, post, BlogObjects.POST);
                }
			}
		}

		public static async Task SendObjectAsync<T>(WebSocket webSocket, T obj, BlogObjects blogObjects)
		{
			byte[] sendBuffer = new byte[1024 * 4];
			WebSocketObjectWrapper<T> webSocketObjectWrapper = new WebSocketObjectWrapper<T>(obj, Actions.ADD_OR_REPLACE, blogObjects);
			sendBuffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(webSocketObjectWrapper, new Newtonsoft.Json.Converters.StringEnumConverter()));
			await webSocket.SendAsync(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		public static async Task ReceiveStruturedWebSocketObjectAsync(WebSocket webSocket, JObject webSocketJson){
			switch ((MessageType)Enum.Parse(typeof(MessageType), webSocketJson["messageType"].ToString()))
			{
                case MessageType.REQUEST:
                    using (blogsContext dbContext = new blogsContext())
                    {
						switch ((BlogObjects)Enum.Parse(typeof(BlogObjects), webSocketJson["className"].ToString()))
						{
							case BlogObjects.BLOG:
                                foreach (var blog in dbContext.Blogs)
                                {
                                    await SendObjectAsync(webSocket, blog, BlogObjects.BLOG);
                                }
								break;
							case BlogObjects.AUTHOR:
                                foreach (var author in dbContext.People)
                                {
									await SendObjectAsync(webSocket, author, BlogObjects.AUTHOR);
                                }
								break;
                            case BlogObjects.POST:
                                foreach (var post in dbContext.Blogposts)
                                {
									await SendObjectAsync(webSocket, post, BlogObjects.POST);
                                }
                                break;
							default:
								break;
						}
                    }
                    break;
                case MessageType.BUSINESS_OBJECT:
					switch ((BlogObjects)Enum.Parse(typeof(BlogObjects), webSocketJson["className"].ToString())){
						case BlogObjects.BLOG:
                            var x = webSocketJson.ToObject<WebSocketObjectWrapper<Blogs>>(); //JsonConvert.DeserializeObject<WebSocketObjectWrapper<Blogs>>(receiveString);
                            if (x.Action == Actions.CREATE)
                            {
                                Blogs blog = x.Obj;
                                blog.Id = Guid.NewGuid();
                                using (blogsContext dbContext = new blogsContext())
                                {
                                    dbContext.Blogs.Add(blog);
                                    Task t1 = dbContext.SaveChangesAsync();
                                    Task t2 = SendObjectAsync(webSocket, blog, BlogObjects.BLOG);
                                    Task.WaitAll(t1, t2);
                                }
                            }
							break;
						case BlogObjects.AUTHOR:
                            var authorWrapper = webSocketJson.ToObject<WebSocketObjectWrapper<People>>(); //JsonConvert.DeserializeObject<WebSocketObjectWrapper<Blogs>>(receiveString);
							if (authorWrapper.Action == Actions.CREATE)
                            {
								People person = authorWrapper.Obj;
								person.Id = Guid.NewGuid();
                                using (blogsContext dbContext = new blogsContext())
                                {
									dbContext.People.Add(person);
                                    Task t1 = dbContext.SaveChangesAsync();
									Task t2 = SendObjectAsync(webSocket, person, BlogObjects.AUTHOR);
                                    Task.WaitAll(t1, t2);
                                }
                            }
							break;
						case BlogObjects.POST:
							var postWrapper = webSocketJson.ToObject<WebSocketObjectWrapper<Blogposts>>();
							if (postWrapper.Action == Actions.CREATE)
                            {
								Blogposts post = postWrapper.Obj;
								post.Id = Guid.NewGuid();
								post.DatePosted = DateTime.Now;
                                using (blogsContext dbContext = new blogsContext())
                                {
									dbContext.Blogposts.Add(post);
                                    Task t1 = dbContext.SaveChangesAsync();
									Task t2 = SendObjectAsync(webSocket, post, BlogObjects.POST);
                                    Task.WaitAll(t1, t2);
                                }
                            }
                            break;
						default:
							break;
					}
                    break;
				default:
					break;
			}
		}
    }
}
