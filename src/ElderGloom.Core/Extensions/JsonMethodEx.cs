using System.Text.Json;
using ElderGloom.Core.Json;

namespace ElderGloom.Core.Extensions;

public static class JsonMethodEx
{
    public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, JsonUtils.GetDefaultJsonSettings());

    public static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, JsonUtils.GetDefaultJsonSettings());
}
