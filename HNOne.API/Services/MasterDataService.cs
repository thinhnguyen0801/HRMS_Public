using HNOne.API.Repositories;
using HNOne.API.Repositories.Interfaces;
using HNOne.API.Services.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using System.Data;

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
        public async Task<IEnumerable<MenuModel>> GetMenu(RequestModel request)
            => await _masterRepository.GetMenu(request);

        /// <summary>
        /// lấy ra danh sách chi nhánh
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Branchs>> GetBranch(RequestModel request)
            => await _masterRepository.GetBranch(request);

        /// <summary>
        /// lấy ra danh sách phòng ban
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DepartmentModel>> GetDepartment(RequestModel request)
            => await _masterRepository.GetDepartment(request);

        public async Task<IEnumerable<TitleModel>> GetTitle(RequestModel request)
            => await _masterRepository.GetTitle(request);

        public async Task<IEnumerable<TitleModel>> GetSubDepartment(RequestModel request)
            => await _masterRepository.GetSubDepartment(request);

        public async Task<IEnumerable<PositionModel>> GetPosition(RequestModel request)
                    => await _masterRepository.GetPosition(request);

        public async Task<IEnumerable<ContractTypeModel>> GetContractType(RequestModel request)
                    => await _masterRepository.GetContractType(request);

        public async Task<IEnumerable<ReasonCategorieModel>> GetReasonCategory(RequestModel request)
                    => await _masterRepository.GetReasonCategory(request);

        public async Task<IEnumerable<EnumCatagories>> GetEnum(RequestModel request)
            => await _masterRepository.GetEnum(request);

        /// <summary>
        /// lấy danh sách loại lương
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SalaryCategories>> GetSalaryCatagory(RequestModel request)
            => await _masterRepository.GetSalaryCatagory(request);

        public async Task<IEnumerable<SalaryParameterModel>> GetSalaryParameter(RequestModel request)
            => await _masterRepository.GetSalaryParameter(request);

        public async Task<IEnumerable<SalaryConfigurationModel>> GetSalaryConfig(RequestModel request)
           => await _masterRepository.GetSalaryConfig(request);
        
        /// <summary>
        /// lấy mã số chứng từ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <returns></returns>
        public async Task<string?> GetDocumentNo(string? type, int branchId, string? opt = "", string? opt1 = "", string? opt2 = "")
            => await _masterRepository.GetDocumentNo(type, branchId, opt, opt1, opt2);

        /// <summary>
        /// Lấy danh sách Quốc gia, Tỉnh thành, Quận/Huyện, Xã/Phường
        /// </summary>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ComboboxModel?>> GetLocationData(string? type, string? opt = "", string? opt1 = "", string? opt2 = "")
            => await _masterRepository.GetLocationData(type, opt, opt1, opt2);

        public async Task<IEnumerable<dynamic>?> GetMasterData(RequestModel request)
            => await _masterRepository.GetMasterData(request);
        
        public async Task<IEnumerable<EnumCatagoryModel>> GetFnEnum(RequestModel request)
            => await _masterRepository.GetFnEnum(request);
        
        public async Task<IEnumerable<TaxRateModel>> GetTaxRate(RequestModel request)
            => await _masterRepository.GetTaxRate(request);

        public async Task<IEnumerable<DeductionConfigModel>> GetDeductionConfig(RequestModel request)
            => await _masterRepository.GetDeductionConfig(request);
        
        public async Task<IEnumerable<WorkingBranchModel>> GetWorkingBranch(RequestModel request)
            => await _masterRepository.GetWorkingBranch(request);
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

        /// <summary>
        /// Cập nhật thông tin cấu hình tính lương
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryConfig(string actionType, SalaryConfigurations entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_SALARY_CONFIG:
                        response = await _masterRepository.AddSalaryConfig(entity);
                        break;
                    case ProcessConstants.PUT_SALARY_CONFIG:
                        response = await _masterRepository.UpdateSalaryConfig(entity);
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
        /// Cập nhật thông số lương
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryParameter(string actionType, SalaryParameters entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_SALARY_PARAMETER:
                        response = await _masterRepository.AddSalaryParameter(entity);
                        break;
                    case ProcessConstants.PUT_SALARY_PARAMETER:
                        response = await _masterRepository.UpdateSalaryParameter(entity);
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

        public async Task<ResponseModel> UpdateEnumCatagory(string actionType, EnumCatagories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_ENUM_CATA:
                        response = await _masterRepository.AddEnumCatagory(entity);
                        break;
                    case ProcessConstants.PUT_ENUM_CATA:
                        response = await _masterRepository.UpdateEnumCatagory(entity);
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
        /// xóa bảng động
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseModel> DeleteDynamic(RequestModel request)
            => await _masterRepository.DeleteDynamic(request);

        /// <summary>
        /// cập nhật thông tin danh mục tính thuế
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateTaxRate(string actionType, TaxRates entity)
           => await _masterRepository.UpdateTaxRate(actionType, entity);

        /// <summary>
        /// cập nhật thông tin danh mục tính thuế
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateDeductionConfig(string actionType, DeductionConfigs entity)
           => await _masterRepository.UpdateDeductionConfig(actionType, entity);

        /// <summary>
        /// Import dữ liệu vào hệ thống
        /// </summary>
        /// <param name="branchId"></param>
        /// <param name="userId"></param>
        /// <param name="processKey"></param>
        /// <param name="jData"></param>
        /// <returns></returns>
        public async Task<ResponseModel> ImportData(int branchId, int userId, string processKey, string jData)
            => await _masterRepository.ImportData(branchId, userId, processKey, jData);

        /// <summary>
        /// cập nhật thông báo thành đã đọc
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateStatusNotification(RequestModel request)
            => await _masterRepository.UpdateStatusNotification(request);

        /// <summary>
        /// Thêm mới & cập nhật thông tin chi nhánh làm việc
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateWorkingBranch(string actionType, WorkingBranchs entity)
            => await _masterRepository.UpdateWorkingBranch(actionType, entity);

        /// <summary>
        /// Thêm mới, cập nhật bộ phận
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSubDepartment(string actionType, SubDepartments entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_SUB_DEPARTMENT:
                        response = await _masterRepository.AddSubDepartment(entity);
                        break;
                    case ProcessConstants.PUT_SUB_DEPARTMENT:
                        response = await _masterRepository.UpdateSubDepartment(entity);
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
