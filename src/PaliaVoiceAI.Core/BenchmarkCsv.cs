using System.Globalization;
using System.Text;

namespace PaliaVoiceAI.Core;

public static class BenchmarkCsv
{
    public const string Header = "Timestamp,Engine,Backend,Threads,TextLength,GenerationMs,AudioDurationMs,RTF";

    public static IReadOnlyList<BenchmarkRecord> Read(string path)
    {
        if (!File.Exists(path))
            return [];

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length == 0 || lines.All(string.IsNullOrWhiteSpace))
            return [];

        if (!string.Equals(lines[0].Trim(), Header, StringComparison.Ordinal))
            throw new FormatException("The benchmark file header is not the expected columns.");

        var records = new List<BenchmarkRecord>();
        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var fields = Split(lines[i]);
            if (fields.Count != 8)
                throw new FormatException($"Benchmark row {i + 1} has {fields.Count} columns instead of 8.");

            records.Add(new BenchmarkRecord(
                DateTimeOffset.Parse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                fields[1],
                fields[2],
                int.Parse(fields[3], CultureInfo.InvariantCulture),
                int.Parse(fields[4], CultureInfo.InvariantCulture),
                double.Parse(fields[5], CultureInfo.InvariantCulture),
                double.Parse(fields[6], CultureInfo.InvariantCulture),
                double.Parse(fields[7], CultureInfo.InvariantCulture)));
        }

        return records;
    }

    public static void Write(string path, IEnumerable<BenchmarkRecord> records)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var lines = new List<string> { Header };
        foreach (var record in records)
        {
            lines.Add(string.Join(",",
                record.Timestamp.ToString("O", CultureInfo.InvariantCulture),
                Quote(record.Engine),
                Quote(record.Backend),
                record.Threads.ToString(CultureInfo.InvariantCulture),
                record.TextLength.ToString(CultureInfo.InvariantCulture),
                record.GenerationMs.ToString("F2", CultureInfo.InvariantCulture),
                record.AudioDurationMs.ToString("F2", CultureInfo.InvariantCulture),
                record.RealTimeFactor.ToString("F4", CultureInfo.InvariantCulture)));
        }

        File.WriteAllLines(path, lines, Encoding.UTF8);
    }

    public static string Quarantine(string path)
    {
        var backup = path + ".bak";
        if (File.Exists(backup))
            File.Delete(backup);
        File.Move(path, backup);
        return backup;
    }

    public static IReadOnlyList<string> Split(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (inQuotes)
            {
                if (ch != '"')
                {
                    current.Append(ch);
                    continue;
                }

                if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = false;
                continue;
            }

            if (ch == '"')
            {
                inQuotes = true;
                continue;
            }

            if (ch == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        fields.Add(current.ToString());
        return fields;
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
