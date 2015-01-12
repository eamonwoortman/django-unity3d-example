using System;
using Newtonsoft.Json.Linq;

public static class ExtensionMethods {
    public static JToken GetValue(this JObject jobject, string propertyName) {
        JToken obj;
        if (jobject.TryGetValue(propertyName, out obj)) {
            return obj;
        }
        return null;
    }


    public static string FromBase64(this string inputString) {
        byte[] bytes = Convert.FromBase64String(inputString);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static string ToBase64(this string inputString) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(inputString);
        return Convert.ToBase64String(bytes);
    }


}