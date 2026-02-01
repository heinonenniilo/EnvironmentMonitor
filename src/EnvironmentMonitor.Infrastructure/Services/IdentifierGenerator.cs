using EnvironmentMonitor.Domain.Interfaces;
using System;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class IdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generates a new unique identifier using ULID (Universally Unique Lexicographically Sortable Identifier).
        /// ULIDs are 26 characters long, timestamp-based, and lexicographically sortable.
        /// </summary>
        /// <returns>A ULID string.</returns>
        public string GenerateId()
        {
            return Ulid.NewUlid().ToString();
        }
    }
}
