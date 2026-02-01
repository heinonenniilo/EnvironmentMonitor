using System;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IIdentifierGenerator
    {
        /// <summary>
        /// Generates a new unique identifier string.
        /// </summary>
        /// <returns>A unique identifier string.</returns>
        string GenerateId();
    }
}
