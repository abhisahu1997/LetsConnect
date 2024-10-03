using Lets_Connect.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Lets_Connect.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
    }
}
