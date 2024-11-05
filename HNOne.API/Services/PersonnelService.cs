using HNOne.API.Repositories.Interfaces;
using HNOne.API.Services.Interfaces;
using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Common;
using HNOne.Model.Models;

namespace HNOne.API.Services
{
    public class PersonnelService : IPersonnelService
    {
        private readonly IPersonnelRepository _personnelRepository;
        public PersonnelService(IPersonnelRepository personnelRepository)
        {
            _personnelRepository = personnelRepository;
        }

        #region Query
        /// <summary>
        /// lấy danh sách nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EmployeeModel>> GetEmployee(RequestModel request)
            => await _personnelRepository.GetEmployee(request);

        /// <summary>
        /// lấy danh sách hợp đồng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ContractModel>> GetContract(RequestModel request)
            => await _personnelRepository.GetContract(request);
        /// <summary>
        /// lấy danh sách quan hệ gia đình
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FamilyRelationships>> GetFamilyRelationship(int employeeId)
            => await _personnelRepository.GetFamilyRelationship(employeeId);
        #endregion


        #region Command
        public async Task<ResponseModel> UpdateEmployee(string actionType, Employees entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_EMPLOYEE:
                        response = await _personnelRepository.AddEmployee(entity);
                        break;
                    case ProcessConstants.PUT_EMPLOYEE:
                        response = await _personnelRepository.UpdateEmployee(entity);
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
        /// cập nhật thông tin hợp đồng
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <param name="lstSalaryConfig"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateContract(string actionType, Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_CONTRACT:
                        response = await _personnelRepository.AddContract(entity, lstSalaryConfig);
                        break;
                    case ProcessConstants.PUT_CONTRACT:
                        response = await _personnelRepository.UpdateContract(entity, lstSalaryConfig);
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
        /// cập nhật thông tin mối quan hệ gia đình
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <param name="lstSalaryConfig"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateFamilyRelationship(string actionType, FamilyRelationships entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_FAMILYRELATIONSHIP:
                        response = await _personnelRepository.AddFamilyRelationship(entity);
                        break;
                    case ProcessConstants.PUT_FAMILYRELATIONSHIP:
                        response = await _personnelRepository.UpdateFamilyRelationship(entity);
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
