using Microsoft.AspNetCore.Mvc;
using SAP.API.Entities.Rates;
using SAP.API.Services.Interfaces;

namespace SAP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetController : ControllerBase
    {
        private IRate _service;
        private ILogger<GetController> _logger;

        public GetController(IRate _service, ILogger<GetController> logger)
        {
            this._service = _service;
            this._logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<List<Rate>>> Get(int id, string startDate, string endDate)
        {
            try
            {
                List<Rate> res = await _service.GetAll(id, DateTime.Parse(startDate),DateTime.Parse(endDate));
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Nato'g'ri ma'lumotlar kiritildi: => {ex}");
                // Log the exception or handle it accordingly
                return StatusCode(400, "Malumotlar nato'g'ri kiritildi");
            }
        }

    }
}
