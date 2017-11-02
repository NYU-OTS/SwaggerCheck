using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OTSS.TestApi.Controllers
{
    [Route("/")]
    public class RootController : BaseController
    {
        public RootController(ILogger<RootController> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Returns the current version of the API
        /// </summary>
        [HttpGet]
        public object Version()
        {
            return new {
                // We prefer to use Semantic Versioning, so we only return the
                // first three components of the version number
                version = typeof(Startup).GetTypeInfo().Assembly.GetName()?.Version?.ToString(3) ?? "0.0.0"
             };
        }
    }
}
