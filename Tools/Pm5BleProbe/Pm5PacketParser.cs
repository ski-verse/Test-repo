using System.Globalization;
using System.Text;

internal sealed class Pm5PacketParser
{
    private static readonly Guid GeneralStatusUuid = new("ce060031-43e5-11e4-916c-0800200c9a66");
    private static readonly Guid AdditionalStatus1Uuid = new("ce060032-43e5-11e4-916c-0800200c9a66");
    private static readonly Guid AdditionalStrokeDataUuid = new("ce060036-43e5-11e4-916c-0800200c9a66");
    private static readonly Guid MultiplexedInformationUuid = new("ce060080-43e5-11e4-916c-0800200c9a66");

    private readonly Pm5ParsedMetrics state = new();

    public Pm5ParsedMetrics Parse(string sourceName, Guid characteristicUuid, byte[] raw)
    {
        state.Source = sourceName;
        state.MultiplexedId = null;
        state.ParseWarning = null;

        if (raw.Length == 0)
        {
            state.ParseWarning = "empty packet";
            return state.Copy();
        }

        if (characteristicUuid == MultiplexedInformationUuid)
        {
            var id = raw[0];
            state.MultiplexedId = id;
            var payload = raw.Skip(1).ToArray();
            ParseByConcept2Id(id, payload);
            return state.Copy();
        }

        if (characteristicUuid == GeneralStatusUuid)
        {
            ParseGeneralStatus(raw);
        }
        else if (characteristicUuid == AdditionalStatus1Uuid)
        {
            ParseAdditionalStatus1(raw);
        }
        else if (characteristicUuid == AdditionalStrokeDataUuid)
        {
            ParseAdditionalStrokeData(raw);
        }
        else
        {
            state.ParseWarning = $"unsupported characteristic {characteristicUuid}";
        }

        return state.Copy();
    }

    private void ParseByConcept2Id(byte id, byte[] payload)
    {
        switch (id)
        {
            case 0x31:
                ParseGeneralStatus(payload);
                break;
            case 0x32:
                ParseAdditionalStatus1(payload);
                break;
            case 0x33:
                ParseAdditionalStatus2(payload);
                break;
            case 0x35:
                ParseStrokeData(payload);
                break;
            case 0x36:
                ParseAdditionalStrokeData(payload);
                break;
            default:
                state.ParseWarning = $"unsupported multiplexed id 0x{id:X2}";
                break;
        }
    }

    private void ParseGeneralStatus(byte[] raw)
    {
        if (raw.Length < 19)
        {
            state.ParseWarning = $"general status packet too short: {raw.Length}";
            return;
        }

        state.ElapsedTimeSeconds = ReadUInt24(raw, 0) / 100.0;
        state.DistanceMeters = ReadUInt24(raw, 3) / 10.0;
        state.WorkoutType = raw[6];
        state.IntervalType = raw[7];
        state.WorkoutState = raw[8];
        state.RowingState = raw[9];
        state.StrokeState = raw[10];
        state.TotalWorkDistanceMeters = ReadUInt24(raw, 11);
        state.WorkoutDuration = ReadUInt24(raw, 14);
        state.WorkoutDurationType = raw[17];
        state.DragFactor = raw[18];
    }

    private void ParseAdditionalStatus1(byte[] raw)
    {
        if (raw.Length < 11)
        {
            state.ParseWarning = $"additional status 1 packet too short: {raw.Length}";
            return;
        }

        state.ElapsedTimeSeconds = ReadUInt24(raw, 0) / 100.0;
        state.SpeedMetersPerSecond = ReadUInt16(raw, 3) / 1000.0;
        state.StrokeRateSpm = raw[5];
        state.HeartRateBpm = raw[6] == 255 ? null : raw[6];
        state.CurrentPaceSecondsPer500m = ReadUInt16(raw, 7) / 100.0;
        state.AveragePaceSecondsPer500m = ReadUInt16(raw, 9) / 100.0;
        if (raw.Length >= 13)
        {
            state.RestDistanceMeters = ReadUInt16(raw, 11);
        }

        if (raw.Length >= 16)
        {
            state.RestTimeSeconds = ReadUInt24(raw, 13) / 100.0;
        }

        if (raw.Length >= 18)
        {
            state.AveragePowerWatts = ReadUInt16(raw, 16);
        }

        if (raw.Length >= 19)
        {
            state.ErgMachineType = raw[18];
        }
    }

    private void ParseAdditionalStatus2(byte[] raw)
    {
        if (raw.Length < 10)
        {
            state.ParseWarning = $"additional status 2 packet too short: {raw.Length}";
            return;
        }

        state.ElapsedTimeSeconds = ReadUInt24(raw, 0) / 100.0;
        state.IntervalCount = raw[3];
        if (raw.Length >= 6)
        {
            state.TotalCalories = ReadUInt16(raw, 4);
        }

        if (raw.Length >= 8)
        {
            state.SplitAveragePaceSecondsPer500m = ReadUInt16(raw, 6) / 100.0;
        }

        state.SplitAveragePowerWatts = ReadUInt16(raw, 8);
        if (raw.Length >= 12)
        {
            state.SplitAverageCaloriesPerHour = ReadUInt16(raw, 10);
        }

        if (raw.Length >= 15)
        {
            state.LastSplitTimeSeconds = ReadUInt24(raw, 12) / 10.0;
        }

        if (raw.Length >= 18)
        {
            state.LastSplitDistanceMeters = ReadUInt24(raw, 15);
        }
    }

    private void ParseStrokeData(byte[] raw)
    {
        if (raw.Length < 3)
        {
            state.ParseWarning = $"stroke data packet too short: {raw.Length}";
            return;
        }

        state.ElapsedTimeSeconds = ReadUInt24(raw, 0) / 100.0;
        if (raw.Length >= 6)
        {
            state.DistanceMeters = ReadUInt24(raw, 3) / 10.0;
        }

        if (raw.Length >= 7)
        {
            state.DriveLengthMeters = raw[6] / 100.0;
        }

        if (raw.Length >= 8)
        {
            state.DriveTimeSeconds = raw[7] / 100.0;
        }

        if (raw.Length >= 10)
        {
            state.StrokeRecoveryTimeSeconds = ReadUInt16(raw, 8) / 100.0;
        }

        if (raw.Length >= 12)
        {
            state.StrokeDistanceMeters = ReadUInt16(raw, 10) / 100.0;
        }

        if (raw.Length >= 14)
        {
            state.PeakDriveForcePounds = ReadUInt16(raw, 12) / 10.0;
        }

        if (raw.Length >= 16)
        {
            state.AverageDriveForcePounds = ReadUInt16(raw, 14) / 10.0;
        }

        if (raw.Length >= 18)
        {
            state.StrokeCount = ReadUInt16(raw, 16);
        }
    }

    private void ParseAdditionalStrokeData(byte[] raw)
    {
        if (raw.Length < 5)
        {
            state.ParseWarning = $"additional stroke data packet too short: {raw.Length}";
            return;
        }

        if (raw.Length >= 3)
        {
            state.ElapsedTimeSeconds = ReadUInt24(raw, 0) / 100.0;
        }

        state.StrokePowerWatts = ReadUInt16(raw, 3);
        if (raw.Length >= 7)
        {
            state.StrokeCaloriesPerHour = ReadUInt16(raw, 5);
        }

        if (raw.Length >= 9)
        {
            state.StrokeCount = ReadUInt16(raw, 7);
        }

        if (raw.Length >= 12)
        {
            state.ProjectedWorkTimeSeconds = ReadUInt24(raw, 9);
        }

        if (raw.Length >= 15)
        {
            state.ProjectedWorkDistanceMeters = ReadUInt24(raw, 12);
        }

        if (raw.Length >= 17)
        {
            state.WorkPerStrokeJoules = ReadUInt16(raw, 15) / 10.0;
        }
    }

    private static int ReadUInt16(byte[] raw, int offset)
    {
        return raw[offset] | (raw[offset + 1] << 8);
    }

    private static int ReadUInt24(byte[] raw, int offset)
    {
        return raw[offset] | (raw[offset + 1] << 8) | (raw[offset + 2] << 16);
    }
}

internal sealed class Pm5ParsedMetrics
{
    public string Source { get; set; } = "";
    public int? MultiplexedId { get; set; }
    public string? ParseWarning { get; set; }
    public double? ElapsedTimeSeconds { get; set; }
    public double? DistanceMeters { get; set; }
    public double? SpeedMetersPerSecond { get; set; }
    public int? StrokeRateSpm { get; set; }
    public int? HeartRateBpm { get; set; }
    public double? CurrentPaceSecondsPer500m { get; set; }
    public double? AveragePaceSecondsPer500m { get; set; }
    public int? AveragePowerWatts { get; set; }
    public int? StrokePowerWatts { get; set; }
    public int? StrokeCount { get; set; }
    public int? WorkoutType { get; set; }
    public int? IntervalType { get; set; }
    public int? WorkoutState { get; set; }
    public int? RowingState { get; set; }
    public int? StrokeState { get; set; }
    public double? TotalWorkDistanceMeters { get; set; }
    public double? WorkoutDuration { get; set; }
    public int? WorkoutDurationType { get; set; }
    public int? DragFactor { get; set; }
    public double? RestDistanceMeters { get; set; }
    public double? RestTimeSeconds { get; set; }
    public int? ErgMachineType { get; set; }
    public int? StrokeCaloriesPerHour { get; set; }
    public double? ProjectedWorkTimeSeconds { get; set; }
    public double? ProjectedWorkDistanceMeters { get; set; }
    public double? WorkPerStrokeJoules { get; set; }
    public int? IntervalCount { get; set; }
    public int? TotalCalories { get; set; }
    public double? SplitAveragePaceSecondsPer500m { get; set; }
    public int? SplitAveragePowerWatts { get; set; }
    public int? SplitAverageCaloriesPerHour { get; set; }
    public double? LastSplitTimeSeconds { get; set; }
    public double? LastSplitDistanceMeters { get; set; }
    public double? DriveLengthMeters { get; set; }
    public double? DriveTimeSeconds { get; set; }
    public double? StrokeRecoveryTimeSeconds { get; set; }
    public double? StrokeDistanceMeters { get; set; }
    public double? PeakDriveForcePounds { get; set; }
    public double? AverageDriveForcePounds { get; set; }

    public int? DisplayWatts => StrokePowerWatts ?? AveragePowerWatts ?? SplitAveragePowerWatts;

    public Pm5ParsedMetrics Copy()
    {
        return (Pm5ParsedMetrics)MemberwiseClone();
    }

    public string ToLogLine()
    {
        var builder = new StringBuilder("PARSED");
        Append(builder, "Source", Source);
        if (MultiplexedId.HasValue)
        {
            Append(builder, "MuxId", $"0x{MultiplexedId.Value:X2}");
        }

        Append(builder, "Time", FormatTime(ElapsedTimeSeconds));
        Append(builder, "Distance", FormatMeters(DistanceMeters));
        Append(builder, "SpeedKmh", SpeedMetersPerSecond.HasValue ? FormatNumber(SpeedMetersPerSecond.Value * 3.6, "0.0") : null);
        Append(builder, "SPM", StrokeRateSpm);
        Append(builder, "Watts", DisplayWatts);
        Append(builder, "StrokeCount", StrokeCount);
        Append(builder, "HeartRate", HeartRateBpm);
        Append(builder, "Pace", FormatTime(CurrentPaceSecondsPer500m));
        Append(builder, "AvgPace", FormatTime(AveragePaceSecondsPer500m));
        Append(builder, "SplitAvgWatts", SplitAveragePowerWatts);
        Append(builder, "WorkoutState", WorkoutState);
        Append(builder, "RowingState", RowingState);
        Append(builder, "StrokeState", StrokeState);
        Append(builder, "DragFactor", DragFactor);

        if (!string.IsNullOrWhiteSpace(ParseWarning))
        {
            Append(builder, "Warning", ParseWarning);
        }

        return builder.ToString();
    }

    private static void Append(StringBuilder builder, string name, object? value)
    {
        builder.Append('|');
        builder.Append(name);
        builder.Append('=');
        builder.Append(value?.ToString() ?? "null");
    }

    private static string? FormatMeters(double? meters)
    {
        return meters.HasValue ? $"{FormatNumber(meters.Value, "0.0")}m" : null;
    }

    private static string? FormatTime(double? seconds)
    {
        if (!seconds.HasValue)
        {
            return null;
        }

        var value = TimeSpan.FromSeconds(seconds.Value);
        return value.TotalHours >= 1
            ? $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}.{value.Milliseconds / 10:00}"
            : $"{value.Minutes:00}:{value.Seconds:00}.{value.Milliseconds / 10:00}";
    }

    private static string FormatNumber(double value, string format)
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }
}
