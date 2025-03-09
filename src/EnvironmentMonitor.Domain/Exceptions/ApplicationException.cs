using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }
        public string Detail { get; }

        protected BaseException(string message, int statusCode = 500, string detail = "")
            : base(message)
        {
            StatusCode = statusCode;
            Detail = detail;
        }
    }
}
