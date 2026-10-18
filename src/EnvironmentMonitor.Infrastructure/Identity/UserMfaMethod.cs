using EnvironmentMonitor.Domain.Enums;
using System;

namespace EnvironmentMonitor.Infrastructure.Identity;

/// <summary>
/// A multi factor authentication method which has been set up for a user.
/// A row is created when the method is confirmed and kept (with <see cref="Enabled"/> set to false)
/// when the method is disabled, so that the history remains visible.
/// </summary>
public class UserMfaMethod
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public MfaMethods Method { get; set; }
    public bool Enabled { get; set; }

    /// <summary>
    /// When the method was last confirmed with a valid code.
    /// </summary>
    public DateTime? ConfirmedUtc { get; set; }

    /// <summary>
    /// When the method was last disabled.
    /// </summary>
    public DateTime? DisabledUtc { get; set; }

    public DateTime? Updated { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public string? UpdatedById { get; set; }
    public ApplicationUser? UpdatedBy { get; set; }
}
