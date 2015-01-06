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

}