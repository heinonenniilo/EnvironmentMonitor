using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Exceptions
{
    public class DuplicateEntityException : BaseException
    {
        public DuplicateEntityException(string message = "")
            : base("Entity already exists", (int)HttpStatusCode.Conflict, message)
        {
        }
    }
}
