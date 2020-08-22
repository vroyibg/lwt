namespace Lwt.Utilities
{
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;

    /// <inheritdoc />
    public class MongoDbIndexCreator : IIndexCreator
    {
        private readonly LwtDbContext lwtDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIndexCreator"/> class.
        /// </summary>
        /// <param name="lwtDbContext">the index db context.</param>
        public MongoDbIndexCreator(LwtDbContext lwtDbContext)
        {
            this.lwtDbContext = lwtDbContext;
        }

        /// <inheritdoc />
        public async Task CreateIndexesAsync()
        {
            await this.CreateTermIndexesAsync();
            await this.CreateTextIndexesAsync();
        }

        private async Task CreateTermIndexesAsync()
        {
            IMongoCollection<Term> collection = this.lwtDbContext.GetCollection<Term>();
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Term>(
                Builders<Term>.IndexKeys.Ascending(term => term.Content)));
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Term>(
                Builders<Term>.IndexKeys.Ascending(term => term.CreatorId)));
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Term>(
                Builders<Term>.IndexKeys.Ascending(term => term.LanguageCode)));
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Term>(
                Builders<Term>.IndexKeys.Combine(
                    Builders<Term>.IndexKeys.Ascending(t => t.CreatorId),
                    Builders<Term>.IndexKeys.Ascending(t => t.LanguageCode),
                    Builders<Term>.IndexKeys.Ascending(t => t.Content)),
                new CreateIndexOptions { Unique = true }));
        }

        private async Task CreateTextIndexesAsync()
        {
            IMongoCollection<Text> collection = this.lwtDbContext.GetCollection<Text>();
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Text>(
                Builders<Text>.IndexKeys.Ascending(text => text.LanguageCode)));
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Text>(
                Builders<Text>.IndexKeys.Ascending(text => text.CreatorId)));
        }
    }
}