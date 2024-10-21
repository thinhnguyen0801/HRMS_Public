using HNOne.API.Repositories.Interfaces;
using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace HNOne.API.Repositories
{
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly MasterDbContext _dbContext;
        public MasterDataRepository(MasterDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Branchs>> GetBranch()
        {
            var lstBranch = await _dbContext.Branchs.Where(m=> m.IsDelete == false).ToListAsync();
            return lstBranch;
        }

        public async Task<IEnumerable<Menus>> GetMenu()
        {
            var lstMenus = await _dbContext.Menus.Where(m => m.IsVisible).ToListAsync();
            return lstMenus;
        }
    }
}
