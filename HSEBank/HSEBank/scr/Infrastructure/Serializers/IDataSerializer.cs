namespace HSEBank.scr.Infrastructure.Serializers;

public interface IDataSerializer
{
    string Format { get; }

    string Serialize<T>(IEnumerable<T> data);
    IEnumerable<T> Deserialize<T>(string text);
}