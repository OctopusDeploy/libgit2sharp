using System;
using System.Collections.Generic;
using System.IO;
using LibGit2Sharp;

namespace SshCloneTestApp;

/// <summary>The classified result of one clone attempt.</summary>
public enum Outcome
{
    /// <summary>Authentication was rejected — the SSH pipeline worked end to end. PASS.</summary>
    AuthFailure,

    /// <summary>The key could not be parsed or its algorithm is unsupported by the backend. FAIL.</summary>
    KeyParseOrUnsupported,

    /// <summary>The clone unexpectedly succeeded with a throwaway key. FAIL.</summary>
    UnexpectedSuccess,

    /// <summary>The failure matched no known signature. FAIL — needs investigation/calibration.</summary>
    Unknown,
}

/// <summary>The outcome of a probe plus a human-readable detail string.</summary>
public sealed record ProbeResult(Outcome Outcome, string Detail)
{
    public bool IsPass => Outcome == Outcome.AuthFailure;
}

/// <summary>
/// Attempts an SSH clone with an in-memory key and classifies the result.
/// </summary>
public static class AuthProbe
{
    // Signatures (matched case-insensitively) indicating the key could not be loaded or the
    // algorithm is unsupported by the active crypto backend. CHECKED FIRST, because a
    // backend may wrap a parse failure inside a generic "failed to authenticate" message.
    private static readonly string[] ParseOrUnsupportedSignatures =
    {
        "extract public key",
        "unable to extract",
        "unsupported",
        "unimplemented",
        "invalid privatekey",
        "unable to parse",
        "failed to initialize ssh",
        "could not load",
        "wrong passphrase",
    };

    // Signatures indicating authentication was attempted and rejected (the expected outcome).
    private static readonly string[] AuthFailureSignatures =
    {
        "authentication",
        "authenticate",
        "too many redirects or authentication replays",
        "permission denied",
        "combination invalid",
        "username/publickey",
        "username does not match",
        "callback returned an invalid",
    };

    /// <summary>Clones <paramref name="url"/> with the given in-memory key and classifies the result.</summary>
    public static ProbeResult Probe(string url, SshKeyGenerator.GeneratedKey key)
    {
        var options = new CloneOptions
        {
            FetchOptions =
            {
                CredentialsProvider = (_, userFromUrl, _) => new SshKeyMemoryCredentials
                {
                    Username = string.IsNullOrEmpty(userFromUrl) ? "git" : userFromUrl,
                    PublicKey = key.PublicKey,
                    PrivateKey = key.PrivateKey,
                    Passphrase = string.Empty,
                },
                CertificateCheck = (_, _, _) => true, // accept the host key; part of "the process working"
            },
        };

        string destination = Path.Combine(Path.GetTempPath(), "octossh-" + Path.GetRandomFileName());

        try
        {
            Repository.Clone(url, destination, options);
            return new ProbeResult(Outcome.UnexpectedSuccess,
                "Clone succeeded with a throwaway key — the key must not be authorized.");
        }
        catch (Exception ex)
        {
            string message = Flatten(ex);
            string lower = message.ToLowerInvariant();

            foreach (var sig in ParseOrUnsupportedSignatures)
            {
                if (lower.Contains(sig))
                {
                    return new ProbeResult(Outcome.KeyParseOrUnsupported, message);
                }
            }

            foreach (var sig in AuthFailureSignatures)
            {
                if (lower.Contains(sig))
                {
                    return new ProbeResult(Outcome.AuthFailure, message);
                }
            }

            return new ProbeResult(Outcome.Unknown, message);
        }
        finally
        {
            TryDelete(destination);
        }
    }

    private static string Flatten(Exception ex)
    {
        var parts = new List<string>();
        for (Exception? e = ex; e != null; e = e.InnerException)
        {
            parts.Add($"{e.GetType().Name}: {e.Message}");
        }
        return string.Join(" | ", parts);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup of the (empty/partial) clone target
        }
    }
}
