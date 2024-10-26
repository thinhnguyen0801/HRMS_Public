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
        #endregion
    }
}
