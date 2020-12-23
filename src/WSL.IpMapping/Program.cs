using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WSL.IpMapping
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Environment.OSVersion);
            Console.WriteLine("WSL IP Mapping");
            Console.WriteLine();

            using var process = new Process();
            process.StartInfo.FileName = "C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe";
            process.StartInfo.Arguments = "wsl ip addr show eth0 \n wsl service docker start";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.Start();
            await process.WaitForExitAsync();

            var ex = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(ex))
            {
                Console.WriteLine($"Execute command exception：{process.StartInfo.Arguments}\n{ex}");
                return;
            }
            var output = await process.StandardOutput.ReadToEndAsync();

            var match = Regex.Match(output, @"inet\s(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})/20");
            if (!match.Success)
            {
                Console.WriteLine($"No match WSL IP：{output}");
                return;
            }
            string ip = match.Groups[1].Value;
            Console.WriteLine(ip);
            Console.WriteLine();

            var path = "C:/Windows/System32/drivers/etc/hosts";
            var text = await File.ReadAllTextAsync(path);
            match = Regex.Match(text, @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\swsl.local");
            if (match.Success)
            {
                text = text.Replace(match.Groups[1].Value, ip);
            }
            else
            {
                text += $"\n{ip} wsl.local";
            }

            await File.WriteAllTextAsync(path, text);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Successful！");
            Console.ResetColor();
        }
    }
}
