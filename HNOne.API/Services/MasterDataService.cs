using HNOne.API.Repositories.Interfaces;
using HNOne.API.Services.Interfaces;
using HNOne.Model.Entities;

namespace HNOne.API.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly IMasterDataRepository _masterRepository;

        public MasterDataService(IMasterDataRepository masterRepository)
        {
            _masterRepository = masterRepository;
        }

        #region Query

        /// <returns></returns>
        /// <summary>
        /// lấy danh sách menu
        /// </summary>
        public async Task<IEnumerable<Menus>> GetMenu()
            => await _masterRepository.GetMenu();


        /// <summary>
        /// lấy ra danh sách chi nhánh
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Branchs>> GetBranch()
            => await _masterRepository.GetBranch();

        #endregion

        #region Command
        public async Task<Branchs> UpdateBranch(string actionType, Branchs branch)
        {
            return null;
        }
        #endregion
    }
}
