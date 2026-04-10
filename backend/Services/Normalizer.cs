using System.Text;

namespace Pulse.Api.Services;

public static class Normalizer
{
    public static string Normalize(string name)
    {
        var sb = new StringBuilder();
        foreach (var c in name.ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
                sb.Append(c);
        }
        return sb.Length > 0 ? sb.ToString() : "unknown";
    }
}
