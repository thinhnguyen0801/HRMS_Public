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
        public async Task<IEnumerable<Departments>> GetDepartment(RequestModel request)
            => await _masterRepository.GetDepartment(request);

        public async Task<IEnumerable<Titles>> GetTitle(RequestModel request)
            => await _masterRepository.GetTitle(request);

        public async Task<IEnumerable<Positions>> GetPosition(RequestModel request)
                    => await _masterRepository.GetPosition(request);

        public async Task<IEnumerable<ContractTypes>> GetContractType(RequestModel request)
                    => await _masterRepository.GetContractType(request);

        public async Task<IEnumerable<ReasonCategories>> GetReasonCategorie(RequestModel request)
                    => await _masterRepository.GetReasonCategorie(request);

        public async Task<IEnumerable<EnumCatagories>> GetEnum(string enumType)
            => await _masterRepository.GetEnum(enumType);

        /// <summary>
        /// lấy danh sách loại lương
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SalaryCategories>> GetSalaryCatagory()
            => await _masterRepository.GetSalaryCatagory();
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
        /// Thêm mới, cập nhật phòng ban
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
        /// Thêm mới, cập nhật chức danh
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
        /// Thêm mới, cập nhật chức vụ
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

        /// <summary>
        /// Thêm mới, cập nhật loại hợp đồng
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateContractType(string actionType, ContractTypes entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_CONTRACTTYPE:
                        response = await _masterRepository.AddContractType(entity);
                        break;
                    case ProcessConstants.PUT_CONTRACTTYPE:
                        response = await _masterRepository.UpdateContractType(entity);
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
        /// Thêm mới, cập nhật danh mục lý do
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateReasonCategorie(string actionType, ReasonCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_REASONCATEGORIE:
                        response = await _masterRepository.AddReasonCategorie(entity);
                        break;
                    case ProcessConstants.PUT_REASONCATEGORIE:
                        response = await _masterRepository.UpdateReasonCategorie(entity);
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
        /// cập nhật thông tin loại lương
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryCategory(string actionType, SalaryCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_SALARY_CATEGORY:
                        response = await _masterRepository.AddSalaryCategory(entity);
                        break;
                    case ProcessConstants.PUT_SALARY_CATEGORY:
                        response = await _masterRepository.UpdateSalaryCategory(entity);
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
