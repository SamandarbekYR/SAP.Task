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

        public GetController(IRate _service)
        {
            this._service = _service;
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
                // Log the exception or handle it accordingly
                return StatusCode(400, "Malumotlar nato'g'ri kiritildi");
            }
        }

    }
}
