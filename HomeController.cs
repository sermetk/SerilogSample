using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SerilogSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Route("/Validation")]
        public IActionResult Validation()
        {
            throw new ValidationException("Missing fields");
        }

        [HttpGet]
        [Route("/Exception")]
        public IActionResult Exception()
        {
            throw new Exception();
        }

        [HttpGet]
        [Route("/Success")]
        public IActionResult Success()
        {
            return new JsonResult("Success");
        }
    }
}
