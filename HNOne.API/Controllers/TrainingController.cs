using HNOne.API.Repositories;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingController : ControllerBase
    {
        private readonly ILogger<TrainingController> _logger;
        private readonly ITrainingRepository _trainingRepository;

        public TrainingController(ITrainingRepository trainingRepository, ILogger<TrainingController> logger)
        {
            _trainingRepository = trainingRepository;
            _logger = logger;
        }

        [HttpPost]
        [Route("get-data")]
        public async Task<IActionResult> GetData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                switch (processKey)
                {
                    case ProcessConstants.GET_TRAINING:
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }
                if ((response.data is IEnumerable<object> dataList) && dataList.IsNullOrEmpty())
                {
                    response.status = StatusCodes.Status204NoContent;
                    response.message = "Không tìm thấy dữ liệu!!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("post-data")]
        public async Task<IActionResult> PostData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                // Helper function for deserialization to reduce duplication
                T DeserializeJson<T>(string json) => JsonConvert.DeserializeObject<T>(json)!;
                switch (processKey)
                {
                    case ProcessConstants.POST_TRAINING:
                        var trainPost = DeserializeJson<Trainings>($"{request.json}");
                        var lstTrainPost = DeserializeJson<List<Training1s>>($"{request.jsonDetail}");
                        response = await _trainingRepository.AddTraining(trainPost!, lstTrainPost!);
                        break;
                    case ProcessConstants.PUT_TRAINING:
                        var trainPut = DeserializeJson<Trainings>($"{request.json}");
                        var lstTrainPut = DeserializeJson<List<Training1s>>($"{request.jsonDetail}");
                        response = await _trainingRepository.UpdateTraining(trainPut!, lstTrainPut!);
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }
    }
}
