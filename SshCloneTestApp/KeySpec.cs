using System.Collections.Generic;

namespace SshCloneTestApp;

/// <summary>
/// Describes one SSH key type the harness exercises.
/// </summary>
/// <param name="Label">Human-readable identifier used in output and key comments.</param>
/// <param name="SshKeygenType">The value passed to <c>ssh-keygen -t</c>.</param>
/// <param name="Bits">Value for <c>ssh-keygen -b</c>, or null to omit the flag.</param>
/// <param name="UsePemFormat">When true, emit a classic PEM key (<c>-m PEM</c>) for broad
/// libssh2 compatibility. ED25519 has no PEM form and must use the native OpenSSH format.</param>
/// <param name="SupportedOnWindows">False for key types the libssh2 WinCNG backend cannot
/// handle (ED25519), which the harness skips on Windows.</param>
public sealed record KeySpec(
    string Label,
    string SshKeygenType,
    int? Bits,
    bool UsePemFormat,
    bool SupportedOnWindows)
{
    /// <summary>
    /// The full key-type matrix. ED25519 is unsupported by the libssh2 WinCNG backend.
    /// </summary>
    public static IReadOnlyList<KeySpec> All { get; } = new[]
    {
        new KeySpec("rsa-4096", "rsa", 4096, UsePemFormat: true, SupportedOnWindows: true),
        new KeySpec("ecdsa-nistp256", "ecdsa", 256, UsePemFormat: true, SupportedOnWindows: true),
        new KeySpec("ed25519", "ed25519", null, UsePemFormat: false, SupportedOnWindows: true),
    };
}
