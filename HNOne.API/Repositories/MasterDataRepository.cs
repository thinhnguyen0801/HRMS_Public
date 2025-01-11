using Azure.Core;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace HNOne.API.Repositories
{
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEncryptHelper _encryptHelper;
        public MasterDataRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper
            , IEncryptHelper encryptHelper) 
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
            _encryptHelper = encryptHelper;
        }

        #region Query

        public async Task<IEnumerable<Branchs>> GetBranch()
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string strQuery = "select * from Branchs as T0 with(nolock) where T0.IsDelete = 0";
                var result = await connection.QueryAsync<Branchs>(strQuery, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }
        }

        public async Task<IEnumerable<MenuModel>> GetMenu(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                List<MenuModel> lstMenu = new List<MenuModel>();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@Type", request.type, DbType.String);
                parameters.Add("@MenuId", request.opt, DbType.String);
                var result = await connection.QueryAsync<MenuModel>(StoreConstants.STORE_H1_MENU_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (string.Equals(request.type, "AUTHENTICATION", StringComparison.OrdinalIgnoreCase))
                {
                    var groupBy = result.GroupBy(m => $"{m.parentID}-{m.menuID}");
                    foreach(var group in groupBy)
                    {
                        var header = group.First();
                        MenuModel menuModel = new MenuModel();
                        menuModel.menuID = header.menuID;
                        menuModel.menuName = header.menuName;
                        menuModel.parentID = header.parentID;
                        menuModel.parentName = header.parentName;
                        menuModel.level = header.level;
                        menuModel.ordinalNumber = header.ordinalNumber;
                        menuModel.listEvent = group.Select(m => new EventConfigModel() { eventId = m.eventId, actionName = m.actionName }).ToList();
                        lstMenu.Add(menuModel);
                    }    
                }
                else
                {
                    lstMenu = result?.ToList() ?? new List<MenuModel>();
                }
                return lstMenu;
            };
        }

        /// <summary>
        /// lấy danh sách phòng ban
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DepartmentModel>> GetDepartment(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                
                string strQuery = "select T0.*" +
                    " ,T1.BranchCode as BranchCode, T1.BranchName as BranchName" +
                    " ,T2.[Code] as HeadCode, T2.[Name] as HeadName" +
                    " ,T3.[Code] as AssistantManagerCode, T3.[Name] as AssistantManagerName" +
                    " from Departments as T0 with(nolock)" +
                    " inner join Branchs as T1 with(nolock) on T0.BranchId = T1.BranchId" +
                    " left join Employees as T2 with(nolock) on T0.HeadId = T2.Id" +
                    " left join Employees as T3 with(nolock) on T0.AssistantManagerIds = T3.Id" +
                    " where T0.IsDelete = 0 and T0.BranchId = @BranchId";
                // thêm điều kiện
                if (request.opt == CommonConstants.ENUM_ACTIVE) strQuery += " and T0.IsActive = '1'";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                var result = await connection.QueryAsync<DepartmentModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            } 
        }
        
        public async Task<IEnumerable<TitleModel>> GetTitle(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string strQuery = "select T0.*" +
                    " ,T1.BranchCode as BranchCode, T1.BranchName as BranchName" +
                    " ,T2.Code as DepartmentCode,T2.[Name] as DepartmentName" +
                    " from Titles as T0 with(nolock)" +
                    " inner join Branchs as T1 with(nolock) on T0.BranchId = T1.BranchId" +
                    " inner join Departments as T2 with(nolock) on T0.DepartmentId = T2.Id" +
                    " where T0.IsDelete = 0 and T0.BranchId = @BranchId";
                // thêm điều kiện
                if (request.opt == CommonConstants.ENUM_ACTIVE) strQuery += " and T0.IsActive = '1'";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                var result = await connection.QueryAsync<TitleModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }
        }
        public async Task<IEnumerable<PositionModel>> GetPosition(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string strQuery = "select T0.*" +
                    " ,T1.BranchCode as BranchCode, T1.BranchName as BranchName, T2.[Name] as LevelName" +
                    " from Positions as T0 with(nolock)" +
                    " inner join Branchs as T1 with(nolock) on T0.BranchId = T1.BranchId" +
                    " inner join EnumCatagories as T2 with(nolock) on T0.LevelCode = T2.Code and EnumType = 'CapDoNhanVien'" +
                    " where T0.IsDelete = 0 and T0.BranchId = @BranchId";
                // thêm điều kiện
                if (request.opt == CommonConstants.ENUM_ACTIVE) strQuery += " and T0.IsActive = '1'";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                var result = await connection.QueryAsync<PositionModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }
        }
        public async Task<IEnumerable<ContractTypeModel>> GetContractType(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string strQuery = "select T0.* from ContractTypes as T0 with(nolock) where T0.IsDelete = 0";
                var result = await connection.QueryAsync<ContractTypeModel>(strQuery, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }
        }
        public async Task<IEnumerable<ReasonCategorieModel>> GetReasonCategory(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                DynamicParameters parameters = new DynamicParameters();
                string strQuery = "select T0.*, T1.Name as TypeName" +
                    " from ReasonCategories as T0 with(nolock)" +
                    " inner join [dbo].[HRM_FN_GET_ENUM] ('LoaiLyDo', '', '') as T1 on T0.Type = T1.Code" +
                    " where T0.IsDelete = 0";
                // thêm điều kiện
                if (request.opt == CommonConstants.ENUM_ACTIVE) strQuery += " and T0.IsActive = '1'";
                if (!string.IsNullOrEmpty(request.type))
                {
                    strQuery += " and T0.Type = @EnumType";
                    parameters.Add("@EnumType", request.type, DbType.String);
                }    
                var result = await connection.QueryAsync<ReasonCategorieModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }
        }

        /// <summary>
        /// lấy danh sách enum
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EnumCatagories>> GetEnum(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EnumType", request.opt, DbType.String);
                string query = "select T0.[Id],T0.[EnumType],T0.[Code],T0.[Name] " +
                    " ,case T0.EnumType when 'CaLamViec' then FORMAT(cast(T0.[Value] as datetime), 'HH:mm') else T0.[Value] end as [Value]" +
                    " ,case T0.EnumType when 'CaLamViec' then FORMAT(cast(T0.[Value1] as datetime), 'HH:mm') else T0.[Value1] end as [Value1]" +
                    " ,case T0.EnumType when 'CaLamViec' then FORMAT(cast(T0.[Value2] as datetime), 'HH:mm') else T0.[Value2] end as [Value2]" +
                    " ,case T0.EnumType when 'CaLamViec' then FORMAT(cast(T0.[Value3] as datetime), 'HH:mm') else T0.[Value3] end as [Value3]" +
                    " ,T0.[Value4],T0.[UserSign],T0.[DateTracking],T0.[RowOrder],T0.[IsAllowEditing],T0.[EnumTypeName]" +
                    " ,T0.[DeleteReason],T0.[IsDelete],T0.[UpdateDate],T0.[UserSign2]" +
                    " from EnumCatagories as T0 with(nolock)" +
                    " where T0.IsDelete = '0'";
                if(request.opt != "AllowEdit") query += " and T0.EnumType = @EnumType"; // nếu cho phép chỉnh sửa thì lấy hết
                else
                {
                    query += " and IsAllowEditing = 1";
                }
                query += " order by T0.EnumType, T0.RowOrder";
                var results = await connection.QueryAsync<EnumCatagories>(query, parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }    
        }

        /// <summary>
        /// lấy danh sách loại lương
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SalaryCategories>> GetSalaryCatagory(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.* from SalaryCategories as T0 with(nolock)" +
                    " where T0.IsDelete = '0'";
                // thêm điều kiện
                if (request.opt == "ACTIVE") query += " and T0.IsActive = '1'";
                query += " order by T0.RowOrder";
                var results = await connection.QueryAsync<SalaryCategories>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }
        }

        /// <summary>
        /// lấy danh sách loại lương
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SalaryParameterModel>> GetSalaryParameter(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.* from SalaryParameters as T0 with(nolock)" +
                    " where T0.IsDelete = '0'";
                // thêm điều kiện
                if (request.opt == "ACTIVE") query += " and T0.IsActive = '1'";
                var results = await connection.QueryAsync<SalaryParameterModel>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }
        }

        /// <summary>
        /// lấy danh sách cấu hình lương
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SalaryConfigurationModel>> GetSalaryConfig(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.*, T1.Code as SalaryCategoryCode" +
                    ",T1.Name as SalaryCategoryName" +
                    ",T2.BranchCode, T2.BranchName, T3.Name as SalaryCalculateMethodName" +
                    " from SalaryConfigurations as T0 with(nolock) " +
                    " inner join SalaryCategories as T1 with(nolock) on T0.SalaryCategoryId = T1.Id " +
                    " inner join Branchs as T2 with(nolock) on T0.BranchId = T2.BranchId" +
                    " left join EnumCatagories as T3 with(nolock) on T0.SalaryCalculateMethod = T3.Code and T3.EnumType = 'CachTinhLuongPhuCap'" +
                    " where T0.IsDelete = '0' and T0.BranchId = @BranchId";
                var parameters = new DynamicParameters();
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                var results = await connection.QueryAsync<SalaryConfigurationModel>(query, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            };
        }
        
        /// <summary>
        /// lấy mã chứng từ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <returns></returns>
        public async Task<string?> GetDocumentNo(string? type, string? opt = "", string? opt1 = "", string? opt2 = "")
        {
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type,@Opt,@Opt1,@Opt2)";
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Type", type, DbType.String);
                    parameters.Add("@Opt", opt, DbType.String);
                    parameters.Add("@Opt1", opt1, DbType.String);
                    parameters.Add("@Opt2", opt2, DbType.String);
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    return voucherNo;
                }    
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Lấy danh sách Quốc gia, Tỉnh thành, Quận/Huyện, Xã/Phường
        /// </summary>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ComboboxModel?>> GetLocationData(string? type, string? opt = "", string? opt1 = "", string? opt2 = "")
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CountryId", opt, DbType.String);
                parameters.Add("@ProvinceId", opt1, DbType.String);
                parameters.Add("@DistrictId", opt2, DbType.String);
                parameters.Add("@Type", type, DbType.String);
                IEnumerable<ComboboxModel> dt  = await connection.QueryAsync<ComboboxModel>(StoreConstants.STORE_H1_LOCATIONDATA_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return dt;

            };
        }
        
        /// <summary>
        /// lấy danh mục master data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>?> GetMasterData(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@Type", request.type, DbType.String);
                parameters.Add("@Opt", $"{request.opt}", DbType.String);
                parameters.Add("@Opt1", $"{request.opt1}", DbType.String);
                parameters.Add("@Opt2", $"{request.opt2}", DbType.String);
                parameters.Add("@Opt3", $"{request.opt3}", DbType.String);
                var results = await connection.QueryAsync(StoreConstants.STORE_H1_MASTER_DATA_SELECT, parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return results;
            }    
        }
        
        /// <summary>
        /// lấy danh sách enum
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EnumCatagoryModel>> GetFnEnum(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EnumType", request.type, DbType.String);
                parameters.Add("@Opt", $"{request.opt}", DbType.String);
                parameters.Add("@Opt1", $"{request.opt1}", DbType.String);
                string commandText = @$"select * from {StoreConstants.FUNC_GET_ENUM}(@EnumType, @Opt, @Opt1) order by RowOrder asc";
                var results = await connection.QueryAsync<EnumCatagoryModel>(commandText, parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results?.OrderBy(m=>m.rowOrder)?.ToList() ?? new List<EnumCatagoryModel>();
            }
        }

        /// <summary>
        /// lấy danh sách danh mục mức thuế
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TaxRateModel>> GetTaxRate(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.*, T2.BranchCode, T2.BranchName" +
                    " from TaxRates as T0 with(nolock)" +
                    " inner join Branchs as T2 with(nolock) on T0.BranchId = T2.BranchId" +
                    " where T0.IsDelete = '0'";
                var results = await connection.QueryAsync<TaxRateModel>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }
        }

        /// <summary>
        /// lấy danh sách danh mục mức thuế
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DeductionConfigModel>> GetDeductionConfig(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.*, T1.Name as TypeName, T2.BranchCode, T2.BranchName" +
                    " from DeductionConfigs as T0 with(nolock)" +
                    " inner join [dbo].[HRM_FN_GET_ENUM] ('TrichNop', '', '') as T1 on T0.Type = T1.Code" +
                    " inner join Branchs as T2 with(nolock) on T0.BranchId = T2.BranchId" +
                    " where T0.IsDelete = '0'";
                var results = await connection.QueryAsync<DeductionConfigModel>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }
        }
        #endregion 

        #region Command

        /// <summary>
        /// Thêm chi nhánh
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddBranch(Branchs entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    //string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    //string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_BRANCH }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    //if (string.IsNullOrEmpty(voucherNo))
                    //{
                    //    response.status = StatusCodes.Status204NoContent;
                    //    response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                    //    return response;
                    //}
                    bool isResult = await _dbContext.Branchs.FirstOrDefaultAsync(m => m.BranchCode == entity.BranchCode) != null;
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = string.Format(MessageConstants.MESSAGE_CONFLICT_FORMAT, "Mã chi nhánh");
                        return response;
                    }
                    entity.BranchId = await _dbContext.Branchs.Select(m=>m.BranchId).DefaultIfEmpty().MaxAsync() + 1;
                    entity.BranchCode = entity.BranchCode;
                    entity.DefaultPassword = _encryptHelper.Encrypt(entity.DefaultPassword?.Trim());
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Branchs.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm chi nhánh
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateBranch(Branchs entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var branch = await _dbContext.Branchs.FirstOrDefaultAsync(m => m.BranchId == entity.BranchId);
                if(branch == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                branch.BranchName = entity.BranchName;
                branch.Address = entity.Address;
                branch.ImgUrl = entity.ImgUrl;
                branch.PhoneNumber = entity.PhoneNumber;
                branch.DefaultPassword = _encryptHelper.Encrypt(entity.DefaultPassword?.Trim());
                branch.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                branch.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                branch.UserSign2 = entity.UserSign2;
                _dbContext.Branchs.Attach(branch);
                _dbContext.Entry(branch).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception){ throw; }
        }

        /// <summary>
        /// Thêm phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddDepartment(Departments entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                bool isResult = true;
                // Tạo mới
                isResult = await _dbContext.Departments.FirstOrDefaultAsync(m => m.Code == entity.Code) != null;
                if (isResult)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = "Mã phòng ban đã tồn tại!";
                    return response;
                }
                entity.Id = await _dbContext.Departments.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.Departments.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm cập nhật phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateDepartment(Departments entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var department = await _dbContext.Departments.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (department == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                department.Name = entity.Name;
                department.ManagerId = entity.ManagerId;
                department.HeadId = entity.HeadId;
                department.AssistantManagerIds = entity.AssistantManagerIds;
                department.Remark = entity.Remark;
                department.IsActive = entity.IsActive;
                department.BranchId = entity.BranchId;
                department.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                department.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                department.UserSign2 = entity.UserSign2;
                _dbContext.Departments.Attach(department);
                _dbContext.Entry(department).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm chức vụ
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddPosition(Positions entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_POSITION }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Id = await _dbContext.Positions.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.Code = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Positions.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Cập nhật chức vụ
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdatePosition(Positions entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Positions.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.LevelCode = entity.LevelCode;
                data.Remark = entity.Remark;
                data.IsActive = entity.IsActive;
                data.BranchId = entity.BranchId;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.Positions.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddTitle(Titles entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_TITLE }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Id = await _dbContext.Titles.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.Code = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Titles.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm cập nhật phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateTitle(Titles entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Titles.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.Remark = entity.Remark;
                data.IsActive = entity.IsActive;
                data.BranchId = entity.BranchId;
                data.DepartmentId = entity.DepartmentId;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.Titles.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm loại hợp đồng
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddContractType(ContractTypes entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                entity.Id = await _dbContext.ContractTypes.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.ContractTypes.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Thêm cập nhật loại hợp đồng
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateContractType(ContractTypes entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.ContractTypes.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.Remark = entity.Remark;
                data.BranchId = entity.BranchId;
                data.StatusCode = entity.StatusCode;
                data.Duration = entity.Duration;
                data.IsIndefiniteDuration = entity.IsIndefiniteDuration;
                data.NumberOfDaysReduced = entity.NumberOfDaysReduced;
                data.IsActive = entity.IsActive;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.ContractTypes.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm danh mục lý do
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddReasonCategorie(ReasonCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    entity.Id = await _dbContext.ReasonCategories.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.ReasonCategories.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Thêm cập nhật loại hợp đồng
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateReasonCategorie(ReasonCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.ReasonCategories.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.Type = entity.Type;
                data.IsActive = entity.IsActive;
                data.Value = entity.Value;
                data.Value1 = entity.Value1;
                data.Value2 = entity.Value2;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.ReasonCategories.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm mới danh mục loại lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddSalaryCategory(SalaryCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DynamicParameters parameters = new DynamicParameters();
                    bool isResult = true;
                    string strQuery = "select count(1) from [SalaryCategories] with(nolock) where Code = @Code";
                    isResult = await connection.ExecuteScalarAsync<bool>(strQuery, new { Code = entity.Code?.Trim() });
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Mã loại lương đã tồn tại!";
                        return response;
                    }
                    entity.Id = await _dbContext.SalaryCategories.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.SalaryCategories.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }    
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// cập nhật thông tin loại lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryCategory(SalaryCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var result = await _dbContext.SalaryCategories.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (result == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                result.Name = entity.Name;
                result.RowOrder = entity.RowOrder;
                result.IsActive = entity.IsActive;
                result.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                result.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                result.UserSign2 = entity.UserSign2;
                _dbContext.SalaryCategories.Attach(result);
                _dbContext.Entry(result).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lưu thông tin cấu hình lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddSalaryConfig(SalaryConfigurations entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DynamicParameters parameters = new DynamicParameters();
                    bool isResult = true;
                    string strQuery = "select count(1) from [SalaryConfigurations] with(nolock) where SalaryCategoryId = @SalaryCategoryId and BranchId = @BranchId";
                    isResult = await connection.ExecuteScalarAsync<bool>(strQuery, new { entity.SalaryCategoryId, entity.BranchId });
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Thông tin cấu hình lương đã tồn tại. Vui lòng kiểm tra Loại lương và Chi nhánh!";
                        return response;
                    }
                    entity.Id = await _dbContext.SalaryConfigurations.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.SalaryConfigurations.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// cập nhật thông tin cấu hình tính lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryConfig(SalaryConfigurations entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var result = await _dbContext.SalaryConfigurations.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (result == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                result.IsActive = entity.IsActive;
                result.IsPersonalIncomeTax = entity.IsPersonalIncomeTax;
                result.TaxLimit = entity.TaxLimit;
                result.IsSocialInsurance = entity.IsSocialInsurance;
                result.IsHealthInsurance = entity.IsHealthInsurance;
                result.IsAccidentInsurance = entity.IsAccidentInsurance;
                result.IsOccupationalAccidentInsurance = entity.IsOccupationalAccidentInsurance;
                result.IsUnionFee = entity.IsUnionFee;
                result.IsOvertime = entity.IsOvertime;
                result.OvertimeCoefficient = entity.OvertimeCoefficient;
                result.IsNightShift = entity.IsNightShift;
                result.CoefficientNightShift = entity.CoefficientNightShift;
                result.IsAllowance = entity.IsAllowance;
                result.IsPrintContract = entity.IsPrintContract;
                result.IsProbationaryPeriod = entity.IsProbationaryPeriod;
                result.SalaryDefault = entity.SalaryDefault;
                result.SalaryCalculateMethod = entity.SalaryCalculateMethod;
                result.IsUseOfGradeLevel = entity.IsUseOfGradeLevel;
                result.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                result.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                result.UserSign2 = entity.UserSign2;
                _dbContext.SalaryConfigurations.Attach(result);
                _dbContext.Entry(result).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm chi nhánh
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddEnumCatagory(EnumCatagories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                bool isResult = true;
                // Tạo mới
                isResult = await _dbContext.EnumCatagories.FirstOrDefaultAsync(m => m.Code == entity.Code && m.EnumType == entity.EnumType) != null;
                if (isResult)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = $"Mã danh mục thuộc loại {entity.EnumTypeName} đã tồn tại!";
                    return response;
                }
                entity.Id = Guid.NewGuid();
                entity.IsAllowEditing = true;
                entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.EnumCatagories.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm chi nhánh
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateEnumCatagory(EnumCatagories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.EnumCatagories.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.Value = entity.Value;
                data.Value1 = entity.Value1;
                data.Value2 = entity.Value2;
                data.Value3 = entity.Value3;
                data.Value4 = entity.Value4;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.EnumCatagories.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        public async Task<ResponseModel> DeleteDynamic(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string tableName = _encryptHelper.Decrypt(request.type); // mã hóa dữ liệu table ra
                string _pk = _encryptHelper.Decrypt(request.opt1); // mã hóa dữ liệu table ra
                string _fk = _encryptHelper.Decrypt(request.opt2); // mã hóa dữ liệu table ra
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@TableName", tableName); // bảng nào
                parameters.Add("@ColumnName", _pk); // Primary Key
                parameters.Add("@ColumnName1", _fk); // foreign key
                parameters.Add("@Code", request.opt); // -- mã
                parameters.Add("@ReasonDelete", request.reason);
                var lstResult = await connection.QueryFirstAsync<ResponseModel>(StoreConstants.STORE_H1_DYNAMIC_DATA_DELETE, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }

        /// <summary>
        /// Thêm mới danh mục loại lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddSalaryParameter(SalaryParameters entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                entity.Id = await _dbContext.SalaryParameters.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.SalaryParameters.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// cập nhật thông tin loại lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryParameter(SalaryParameters entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var result = await _dbContext.SalaryParameters.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (result == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                result.BranchId = entity.BranchId;
                result.IsActive = entity.IsActive;
                result.TaxSalary = entity.TaxSalary;
                result.TaxSalaryProbationary = entity.TaxSalaryProbationary;
                result.SalaryFamilyCircumstanceDeduction = entity.SalaryFamilyCircumstanceDeduction;
                result.FromDate = entity.FromDate;
                result.ToDate = entity.ToDate;
                result.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                result.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                result.UserSign2 = entity.UserSign2;
                _dbContext.SalaryParameters.Attach(result);
                _dbContext.Entry(result).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }
        
        /// <summary>
        /// lưu + cập nhật thông tin danh mục thuế
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateTaxRate(string actionType, TaxRates entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if (actionType == ProcessConstants.POST_TAXT_RATE)
                {
                    // Tạo mới
                    entity.Id = await _dbContext.TaxRates.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.TaxRates.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
                // cập nhật
                var data = await _dbContext.TaxRates.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.BranchId = entity.BranchId;
                data.MinSalary = entity.MinSalary;
                data.MaxSalary = entity.MaxSalary;
                data.ProgressiveAmount = entity.ProgressiveAmount;
                data.TaxRate = entity.TaxRate;
                data.TaxBracket = entity.TaxBracket;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.TaxRates.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// lưu + cập nhật thông tin cấu hình trích nộp
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateDeductionConfig(string actionType, DeductionConfigs entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if (actionType == ProcessConstants.POST_DEDUCTION_CONFIG)
                {
                    // Tạo mới
                    entity.Id = await _dbContext.DeductionConfigs.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.DeductionConfigs.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
                // cập nhật
                var data = await _dbContext.DeductionConfigs.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.BranchId = entity.BranchId;
                data.Type = entity.Type;
                data.CoefficientEnterprise = entity.CoefficientEnterprise;
                data.CoefficientEmployee = entity.CoefficientEmployee;
                data.IsActive = entity.IsActive;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.MaxEnterprise = entity.MaxEnterprise;
                data.MaxEmployee = entity.MaxEmployee;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.DeductionConfigs.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
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
