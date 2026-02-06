using Microsoft.AspNetCore.Mvc;

namespace SEB.FPE.Telemetry.TestApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simple GET endpoint to test request/response logging
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Telemetry test endpoint - GET request",
                timestamp = DateTime.UtcNow,
                status = "success",
                method = HttpContext.Request.Method,
                path = HttpContext.Request.Path.Value,
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            });
        }

        /// <summary>
        /// POST endpoint to test request body logging
        /// </summary>
        [HttpPost]
        public IActionResult Post([FromBody] TestRequest request)
        {
            return Ok(new
            {
                message = "Telemetry test endpoint - POST request",
                timestamp = DateTime.UtcNow,
                status = "success",
                receivedData = request,
                method = HttpContext.Request.Method
            });
        }

        /// <summary>
        /// Endpoint that simulates an error to test exception logging
        /// </summary>
        [HttpGet("error")]
        public IActionResult GetError()
        {
            try
            {
                throw new InvalidOperationException("This is a test exception to verify exception logging in telemetry middleware");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test error endpoint called");
                throw; // Re-throw to let middleware catch it
            }
        }

        /// <summary>
        /// Endpoint that simulates a slow request to test duration logging
        /// </summary>
        [HttpGet("slow")]
        public async Task<IActionResult> GetSlow()
        {
            await Task.Delay(2000); // Simulate 2 second delay
            return Ok(new
            {
                message = "Slow request completed",
                duration = "~2000ms",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Endpoint to test query parameter masking
        /// </summary>
        [HttpGet("query")]
        public IActionResult GetWithQuery([FromQuery] string token, [FromQuery] string id, [FromQuery] string password)
        {
            return Ok(new
            {
                message = "Query parameter test",
                note = "Check logs to see if sensitive parameters (token, password) are masked",
                receivedParams = new
                {
                    token = token ?? "not provided",
                    id = id ?? "not provided",
                    password = password != null ? "***provided***" : "not provided"
                },
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Endpoint to test header logging
        /// </summary>
        [HttpGet("headers")]
        public IActionResult GetWithHeaders()
        {
            var customHeader = HttpContext.Request.Headers["X-Custom-Header"].ToString();
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

            return Ok(new
            {
                message = "Header test endpoint",
                note = "Check logs to see if Authorization header is masked",
                headers = new
                {
                    customHeader = customHeader ?? "not provided",
                    authorization = authHeader != null ? "***provided***" : "not provided"
                },
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Endpoint that returns different status codes
        /// </summary>
        [HttpGet("status/{code}")]
        public IActionResult GetStatus(int code)
        {
            return StatusCode(code, new
            {
                message = $"Status code {code} response",
                timestamp = DateTime.UtcNow
            });
        }
    }

    public class TestRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }
        public string? Password { get; set; }
    }
}
