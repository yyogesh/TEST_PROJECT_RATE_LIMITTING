using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMS.ContentEngine;
using CMS.Websites;

namespace Kentico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "API is working!",
                timestamp = DateTime.Now 
            });
        }

        [HttpGet("pages")]
        public async Task<IActionResult> GetPages()
        {
            // This is a simple test endpoint
            // You can expand this to query Kentico content
            return Ok(new { 
                pages = new[] { "Page 1", "Page 2" },
                message = "Custom API endpoint working"
            });
        }
    }
}