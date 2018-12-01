namespace Lwt.Repositories
{
    using System;
    using System.Threading.Tasks;

    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;

    using MongoDB.Driver;

    /// <summary>
    /// the term repository.
    /// </summary>
    public class TermRepository : BaseRepository<Term>, ITermRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TermRepository"/> class.
        /// </summary>
        /// <param name="lwtDbContext">the application db context.</param>
        public TermRepository(LwtDbContext lwtDbContext)
            : base(lwtDbContext)
        {
        }

        /// <inheritdoc/>
        public Task<Term> GetByContentAsync(string content)
        {
            return this.Collection.Find(term => term.Content == content).SingleOrDefaultAsync();
        }

        /// <inheritdoc />
        public Task<Term> GetByUserIdAndContentAsync(Guid userId, string word)
        {
            return this.Collection.Find(term => term.Content == word && term.CreatorId == userId)
                .SingleOrDefaultAsync();
        }
    }
}