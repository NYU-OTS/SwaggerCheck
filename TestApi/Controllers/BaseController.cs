using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OTSS.TestApi.Controllers
{
    /// <summary>
    /// A base class for API controllers.
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// A logger instance for this controller.
        /// </summary>
        protected ILogger Logger { get; }

        protected BaseController(ILogger logger)
        {
            Logger = logger;
        }
    }
}
