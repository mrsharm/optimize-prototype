using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Analysis;
using Etlx = Microsoft.Diagnostics.Tracing.Etlx;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            return;
        }

        string loh = args[0];
        var lohVal = Convert.ToUInt64(args[0]).ToString("X");
        string traceFile = $"{loh}.etl.zip";

        using Process process = new Process();
        process.StartInfo.FileName = "crank";
        process.StartInfo.Arguments = "--config C:\\Users\\musharm\\source\\repos\\performance\\artifacts\\bin\\GC.Infrastructure\\Debug\\net7.0\\Commands\\RunCommand\\BaseSuite\\CrankConfiguration.yaml --scenario gcperfsim --application.environmentVariables COMPlus_GCHeapCount=0xC  --application.variables.tc 22  --application.variables.tagb 540  --application.variables.tlgb 2  --application.variables.lohar 100  --application.variables.pohar 0  --application.variables.sohsr 100-4000  --application.variables.lohsr 102400-204800  --application.variables.pohsr 100-204800  --application.variables.sohsi 50  --application.variables.lohsi 50  --application.variables.pohsi 0  --application.variables.sohpi 0  --application.variables.lohpi 0  --application.variables.sohfi 0  --application.variables.lohfi 0  --application.variables.pohfi 0 --application.variables.allocType reference  --application.variables.testKind time --application.collect true  --application.collectStartup true  --application.collectArguments GCCollectOnly  --application.framework net8.0  --profile aspnet-perf-win ";

        process.StartInfo.Arguments += $" --application.environmentVariables COMPlus_GCLOHThreshold=0x{loh} ";
        process.StartInfo.Arguments += $" --application.options.traceOutput {lohVal}.etl.zip";

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;


        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit(1000 * 1000);

        if (Path.GetExtension(traceFile) == ".zip")
        {
            var zippedReader = new ZippedETLReader(traceFile);
            zippedReader.UnpackArchive();
            traceFile = zippedReader.EtlFileName;
        }

        Etlx.TraceLog traceLog = Etlx.TraceLog.OpenOrConvert(traceFile);
        var eventSource = traceLog.Events.GetSource();
        eventSource.NeedLoadedDotNetRuntimes();
        eventSource.Process();
        var processes = eventSource.Processes();

        TraceLoadedDotNetRuntime? managedProcess = null;
        foreach (var p in processes)
        {
            managedProcess = p.LoadedDotNetRuntime();
            if (!p.Name.Contains("GCPerfSim"))
            {
                continue;
            }

            else
            {
                break;
            }
        }

        Console.WriteLine(managedProcess.GC.Stats().GetGCPauseTimePercentage());
    }
}