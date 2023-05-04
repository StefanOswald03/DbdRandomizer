using Core.Contracts;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    internal class PerkTranslationRepository : GenericRepository<PerkTranslation>, IPerkTranslationRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PerkTranslationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
