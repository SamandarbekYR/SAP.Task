using Microsoft.AspNetCore.Mvc;
using SAP.API.Services;
using SAP.API.Services.Interfaces;

namespace SAP.API.Controllers
{
    [Route("api/get")]
    [ApiController]
    public class GetController : ControllerBase
    {
        private IRate _service;
        private ILogger<GetController> _logger;
        private IRedis _redis;

        public GetController(IRate _service, ILogger<GetController> logger, IRedis redis)
        {
            this._service = _service;
            this._logger = logger;
            _redis = redis;
        }
        [HttpGet]
        public async Task<ActionResult<List<Rate>>> Get(int id, string startDate, string endDate)
        {
            try
            {
                List<Rate> res = await _service.GetAll(id, DateTime.Parse(startDate), DateTime.Parse(endDate));
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Nato'g'ri ma'lumotlar kiritildi: => {ex}");
                // Log the exception or handle it accordingly
                return StatusCode(400, "Malumotlar nato'g'ri kiritildi");
            }
        }
        [HttpGet("cache-to-file")]
        public async Task<ActionResult<string?>?> GetAll(string key)
        {
            var list = await _redis.GetString(key);
            return list;
        }
        [HttpPost("file-to-cache")]
        public async Task<ActionResult<List<string>>> SetCache(string key, List<string> info)
        {
            try
            {
                if (info is not null)
                {
                    await _redis.SetAsync(key, info.ToString()!);
                    _logger.LogInformation($"Filedagi ma'lumotlar cachega saqlandi");
                }
                return Ok(info);
            }
            catch(Exception ex)
            {
                _logger.LogWarning($"Filedagi ma'lumotlar cachga saqlashda xatolik yuz berdi {ex}");
                return new List<string>();
            }
        }


    }
}
