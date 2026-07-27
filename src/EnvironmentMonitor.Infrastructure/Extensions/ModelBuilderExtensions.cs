using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;

namespace EnvironmentMonitor.Infrastructure.Extensions;

internal static class ModelBuilderExtensions
{
    public static void ApplyPostgreSqlCompatibility(this ModelBuilder modelBuilder, string? providerName)
    {
        if (providerName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            return;
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetProperties()))
        {
            var defaultValueSql = property.GetDefaultValueSql();
            if (defaultValueSql != null)
            {
                property.SetDefaultValueSql(ToPostgreSqlDefault(defaultValueSql));
            }
            if (property.GetColumnType()?.Equals("nvarchar(max)", StringComparison.OrdinalIgnoreCase) == true)
            {
                property.SetColumnType("text");
            }

            if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
            {
                var propertyName = property.Name;

                if (propertyName.Contains("Utc", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetColumnType("timestamp with time zone");
                }
                else
                {
                    property.SetColumnType("timestamp without time zone");
                }
            }
        }

        foreach (var index in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetIndexes()))
        {
            var filter = index.GetFilter();
            if (filter != null)
            {
                index.SetFilter(ToPostgreSqlFilter(filter));
            }
        }
    }

    private static string ToPostgreSqlDefault(string defaultValueSql)
    {
        return defaultValueSql.Trim().ToUpperInvariant() switch
        {
            "NEWID()" => "gen_random_uuid()",
            "GETDATE()" or "GETUTCDATE()" => "LOCALTIMESTAMP",
            _ => defaultValueSql
        };
    }

    private static string ToPostgreSqlFilter(string filter)
    {
        var translated = Regex.Replace(filter, @"\[([^\]]+)\]", "\"$1\"");
        translated = Regex.Replace(translated, @"=\s*1\b", "= TRUE");
        return Regex.Replace(translated, @"=\s*0\b", "= FALSE");
    }
}
