using System.Globalization;
using System.Text;
using HSEBank.scr.Domain.Entities;
using HSEBank.scr.ImportExport;

namespace HSEBank.scr.Infrastructure.Serializers;

public class CsvSerializer : IDataSerializer
{
    public string Format => "CSV";

    private static string CsvEscape(object? v)
    {
        var s = Convert.ToString(v, CultureInfo.InvariantCulture) ?? "";
        if (s.Contains(';') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
            s = "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    public string Serialize<T>(IEnumerable<T> data)
    {
        var props = typeof(T).GetProperties();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", props.Select(p => p.Name)));
        foreach (var item in data)
            sb.AppendLine(string.Join(";", props.Select(p => CsvEscape(p.GetValue(item)))));
        return sb.ToString();
    }

    public IEnumerable<T> Deserialize<T>(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
            return new List<T>();

        var headers = lines[0].Trim().Split(';');
        var props = typeof(T).GetProperties();
        var result = new List<T>();

        foreach (var line in lines.Skip(1))
        {
            var values = SplitCsvLine(line.Trim());
            var obj = Activator.CreateInstance<T>();

            for (int i = 0; i < headers.Length && i < values.Count; i++)
            {
                var prop = props.FirstOrDefault(p =>
                    string.Equals(p.Name, headers[i], StringComparison.OrdinalIgnoreCase));
                if (prop == null) continue;

                var target = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                var s = values[i].Trim();
                if (s.Length > 1 && s.StartsWith("\"") && s.EndsWith("\""))
                    s = s.Substring(1, s.Length - 2).Replace("\"\"", "\"");

                try
                {
                    object? val;
                    if (target == typeof(Guid))
                        val = Guid.TryParse(s, out var g) ? g : Guid.Empty;
                    else if (target == typeof(DateTime))
                        val = DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    else if (target.IsEnum)
                        val = Enum.Parse(target, s, ignoreCase: true);
                    else
                        val = Convert.ChangeType(s, target, CultureInfo.InvariantCulture);

                    prop.SetValue(obj, val);
                }
                catch
                {
                    // ignored
                }
            }

            result.Add(obj);
        }

        return result;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var res = new List<string>();
        var sb = new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (ch == ';' && !quoted) { res.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(ch);
        }
        res.Add(sb.ToString());
        return res;
    }

    public void ExportSeparateFiles(
        string basePath,
        IEnumerable<BankAccount> accounts,
        IEnumerable<Operation> operations,
        IEnumerable<Category> categories)
    {
        File.WriteAllText(Path.Combine(basePath, "accounts.csv"), Serialize(accounts));
        File.WriteAllText(Path.Combine(basePath, "operations.csv"), Serialize(operations));
        File.WriteAllText(Path.Combine(basePath, "categories.csv"), Serialize(categories));

        Console.WriteLine($"Экспортировано 3 таблицы CSV в папку {basePath}");
    }
}