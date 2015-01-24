// Copyright (c) 2015 Eamon Woortman
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

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

    public static T[] SubArray<T>(this T[] data, int index, int length) {
        T[] result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
    
    public static U GetOrCreateComponent<U>(this GameObject gameObject) where U : Component {
        U comp = Component.FindObjectOfType<U>();
        if (comp == null) {
            comp = gameObject.AddComponent<U>();
        }
        return comp;
    }
}
