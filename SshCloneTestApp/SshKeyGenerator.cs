using System;
using System.Diagnostics;
using System.IO;

namespace SshCloneTestApp;

/// <summary>
/// Generates SSH keys by shelling out to ssh-keygen and returns the key material as strings.
/// </summary>
public static class SshKeyGenerator
{
    /// <summary>The private and public key material for a freshly generated key.</summary>
    public sealed record GeneratedKey(string PrivateKey, string PublicKey, string Comment);

    /// <summary>
    /// Generates a fresh key for <paramref name="spec"/> in a unique temp directory and
    /// reads the private and public key files into memory.
    /// </summary>
    public static GeneratedKey Generate(KeySpec spec)
    {
        string dir = Path.Combine(Path.GetTempPath(), "sshkey-" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        try
        {
            string keyPath = Path.Combine(dir, "id");
            string comment = $"libgit2sharp-ssh-test-{spec.Label}";

            var psi = new ProcessStartInfo("ssh-keygen")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            psi.ArgumentList.Add("-t");
            psi.ArgumentList.Add(spec.SshKeygenType);
            if (spec.Bits is int bits)
            {
                psi.ArgumentList.Add("-b");
                psi.ArgumentList.Add(bits.ToString());
            }
            if (spec.UsePemFormat)
            {
                psi.ArgumentList.Add("-m");
                psi.ArgumentList.Add("PEM");
            }
            psi.ArgumentList.Add("-N");
            psi.ArgumentList.Add(string.Empty); // empty passphrase
            psi.ArgumentList.Add("-C");
            psi.ArgumentList.Add(comment);
            psi.ArgumentList.Add("-f");
            psi.ArgumentList.Add(keyPath);
            psi.ArgumentList.Add("-q");

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start ssh-keygen (is OpenSSH installed?)");

            // Drain stderr asynchronously while reading stdout to avoid a pipe-buffer deadlock
            // if ssh-keygen writes a large message to stderr (e.g. on a failure path).
            var stderrTask = process.StandardError.ReadToEndAsync();
            string stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string stderr = stderrTask.GetAwaiter().GetResult();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"ssh-keygen exited {process.ExitCode} for {spec.Label}.\nstdout: {stdout}\nstderr: {stderr}");
            }

            string privateKey = File.ReadAllText(keyPath);
            string publicKey = File.ReadAllText(keyPath + ".pub");
            return new GeneratedKey(privateKey, publicKey, comment);
        }
        finally
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // best-effort cleanup so generated private key material isn't left in temp
            }
        }
    }
}
