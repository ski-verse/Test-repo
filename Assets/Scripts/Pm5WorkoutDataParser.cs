using System;

public static class Pm5WorkoutDataParser
{
    public static readonly Guid RowingAdditionalStatus1Uuid = new Guid("ce060032-43e5-11e4-916c-0800200c9a66");
    public static readonly Guid RowingAdditionalStatus2Uuid = new Guid("ce060033-43e5-11e4-916c-0800200c9a66");
    public static readonly Guid RowingStrokeDataUuid = new Guid("ce060035-43e5-11e4-916c-0800200c9a66");
    public static readonly Guid RowingAdditionalStrokeDataUuid = new Guid("ce060036-43e5-11e4-916c-0800200c9a66");

    public static bool TryApplyCharacteristicUpdate(Guid characteristicUuid, byte[] payload, ref Pm5WorkoutMetrics metrics)
    {
        if (payload == null || payload.Length == 0)
        {
            return false;
        }

        if (characteristicUuid == RowingAdditionalStatus1Uuid)
        {
            return TryParseAdditionalStatus1(payload, ref metrics);
        }

        if (characteristicUuid == RowingAdditionalStrokeDataUuid)
        {
            return TryParseAdditionalStrokeData(payload, ref metrics);
        }

        if (characteristicUuid == RowingStrokeDataUuid)
        {
            return TryParseStrokeData(payload, ref metrics);
        }

        if (characteristicUuid == RowingAdditionalStatus2Uuid)
        {
            return TryParseAdditionalStatus2(payload, ref metrics);
        }

        return false;
    }

    public static bool TryParseHexPayload(string hex, out byte[] payload)
    {
        payload = null;

        if (string.IsNullOrWhiteSpace(hex) || hex.Length % 2 != 0)
        {
            return false;
        }

        var bytes = new byte[hex.Length / 2];
        for (var index = 0; index < bytes.Length; index++)
        {
            if (!byte.TryParse(hex.Substring(index * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out bytes[index]))
            {
                return false;
            }
        }

        payload = bytes;
        return true;
    }

    private static bool TryParseAdditionalStatus1(byte[] payload, ref Pm5WorkoutMetrics metrics)
    {
        var changed = false;

        if (payload.Length > 5 && IsReasonableStrokeRate(payload[5]))
        {
            metrics.HasStrokeRateSpm = true;
            metrics.StrokeRateSpm = payload[5];
            changed = true;
        }

        if (payload.Length > 6 && IsReasonableHeartRate(payload[6]))
        {
            metrics.HasHeartRateBpm = true;
            metrics.HeartRateBpm = payload[6];
            changed = true;
        }

        return changed;
    }

    private static bool TryParseAdditionalStrokeData(byte[] payload, ref Pm5WorkoutMetrics metrics)
    {
        if (!TryReadUInt16(payload, 3, out var watts) || !IsReasonableWatts(watts))
        {
            return false;
        }

        metrics.HasWatts = true;
        metrics.Watts = watts;
        return true;
    }

    private static bool TryParseStrokeData(byte[] payload, ref Pm5WorkoutMetrics metrics)
    {
        if (!TryReadUInt16(payload, 18, out var totalStrokes))
        {
            return false;
        }

        metrics.HasTotalStrokes = true;
        metrics.TotalStrokes = totalStrokes;
        return true;
    }

    private static bool TryParseAdditionalStatus2(byte[] payload, ref Pm5WorkoutMetrics metrics)
    {
        if (!TryReadUInt16(payload, 4, out var averageWatts) || !IsReasonableWatts(averageWatts))
        {
            return false;
        }

        if (!metrics.HasWatts)
        {
            metrics.HasWatts = true;
            metrics.Watts = averageWatts;
            return true;
        }

        return false;
    }

    private static bool TryReadUInt16(byte[] payload, int offset, out int value)
    {
        value = 0;
        if (payload == null || payload.Length <= offset + 1)
        {
            return false;
        }

        value = payload[offset] | (payload[offset + 1] << 8);
        return true;
    }

    private static bool IsReasonableStrokeRate(int value)
    {
        return value >= 0 && value <= 90;
    }

    private static bool IsReasonableHeartRate(int value)
    {
        return value == 0 || value >= 35 && value <= 240;
    }

    private static bool IsReasonableWatts(int value)
    {
        return value >= 0 && value <= 1200;
    }
}
