using System.Text;

namespace HSEBank.scr.Infrastructure.Serializers;

public class YamlSerializer : IDataSerializer
{
    public string Format => "YAML";

    public string Serialize<T>(IEnumerable<T> data)
    {
        var typeName = typeof(T).Name.ToLower() + "s";
        var sb = new StringBuilder();
        sb.AppendLine($"{typeName}:");

        foreach (var item in data)
        {
            sb.AppendLine("  -");
            foreach (var prop in typeof(T).GetProperties())
            {
                var value = prop.GetValue(item) ?? "null";
                sb.AppendLine($"    {prop.Name.ToLower()}: {value}");
            }
        }
        return sb.ToString();
    }

    public IEnumerable<T> Deserialize<T>(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var props = typeof(T).GetProperties();
        var result = new List<T>();
        T? current = default;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("-"))
            {
                if (current != null)
                    result.Add(current);
                current = Activator.CreateInstance<T>();
            }
            else if (trimmed.Contains(":") && current != null)
            {
                var parts = trimmed.Split(':', 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                var prop = props.FirstOrDefault(p =>
                    string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));

                if (prop != null)
                {
                    try
                    {
                        var converted = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(current, converted);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        if (current != null)
            result.Add(current);

        return result;
    }
}