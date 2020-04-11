using Blog;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        const string Target = "127.0.0.1:50051";

        static async Task Main(string[] args)
        {
            Channel channel = new Channel(Target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("The client connectted successfully");
            });

            var client = new BlogService.BlogServiceClient(channel);

            //CreateBlog(client);

            //GetByIdBlog(client);

            //UpdateBlog(client);

            //DeleteBlog(client);

            await ListBlog(client);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }

        private static void UpdateBlog(BlogService.BlogServiceClient client)
        {
            try
            {
                var blog = new Blog.Blog
                {
                    AuthorId = "Şefik Can Totti",
                    Content = "Test Content v2",
                    Title = "Test Title v2",
                    Id = "5e91e157d31ecb45c8f84714"
                };

                var response = client.UpdateBlog(new UpdateBlogRequest()
                {
                    Blog = blog
                });

                Console.WriteLine(response.Blog.ToString());
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private static void GetByIdBlog(BlogService.BlogServiceClient client)
        {
            try
            {
                ReadBlogResponse response = client.ReadBlog(new ReadBlogRequest()
                {
                    BlogId = "5e91e157d31ecb45c8f84714"
                });

                Console.WriteLine(response.Blog.ToString());
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private static void CreateBlog(BlogService.BlogServiceClient client)
        {
            var response = client.CreateBlog(new CreateBlogRequest()
            {
                Blog = new Blog.Blog
                {
                    AuthorId = "Şefik Can",
                    Content = "Test Content",
                    Title = "Test Title"
                }
            });

            Console.WriteLine("The blog :" + response.Blog.Id + " was created !");
        }

        private static void DeleteBlog(BlogService.BlogServiceClient client)
        {
            try
            {
                var response = client.DeleteBlog(new DeleteBlogRequest()
                {
                    BlogId = "5e91e157d31ecb45c8f84714"
                });

                Console.WriteLine("The blog with id " + response.BlogId + " was deleted");
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private static async Task ListBlog(BlogService.BlogServiceClient client)
        {
            try
            {
                var response = client.ListBlog(new ListBlogRequest());

                while (await response.ResponseStream.MoveNext())
                {
                    Console.WriteLine(response.ResponseStream.Current.Blog.ToString());
                }
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }
    }
}
