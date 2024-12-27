using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface ITrainingRepository
    {
        Task<IEnumerable<TrainingModel>> GetTraining(RequestModel request);
        Task<ResponseModel> AddTraining(Trainings entity, IEnumerable<Training1s> lstEntity1);
        Task<ResponseModel> UpdateTraining(Trainings entity, IEnumerable<Training1s> lstEntity1);
    }
}
