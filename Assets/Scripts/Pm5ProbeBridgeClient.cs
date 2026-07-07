using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class Pm5ProbeBridgeClient : IPm5BleClient, IPm5WorkoutDataClient, IPm5BleClientPump
{
    private const string LogPrefix = "[Ski-Verse PM5 Bridge] ";
    private readonly object gate = new object();
    private readonly Queue<string> pendingLines = new Queue<string>();
    private readonly List<Pm5BleDeviceInfo> discoveredDevices = new List<Pm5BleDeviceInfo>();
    private readonly string probeProjectPath;
    private readonly string dotnetExecutable;
    private readonly int scanSeconds;

    private Process bridgeProcess;
    private int bridgeGeneration;
    private int activeBridgeGeneration;
    private Pm5BleConnectionStatus status = Pm5BleConnectionStatus.NotConnected;
    private Pm5WorkoutDataStatus dataStatus = Pm5WorkoutDataStatus.WaitingForWorkoutData;
    private Pm5WorkoutMetrics latestWorkoutMetrics;

    public Pm5ProbeBridgeClient()
        : this(GetDefaultProbeProjectPath(), "dotnet", 30)
    {
    }

    public Pm5ProbeBridgeClient(string probeProjectPath, string dotnetExecutable = "dotnet", int scanSeconds = 30)
    {
        this.probeProjectPath = probeProjectPath;
        this.dotnetExecutable = string.IsNullOrWhiteSpace(dotnetExecutable) ? "dotnet" : dotnetExecutable;
        this.scanSeconds = Mathf.Clamp(scanSeconds, 5, 300);
    }

    public event Action StateChanged;
    public event Action WorkoutDataChanged;

    public Pm5BleConnectionStatus Status
    {
        get
        {
            lock (gate)
            {
                return status;
            }
        }
    }

    public Pm5WorkoutDataStatus DataStatus
    {
        get
        {
            lock (gate)
            {
                return dataStatus;
            }
        }
    }

    public IReadOnlyList<Pm5BleDeviceInfo> DiscoveredDevices
    {
        get
        {
            lock (gate)
            {
                return discoveredDevices.ToArray();
            }
        }
    }

    public bool HasWorkoutData
    {
        get
        {
            lock (gate)
            {
                return latestWorkoutMetrics.HasAnyMetrics;
            }
        }
    }

    public Pm5WorkoutMetrics LatestWorkoutMetrics
    {
        get
        {
            lock (gate)
            {
                return latestWorkoutMetrics;
            }
        }
    }

    public void StartScan()
    {
        UnityEngine.Debug.Log(LogPrefix + "Starting Pm5BleProbe bridge process.");
        StopBridgeProcess();
        lock (gate)
        {
            discoveredDevices.Clear();
            latestWorkoutMetrics = default;
        }

        SetStatus(Pm5BleConnectionStatus.Searching);
        SetDataStatus(Pm5WorkoutDataStatus.WaitingForWorkoutData);
        StartBridgeProcess();
    }

    public void StopScan()
    {
        StopBridgeProcess();
        SetStatus(Pm5BleConnectionStatus.NotConnected);
        SetDataStatus(Pm5WorkoutDataStatus.WaitingForWorkoutData);
    }

    public void Connect(Pm5BleDeviceInfo device)
    {
        UnityEngine.Debug.Log($"{LogPrefix}Connect requested for bridge device '{device.Name}'. The bridge process owns BLE connect/select.");
        if (bridgeProcess == null || bridgeProcess.HasExited)
        {
            StartScan();
            return;
        }

        SetStatus(Pm5BleConnectionStatus.Connecting);
    }

    public void Pump()
    {
        while (true)
        {
            string line;
            lock (gate)
            {
                if (pendingLines.Count == 0)
                {
                    break;
                }

                line = pendingLines.Dequeue();
            }

            HandleBridgeLine(line);
        }
    }

    public static string BuildProbeArguments(string projectPath, int scanSeconds)
    {
        return $"run --project {Quote(projectPath)} -- --bridge --scan-seconds {Mathf.Clamp(scanSeconds, 5, 300).ToString(CultureInfo.InvariantCulture)}";
    }

    public static string GetDefaultProbeProjectPath()
    {
        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        return projectRoot == null
            ? Path.Combine("Tools", "Pm5BleProbe", "Pm5BleProbe.csproj")
            : Path.Combine(projectRoot, "Tools", "Pm5BleProbe", "Pm5BleProbe.csproj");
    }

    private void StartBridgeProcess()
    {
        if (!File.Exists(probeProjectPath))
        {
            UnityEngine.Debug.LogWarning(LogPrefix + "Pm5BleProbe project not found: " + probeProjectPath);
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = dotnetExecutable,
            Arguments = BuildProbeArguments(probeProjectPath, scanSeconds),
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Directory.GetParent(probeProjectPath)?.FullName ?? Application.dataPath,
        };

        var processGeneration = ++bridgeGeneration;
        activeBridgeGeneration = processGeneration;

        bridgeProcess = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        bridgeProcess.OutputDataReceived += (_, args) => EnqueueLine(args.Data);
        bridgeProcess.ErrorDataReceived += (_, args) => EnqueueLine(args.Data);
        bridgeProcess.Exited += (_, _) => EnqueueLine("PM5_BRIDGE_EXITED|Generation=" + processGeneration.ToString(CultureInfo.InvariantCulture));

        try
        {
            bridgeProcess.Start();
            bridgeProcess.BeginOutputReadLine();
            bridgeProcess.BeginErrorReadLine();
            UnityEngine.Debug.Log(LogPrefix + "Bridge process started: " + dotnetExecutable + " " + startInfo.Arguments);
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogWarning(LogPrefix + "Bridge process failed to start: " + exception.Message);
            StopBridgeProcess();
            SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
        }
    }

    private void StopBridgeProcess()
    {
        if (bridgeProcess == null)
        {
            return;
        }

        try
        {
            if (!bridgeProcess.HasExited)
            {
                UnityEngine.Debug.Log(LogPrefix + "Stopping bridge process.");
                bridgeProcess.Kill();
            }
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogWarning(LogPrefix + "Could not stop bridge process: " + exception.Message);
        }
        finally
        {
            activeBridgeGeneration = 0;
            bridgeProcess.Dispose();
            bridgeProcess = null;
        }
    }

    private void EnqueueLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        lock (gate)
        {
            pendingLines.Enqueue(line);
        }
    }

    private void HandleBridgeLine(string line)
    {
        if (line.StartsWith("PM5_BRIDGE_EXITED", StringComparison.Ordinal))
        {
            var fields = ParseFields(line);
            fields.TryGetValue("Generation", out var rawGeneration);
            if (int.TryParse(rawGeneration, NumberStyles.Integer, CultureInfo.InvariantCulture, out var exitedGeneration) &&
                exitedGeneration != activeBridgeGeneration)
            {
                return;
            }

            UnityEngine.Debug.LogWarning(LogPrefix + "Bridge process exited.");
            if (Status != Pm5BleConnectionStatus.NotConnected)
            {
                SetStatus(Pm5BleConnectionStatus.ConnectionFailed);
            }

            return;
        }

        if (line.StartsWith("PM5_STATUS|", StringComparison.Ordinal))
        {
            ApplyStatus(line.Substring("PM5_STATUS|".Length));
            return;
        }

        if (line.StartsWith("PM5_DATA_STATUS|", StringComparison.Ordinal))
        {
            ApplyDataStatus(line.Substring("PM5_DATA_STATUS|".Length));
            return;
        }

        if (line.StartsWith("PM5_DEVICE|", StringComparison.Ordinal))
        {
            ApplyDevice(line);
            return;
        }

        if (line.StartsWith("PM5_METRICS|", StringComparison.Ordinal))
        {
            ApplyMetrics(line);
            return;
        }

        if (line.Contains("ERROR|", StringComparison.Ordinal) || line.Contains("WARN|", StringComparison.Ordinal))
        {
            UnityEngine.Debug.LogWarning(LogPrefix + line);
        }
        else
        {
            UnityEngine.Debug.Log(LogPrefix + line);
        }
    }

    private void ApplyStatus(string value)
    {
        switch (value)
        {
            case "Searching":
                SetStatus(Pm5BleConnectionStatus.Searching);
                break;
            case "Connecting":
                SetStatus(Pm5BleConnectionStatus.Connecting);
                break;
            case "Connected":
                UnityEngine.Debug.Log(LogPrefix + "PM5 connected through bridge.");
                SetStatus(Pm5BleConnectionStatus.Connected);
                break;
            default:
                break;
        }
    }

    private void ApplyDataStatus(string value)
    {
        switch (value)
        {
            case "SubscribingToWorkoutNotifications":
                SetDataStatus(Pm5WorkoutDataStatus.SubscribingToWorkoutNotifications);
                break;
            case "ReceivingLiveData":
                SetDataStatus(Pm5WorkoutDataStatus.ReceivingLiveData);
                break;
            case "NotificationSubscriptionFailed":
                SetDataStatus(Pm5WorkoutDataStatus.NotificationSubscriptionFailed);
                break;
            default:
                break;
        }
    }

    private void ApplyDevice(string line)
    {
        var fields = ParseFields(line);
        fields.TryGetValue("Name", out var name);
        fields.TryGetValue("Address", out var rawAddress);
        ulong.TryParse(rawAddress, NumberStyles.Integer, CultureInfo.InvariantCulture, out var address);

        var device = new Pm5BleDeviceInfo("pm5-bridge-" + address.ToString(CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(name) ? "Concept2 PM5" : name, address);
        lock (gate)
        {
            var exists = false;
            for (var i = 0; i < discoveredDevices.Count; i++)
            {
                if (discoveredDevices[i].BluetoothAddress == device.BluetoothAddress)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                discoveredDevices.Add(device);
            }
        }

        UnityEngine.Debug.Log($"{LogPrefix}PM5 bridge discovered device '{device.Name}' ({device.BluetoothAddress}).");
        if (Status == Pm5BleConnectionStatus.Searching)
        {
            SetStatus(Pm5BleConnectionStatus.Pm5Found);
        }
        else
        {
            StateChanged?.Invoke();
        }
    }

    private void ApplyMetrics(string line)
    {
        Pm5WorkoutMetrics updatedMetrics;
        var changed = false;
        lock (gate)
        {
            updatedMetrics = latestWorkoutMetrics;
            changed = Pm5ProbeBridgeMetricsParser.TryApplyMetricsLine(line, ref updatedMetrics);
            if (changed)
            {
                latestWorkoutMetrics = updatedMetrics;
            }
        }

        if (!changed)
        {
            return;
        }

        if (Status != Pm5BleConnectionStatus.Connected)
        {
            SetStatus(Pm5BleConnectionStatus.Connected);
        }

        SetDataStatus(Pm5WorkoutDataStatus.ReceivingLiveData);
        UnityEngine.Debug.Log($"{LogPrefix}PM5 metrics received. Watts={(updatedMetrics.HasWatts ? updatedMetrics.Watts.ToString("0", CultureInfo.InvariantCulture) : "--")}, SPM={(updatedMetrics.HasStrokeRateSpm ? updatedMetrics.StrokeRateSpm.ToString("0", CultureInfo.InvariantCulture) : "--")}, Strokes={(updatedMetrics.HasTotalStrokes ? updatedMetrics.TotalStrokes.ToString(CultureInfo.InvariantCulture) : "--")}.");
        WorkoutDataChanged?.Invoke();
        StateChanged?.Invoke();
    }

    private void SetStatus(Pm5BleConnectionStatus nextStatus)
    {
        var changed = false;
        lock (gate)
        {
            if (status != nextStatus)
            {
                status = nextStatus;
                changed = true;
            }
        }

        if (changed)
        {
            StateChanged?.Invoke();
        }
    }

    private void SetDataStatus(Pm5WorkoutDataStatus nextStatus)
    {
        var changed = false;
        lock (gate)
        {
            if (dataStatus != nextStatus)
            {
                dataStatus = nextStatus;
                changed = true;
            }
        }

        if (changed)
        {
            StateChanged?.Invoke();
        }
    }

    private static Dictionary<string, string> ParseFields(string line)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        var parts = line.Split('|');
        for (var i = 1; i < parts.Length; i++)
        {
            var separator = parts[i].IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            fields[parts[i].Substring(0, separator)] = parts[i].Substring(separator + 1);
        }

        return fields;
    }

    private static string Quote(string value)
    {
        return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
    }
}
