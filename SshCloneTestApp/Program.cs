using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibGit2Sharp;

namespace SshCloneTestApp;

public static class Program
{
    private const string RepoUrl = "git@github.com:OctopusDeploy/libgit2sharp.git";

    public static int Main()
    {
        Console.WriteLine($"libgit2 version : {GlobalSettings.Version}");
        Console.WriteLine($"features        : {GlobalSettings.Version.Features}");
        Console.WriteLine($"repository url  : {RepoUrl}");
        Console.WriteLine($"os              : {RuntimeInformation.OSDescription}");
        Console.WriteLine();
        Console.WriteLine("Each key is freshly generated and unknown to the server, so an");
        Console.WriteLine("AUTHENTICATION FAILURE is the expected (passing) outcome: it proves the");
        Console.WriteLine("SSH transport, host-key exchange and in-memory key handling all worked.");
        Console.WriteLine();

        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        var rows = new List<(string Label, string Status, string Detail)>();
        bool allPassed = true;

        foreach (var spec in KeySpec.All)
        {
            if (isWindows && !spec.SupportedOnWindows)
            {
                Console.WriteLine($"=== {spec.Label}: SKIPPED — WinCNG has no {spec.SshKeygenType} support ===");
                Console.WriteLine();
                rows.Add((spec.Label, "SKIPPED", "WinCNG backend has no support for this key type"));
                continue;
            }

            Console.WriteLine($"=== {spec.Label} ===");
            try
            {
                var key = SshKeyGenerator.Generate(spec);
                Console.WriteLine($"generated key   : {spec.Label} ({key.Comment})");

                var result = AuthProbe.Probe(RepoUrl, key);
                string status = result.IsPass ? "PASS" : "FAIL";
                if (!result.IsPass)
                {
                    allPassed = false;
                }

                Console.WriteLine($"outcome         : {result.Outcome} -> {status}");
                Console.WriteLine($"detail          : {result.Detail}");
                rows.Add((spec.Label, $"{status} ({result.Outcome})", result.Detail));
            }
            catch (Exception ex)
            {
                allPassed = false;
                Console.Error.WriteLine($"harness error   : {ex.GetType().Name}: {ex.Message}");
                rows.Add((spec.Label, "FAIL (harness error)", ex.Message));
            }

            Console.WriteLine();
        }

        Console.WriteLine("==================== SUMMARY ====================");
        foreach (var row in rows)
        {
            Console.WriteLine($"  {row.Label,-16} {row.Status}");
        }
        Console.WriteLine("================================================");
        Console.WriteLine(allPassed ? "RESULT: PASS" : "RESULT: FAIL");

        return allPassed ? 0 : 1;
    }
}
