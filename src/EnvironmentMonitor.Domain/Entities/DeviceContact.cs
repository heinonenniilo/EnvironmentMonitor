using System;
using System.ComponentModel.DataAnnotations;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceContact : TrackedEntity
    {
        public int Id { get; set; }
        public Guid Identifier { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        
        [Required]
        [MaxLength(1024)]
        public required string Email { get; set; }
    }
}
