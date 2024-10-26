using HNOne.API.Repositories.Interfaces;
using HNOne.API.Services.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using static Dapper.SqlMapper;

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

        /// <summary>
        /// lấy ra danh sách phòng ban
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Departments>> GetDepartment()
            => await _masterRepository.GetDepartment();

        public async Task<IEnumerable<Titles>> GetTitle()
            => await _masterRepository.GetTitle();
        public async Task<IEnumerable<Positions>> GetPosition()
                    => await _masterRepository.GetPosition();

        #endregion

        #region Command
        /// <summary>
        /// Thêm mới, cập nhật chi nhánh
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateBranch(string actionType, Branchs branch)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_BRANCH:
                        response = await _masterRepository.AddBranch(branch);
                        break;
                    case ProcessConstants.PUT_BRANCH:
                        response = await _masterRepository.UpdateBranch(branch);
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;

        }

        /// <summary>
        /// Thêm mới, cập nhật chi nhánh
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateDepartment(string actionType, Departments entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_DEPARTMENT:
                        response = await _masterRepository.AddDepartment(entity);
                        break;
                    case ProcessConstants.PUT_DEPARTMENT:
                        response = await _masterRepository.UpdateDepartment(entity);
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;

        }

        /// <summary>
        /// Thêm mới, cập nhật chi nhánh
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdatePosition(string actionType, Positions entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_POSITION:
                        response = await _masterRepository.AddPosition(entity);
                        break;
                    case ProcessConstants.PUT_POSITION:
                        response = await _masterRepository.UpdatePosition(entity);
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;

        }

        /// <summary>
        /// Thêm mới, cập nhật chi nhánh
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateTitle(string actionType, Titles entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_TITLE:
                        response = await _masterRepository.AddTitle(entity);
                        break;
                    case ProcessConstants.PUT_TITLE:
                        response = await _masterRepository.UpdateTitle(entity);
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;

        }
        #endregion
    }
}
