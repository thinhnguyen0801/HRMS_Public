using HNOne.Common;
using HNOne.API.Repositories.Interfaces;
using HNOne.Model.Entities;
using HNOne.Model;
using Dapper;
using HNOne.API.Constants;
using System.Data;
using Microsoft.EntityFrameworkCore;
using HNOne.Model.Models;
using Newtonsoft.Json;

namespace HNOne.API.Repositories
{
    public class TrainingRepository : ITrainingRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;

        public TrainingRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query
        public async Task<IEnumerable<TrainingModel>> GetTraining(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@DocumentId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                IEnumerable<TrainingModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_TRAINING_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<TrainingModel>();
                    if (request.documentId > 0)
                    {
                        var lstDetail = dtResult.Read<Training1Model>();
                        string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }
                }
                return lstResult ?? new List<TrainingModel>();
            }
        }
        #endregion

        #region Command
        /// <summary>
        /// Thêm mới chứn từ đào tạo
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddTraining(Trainings entity, IEnumerable<Training1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Type", GlobalConstants.TABLE_TRAINING, DbType.String);
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    entity.Id = await _dbContext.Trainings.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.VoucherNo = voucherNo;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.Trainings.AddAsync(entity);
                    foreach (var item in lstEntity1)
                    {
                        Training1s entity1 = new Training1s();
                        entity1.TrainId = entity.Id;
                        entity1.EmployeeId = item.EmployeeId;
                        entity1.IsAbsent = item.IsAbsent;
                        entity1.NoteForAll = item.NoteForAll;
                        entity1.Remark = item.Remark;
                        entity1.DateTracking = dateTimeNow;
                        entity1.UserSign = entity.UserSign;
                        await _dbContext.Training1s.AddAsync(entity1);
                    }
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    response.data = entity.Id;
                }    
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// cập nhật chứn từ đào tạo
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateTraining(Trainings entity, IEnumerable<Training1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Trainings.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                if (data.DateTracking != entity.DateTracking)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                    return response;
                }
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.EmployeeSignatureId = entity.EmployeeSignatureId;
                data.TrainingCourseName = entity.TrainingCourseName;
                data.TypeOfTraning = entity.TypeOfTraning;
                data.TraningFormatCode = entity.TraningFormatCode;
                data.Address = entity.Address;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.Content = entity.Content;
                data.Objectives = entity.Objectives;
                data.NoteForAll = entity.NoteForAll;
                data.Remark = entity.Remark;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                _dbContext.Trainings.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                // thêm chi tiết
                // bỏ dữ liệu củ đi
                var lstRequest1s = await _dbContext.Training1s.Where(m => m.TrainId == data.Id).ToListAsync();
                if (!lstRequest1s.IsNullOrEmpty()) _dbContext.Training1s.RemoveRange(lstRequest1s);
                foreach (var item in lstEntity1)
                {
                    Training1s entity1 = new Training1s();
                    entity1.TrainId = entity.Id;
                    entity1.EmployeeId = item.EmployeeId;
                    entity1.IsAbsent = item.IsAbsent;
                    entity1.NoteForAll = item.NoteForAll;
                    entity1.Remark = item.Remark;
                    entity1.DateTracking = dateTimeNow;
                    entity1.UserSign = entity.UserSign;
                    await _dbContext.Training1s.AddAsync(entity1);
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                response.data = data.Id;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }
        
        /// <summary>
        /// cập nhật đánh giá đào tạo
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateEvaluateTraining(Trainings entity, IEnumerable<Training1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Trainings.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                var lstRequest1s = await _dbContext.Training1s.Where(m => m.TrainId == data.Id).ToListAsync();
                if(!lstRequest1s.IsNullOrEmpty())
                {
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    foreach (var item in lstRequest1s)
                    {
                        foreach(var entity1 in lstEntity1)
                        {
                            if (item.Id != entity1.Id) continue;
                            item.IsAbsent = entity1.IsAbsent;
                            item.NoteForAll = entity1.NoteForAll;
                            item.Remark = entity1.Remark;
                            item.DateTracking = dateTimeNow;
                            item.UserSign = entity.UserSign2;
                            _dbContext.Training1s.Attach(item);
                            _dbContext.Entry(item).State = EntityState.Modified;
                        }    
                    }
                }    
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                response.data = data.Id;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion
    }
}
