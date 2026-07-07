using System;
using System.Collections.Generic;
using System.Globalization;

public static class Pm5ProbeBridgeMetricsParser
{
    private const string MetricsPrefix = "PM5_METRICS|";

    public static bool TryApplyMetricsLine(string line, ref Pm5WorkoutMetrics metrics)
    {
        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith(MetricsPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var fields = ParseFields(line);
        var changed = false;

        changed |= TryApplyFloat(fields, "Watts", value =>
        {
            metrics.HasWatts = true;
            metrics.Watts = value;
        });
        changed |= TryApplyFloat(fields, "SPM", value =>
        {
            metrics.HasStrokeRateSpm = true;
            metrics.StrokeRateSpm = value;
        });
        changed |= TryApplyInt(fields, "StrokeCount", value =>
        {
            metrics.HasTotalStrokes = true;
            metrics.TotalStrokes = value;
        });
        changed |= TryApplyTime(fields, "Time", value =>
        {
            metrics.HasElapsedTimeSeconds = true;
            metrics.ElapsedTimeSeconds = value;
        });
        changed |= TryApplyFloat(fields, "Distance", value =>
        {
            metrics.HasDistanceMeters = true;
            metrics.DistanceMeters = value;
        });
        changed |= TryApplyFloat(fields, "SpeedKmh", value =>
        {
            metrics.HasSpeedKmh = true;
            metrics.SpeedKmh = value;
        });
        changed |= TryApplyTime(fields, "Pace", value =>
        {
            metrics.HasPaceSecondsPer500m = true;
            metrics.PaceSecondsPer500m = value;
        });
        changed |= TryApplyTime(fields, "AvgPace", value =>
        {
            metrics.HasAveragePaceSecondsPer500m = true;
            metrics.AveragePaceSecondsPer500m = value;
        });
        changed |= TryApplyFloat(fields, "SplitAvgWatts", value =>
        {
            metrics.HasSplitAverageWatts = true;
            metrics.SplitAverageWatts = value;
        });
        changed |= TryApplyInt(fields, "DragFactor", value =>
        {
            metrics.HasDragFactor = true;
            metrics.DragFactor = value;
        });
        if (fields.TryGetValue("HeartRate", out var rawHeartRate) && (string.IsNullOrWhiteSpace(rawHeartRate) || rawHeartRate == "null"))
        {
            metrics.HasHeartRateBpm = false;
            metrics.HeartRateBpm = 0f;
            changed = true;
        }
        else
        {
            changed |= TryApplyFloat(fields, "HeartRate", value =>
            {
                metrics.HasHeartRateBpm = true;
                metrics.HeartRateBpm = value;
            });
        }
        changed |= TryApplyInt(fields, "WorkoutState", value =>
        {
            metrics.HasWorkoutState = true;
            metrics.WorkoutState = value;
        });
        changed |= TryApplyInt(fields, "RowingState", value =>
        {
            metrics.HasRowingState = true;
            metrics.RowingState = value;
        });
        changed |= TryApplyInt(fields, "StrokeState", value =>
        {
            metrics.HasStrokeState = true;
            metrics.StrokeState = value;
        });

        return changed;
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

    private static bool TryApplyFloat(Dictionary<string, string> fields, string key, Action<float> apply)
    {
        if (!fields.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw) || raw == "null")
        {
            return false;
        }

        if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        apply(value);
        return true;
    }

    private static bool TryApplyInt(Dictionary<string, string> fields, string key, Action<int> apply)
    {
        if (!fields.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw) || raw == "null")
        {
            return false;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        apply(value);
        return true;
    }

    private static bool TryApplyTime(Dictionary<string, string> fields, string key, Action<float> apply)
    {
        if (!fields.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw) || raw == "null")
        {
            return false;
        }

        if (!TryParseTime(raw, out var seconds))
        {
            return false;
        }

        apply(seconds);
        return true;
    }

    private static bool TryParseTime(string value, out float seconds)
    {
        seconds = 0f;
        var parts = value.Split(':');
        if (parts.Length == 2 &&
            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var sec) &&
            int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
        {
            seconds = minutes * 60f + sec;
            return true;
        }

        if (parts.Length == 3 &&
            int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours) &&
            int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes) &&
            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out sec))
        {
            seconds = hours * 3600f + minutes * 60f + sec;
            return true;
        }

        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
    }
}
