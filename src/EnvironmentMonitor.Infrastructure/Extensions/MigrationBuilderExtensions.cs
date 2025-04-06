using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Infrastructure.Data;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Extensions
{
    public static class MigrationBuilderExtensions
    {
        public static void FillLocationDefaultRow(this MigrationBuilder builder)
        {
            builder.InsertData
            (
                table: "Locations",
                columns: new[] { "Id", "Name" },
                values: new object[] { 0, "Default" }
            );
        }
    }
}
