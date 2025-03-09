using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Exceptions
{
    public class EntityNotFoundException : BaseException
    {
        public EntityNotFoundException(string message = "")
            : base("Not found", (int)HttpStatusCode.NotFound, message)
        {
        }
    }
}
