using Microsoft.AspNetCore.Mvc;
using SAP.API.Services;
using SAP.API.Services.Interfaces;
using System.Formats.Asn1;

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
        [HttpGet("key")]
        public async Task<ActionResult<string?>?> GetAll(string key)
        {
            var list = await _redis.GetString(key);
            return list;
        }
        

    }
}
