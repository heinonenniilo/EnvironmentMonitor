using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.AddModels
{
    public class AddAttachmentModel<TIdentifier>
    {
        public required Attachment Attachment { get; set; }
        public required TIdentifier Identifier { get; set; }
        public required bool SaveChanges { get; set; }
    }
}
