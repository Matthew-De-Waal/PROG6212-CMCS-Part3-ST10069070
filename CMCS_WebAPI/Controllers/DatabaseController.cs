using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CMCS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DatabaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
