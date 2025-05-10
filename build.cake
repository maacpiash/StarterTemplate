/*
 * MIT License
 *
 * Copyright (c) 2025 Mohammad Abdul Ahad Chowdhury
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Diagnostics;

// SETTINGS

const string red = "\u001b[31m";
const string green = "\u001b[32m";
const string inverse = "\u001b[7m";
const string reset = "\u001b[0m";

var target = Argument("t", "dev"); // default target is "dev"
var srcDir = Directory("./src");
var dotnetProject = GetFiles($"{srcDir}/**/*.csproj").FirstOrDefault();

// TASKS

Task("dev").Does(() =>
{
	var bunPath = Context.Tools.Resolve("bun");

	if (bunPath is null)
	{
		Error("Could not find `bun` in `$PATH`.");
		return;
	}
	Information("Using tool: {0}", bunPath);

	Process[] processes = new Process[2];
	processes[0] = StartAndLog("dotnet", "watch run", srcDir);
	processes[1] = StartAndLog("bun", "run dev", srcDir);

	Console.CancelKeyPress += (s, e) =>
	{
		e.Cancel = true;
		foreach (var proc in processes)
		{
			if (!proc.HasExited)
				proc.Kill();
		}
	};

	foreach (var proc in processes)
	{
		proc.WaitForExit();
	}
});

// HELPERS

Process StartAndLog(string cmd, string args, DirectoryPath workingDir)
{
	Information($"Starting `{cmd} {args}` in {workingDir}");

	var process = Process.Start(new ProcessStartInfo
	{
		FileName = cmd,
		Arguments = args,
		WorkingDirectory = workingDir.FullPath,
		RedirectStandardOutput = true,
		RedirectStandardError = true,
		UseShellExecute = false,
		CreateNoWindow = false
	});

	process.OutputDataReceived += (s, e) => Information($"{inverse}{cmd}{reset}\t {e.Data}");
	process.ErrorDataReceived += (s, e) => Error($"{inverse}{cmd}{reset}\t {e.Data}");
	process.BeginOutputReadLine();
	process.BeginErrorReadLine();

	return process;
}

// EXECUTE

RunTarget(target);
