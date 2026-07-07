using System.Globalization;

internal static class Pm5ParserSelfTests
{
    public static int Run()
    {
        var failures = new List<string>();

        var parser = new Pm5PacketParser();

        var generalStatus = parser.Parse("General Status", new Guid("ce060031-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x10, 0x27, 0x00,
            0xD2, 0x04, 0x00,
            0x01, 0x02, 0x03, 0x04, 0x05,
            0xD2, 0x04, 0x00,
            0x10, 0x27, 0x00,
            0x00, 0x7D,
        });
        Expect(failures, generalStatus.ElapsedTimeSeconds, 100.0, "General Status elapsed time");
        Expect(failures, generalStatus.DistanceMeters, 123.4, "General Status distance");
        Expect(failures, generalStatus.WorkoutState, 3, "General Status workout state");
        Expect(failures, generalStatus.RowingState, 4, "General Status rowing state");
        Expect(failures, generalStatus.StrokeState, 5, "General Status stroke state");

        var additionalStatus = parser.Parse("Additional Status 1", new Guid("ce060032-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x88, 0x13, 0x00,
            0x88, 0x13,
            0x1C,
            0x92,
            0x10, 0x27,
            0x20, 0x4E,
            0x34, 0x12,
            0x10, 0x27, 0x00,
            0xC5, 0x00,
            0x0A,
        });
        Expect(failures, additionalStatus.ElapsedTimeSeconds, 50.0, "Additional Status elapsed time");
        Expect(failures, additionalStatus.SpeedMetersPerSecond, 5.0, "Additional Status speed");
        Expect(failures, additionalStatus.StrokeRateSpm, 28, "Additional Status stroke rate");
        Expect(failures, additionalStatus.HeartRateBpm, 146, "Additional Status heart rate");
        Expect(failures, additionalStatus.CurrentPaceSecondsPer500m, 100.0, "Additional Status current pace");
        Expect(failures, additionalStatus.AveragePowerWatts, 197, "Additional Status average watts");

        var directAdditionalStatus17 = new Pm5PacketParser().Parse("Additional Status 1", new Guid("ce060032-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x88, 0x13, 0x00,
            0x88, 0x13,
            0x1C,
            0x92,
            0x10, 0x27,
            0x20, 0x4E,
            0x34, 0x12,
            0x10, 0x27, 0x00,
            0x00,
        });
        Expect(failures, directAdditionalStatus17.ParseWarning, null, "Direct 17-byte Additional Status warning");
        Expect(failures, directAdditionalStatus17.SpeedMetersPerSecond, 5.0, "Direct 17-byte Additional Status speed");
        Expect(failures, directAdditionalStatus17.StrokeRateSpm, 28, "Direct 17-byte Additional Status stroke rate");
        Expect(failures, directAdditionalStatus17.HeartRateBpm, 146, "Direct 17-byte Additional Status heart rate");
        Expect(failures, directAdditionalStatus17.CurrentPaceSecondsPer500m, 100.0, "Direct 17-byte Additional Status current pace");
        Expect(failures, directAdditionalStatus17.AveragePaceSecondsPer500m, 200.0, "Direct 17-byte Additional Status average pace");

        var strokeData = parser.Parse("Additional Stroke Data", new Guid("ce060036-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x70, 0x17, 0x00,
            0xFA, 0x00,
            0x20, 0x03,
            0x2A, 0x00,
            0x00, 0x00, 0x00,
            0x10, 0x27, 0x00,
            0x34, 0x12,
        });
        Expect(failures, strokeData.ElapsedTimeSeconds, 60.0, "Additional Stroke Data elapsed time");
        Expect(failures, strokeData.StrokePowerWatts, 250, "Additional Stroke Data stroke watts");
        Expect(failures, strokeData.StrokeCount, 42, "Additional Stroke Data stroke count");

        var partialStrokeData = new Pm5PacketParser().Parse("Additional Stroke Data", new Guid("ce060036-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x70, 0x17, 0x00,
            0xFA, 0x00,
            0x20, 0x03,
            0x2A, 0x00,
        });
        Expect(failures, partialStrokeData.StrokePowerWatts, 250, "Partial Additional Stroke Data stroke watts");
        Expect(failures, partialStrokeData.StrokeCount, 42, "Partial Additional Stroke Data stroke count");

        var multiplexed = parser.Parse("Multiplexed Information", new Guid("ce060080-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x32,
            0x88, 0x13, 0x00,
            0x88, 0x13,
            0x1C,
            0x92,
            0x10, 0x27,
            0x20, 0x4E,
            0x34, 0x12,
            0x10, 0x27, 0x00,
            0xC5, 0x00,
            0x0A,
        });
        Expect(failures, multiplexed.MultiplexedId, 0x32, "Multiplexed ID");
        Expect(failures, multiplexed.StrokeRateSpm, 28, "Multiplexed stroke rate");
        Expect(failures, multiplexed.AveragePowerWatts, 197, "Multiplexed average watts");

        var multiplexedAdditionalStatus2 = new Pm5PacketParser().Parse("Multiplexed Information", new Guid("ce060080-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x33,
            0x70, 0x17, 0x00,
            0x01,
            0x64, 0x00,
            0x10, 0x27,
            0xDE, 0x00,
            0x20, 0x03,
            0x10, 0x27, 0x00,
            0xF4, 0x01, 0x00,
        });
        Expect(failures, multiplexedAdditionalStatus2.MultiplexedId, 0x33, "Multiplexed Additional Status 2 ID");
        Expect(failures, multiplexedAdditionalStatus2.DisplayWatts, 222, "Multiplexed Additional Status 2 watts");

        var multiplexedStrokeData = new Pm5PacketParser().Parse("Multiplexed Information", new Guid("ce060080-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x35,
            0x70, 0x17, 0x00,
            0xD2, 0x04, 0x00,
            0x96,
            0x42,
            0x10, 0x27,
            0x20, 0x03,
            0x34, 0x12,
            0x78, 0x56,
            0x4D, 0x00,
        });
        Expect(failures, multiplexedStrokeData.MultiplexedId, 0x35, "Multiplexed Stroke Data ID");
        Expect(failures, multiplexedStrokeData.StrokeCount, 77, "Multiplexed Stroke Data stroke count");

        var multiplexedAdditionalStroke = new Pm5PacketParser().Parse("Multiplexed Information", new Guid("ce060080-43e5-11e4-916c-0800200c9a66"), new byte[]
        {
            0x36,
            0x70, 0x17, 0x00,
            0xFA, 0x00,
            0x20, 0x03,
            0x2A, 0x00,
        });
        Expect(failures, multiplexedAdditionalStroke.MultiplexedId, 0x36, "Multiplexed Additional Stroke Data ID");
        Expect(failures, multiplexedAdditionalStroke.DisplayWatts, 250, "Multiplexed Additional Stroke Data watts");
        Expect(failures, multiplexedAdditionalStroke.StrokeCount, 42, "Multiplexed Additional Stroke Data stroke count");

        if (failures.Count == 0)
        {
            Console.WriteLine("SELF_TEST|PASS");
            return 0;
        }

        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"SELF_TEST|FAIL|{failure}");
        }

        return 1;
    }

    private static void Expect(List<string> failures, double? actual, double expected, string name)
    {
        if (actual == null || Math.Abs(actual.Value - expected) > 0.0001)
        {
            failures.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: expected {expected}, got {actual?.ToString(CultureInfo.InvariantCulture) ?? "null"}"));
        }
    }

    private static void Expect(List<string> failures, int? actual, int expected, string name)
    {
        if (actual != expected)
        {
            failures.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: expected {expected}, got {actual?.ToString(CultureInfo.InvariantCulture) ?? "null"}"));
        }
    }

    private static void Expect(List<string> failures, string? actual, string? expected, string name)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            failures.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: expected {expected ?? "null"}, got {actual ?? "null"}"));
        }
    }
}
