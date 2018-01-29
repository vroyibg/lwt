

using LWT.Data.Contexts;
using LWT.Data.Models;
using LWT.Repository.Interfaces;

namespace LWT.Repository.Repositories
{
    public class LanguageRepository : BaseRepository<Language>, ILanguageRepository
    {
        public LanguageRepository(LWTContext context) : base(context)
        {

        }
    }
}