using Blog;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using static Blog.BlogService;

namespace Server
{
    public class BlogServiceImpl : BlogServiceBase
    {
        private static readonly MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        private static readonly IMongoDatabase mongoDatabase = mongoClient.GetDatabase("myDb");
        private static readonly IMongoCollection<BsonDocument> mongoCollection = mongoDatabase.GetCollection<BsonDocument>("blog");

        /// <summary>
        /// Create Blog Method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context)
        {
            var blog = request.Blog;
            var doc = new BsonDocument("author_id", blog.AuthorId)
                .Add("title", blog.Title)
                .Add("content", blog.Content);

            mongoCollection.InsertOne(doc);

            string id = doc.GetValue("_id").ToString();
            blog.Id = id;

            return Task.FromResult(new CreateBlogResponse()
            {
                Blog = blog
            });
        }

        /// <summary>
        /// GetById Blog Method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(request.BlogId));
            var result = await mongoCollection.Find(filter).FirstOrDefaultAsync();
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id " + request.BlogId + " was not find"));
            }

            Blog.Blog blog = new Blog.Blog()
            {
                AuthorId = result.GetValue("author_id").AsString,
                Title = result.GetValue("title").AsString,
                Content = result.GetValue("content").AsString
            };

            return new ReadBlogResponse() { Blog = blog };
        }

        /// <summary>
        /// Update Blog Method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(request.Blog.Id));
            var result = await mongoCollection.Find(filter).FirstOrDefaultAsync();
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id " + request.Blog.Id + " was not find"));
            }

            var doc = new BsonDocument("author_id", request.Blog.AuthorId)
               .Add("title", request.Blog.Title)
               .Add("content", request.Blog.Content);

            mongoCollection.ReplaceOne(filter,doc);

            var blog = new Blog.Blog()
            {
                AuthorId = result.GetValue("author_id").AsString,
                Title = result.GetValue("title").AsString,
                Content = result.GetValue("content").AsString
            };

            blog.Id = request.Blog.Id;

            return new UpdateBlogResponse() { Blog = blog };
        }

        /// <summary>
        /// Delete Blog Method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<DeleteBlogResponse> DeleteBlog(DeleteBlogRequest request, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(request.BlogId));

            var result = await mongoCollection.DeleteOneAsync(filter);
            if (result.DeletedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id " + request.BlogId + " was not find"));
            }

            return new DeleteBlogResponse() { BlogId = request.BlogId };
        }

        /// <summary>
        /// GetAll Blog Method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ListBlog(ListBlogRequest request, IServerStreamWriter<ListBlogResponse> responseStream, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Empty;
            var result = await mongoCollection.FindAsync(filter);

            foreach (var item in result.ToList())
            {
                await responseStream.WriteAsync(new ListBlogResponse()
                {
                    Blog = new Blog.Blog()
                    {
                        AuthorId = item.GetValue("author_id").AsString,
                        Title = item.GetValue("title").AsString,
                        Content = item.GetValue("content").AsString
                    }
                });
            }
        }
    }
}
