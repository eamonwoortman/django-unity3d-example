using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Reflection;

class AssertionFailedException : Exception {
    public AssertionFailedException(string message) : base(message) { }
}

public class BackendTester : MonoBehaviour {
    private BackendManager backendManager;

    void Start() {
        backendManager = GetComponent<BackendManager>();
        if (backendManager == null) {
            backendManager = gameObject.AddComponent<BackendManager>();
        }
        StartTests();
    }

    void StartTests() {
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (MethodInfo methodInfo in methods) {
            if (methodInfo.Name.StartsWith("Test_")) {
                Invoke(methodInfo.Name, 0);
            }
        }
    }

    void Assert(bool statement, string message) {
        if (!statement) {
            throw new AssertionFailedException(message);
        }
    }
    void ValidateField(JObject jsonObject, string key, object value) {
        JToken fieldToken = jsonObject[key];
        Assert(fieldToken != null, key + " field can't be found");
        Assert(fieldToken.Value<object>() == value, "field value does not equal " + value);
    }

    void OnBackendResponse(ResponseType responseType, JObject responseData, string callee) {
        string[] splittedStr = callee.Split('_');
        if (splittedStr.Length != 2) {
            Debug.LogWarning("Could not split callee string into multiple strings");
            return;
        }
        int testNumber;
        if (!int.TryParse(splittedStr[1], out testNumber)) {
            Debug.LogWarning("Could not parse splittedStr[1] to an int");
            return;
        }
        MethodInfo methodInfo = GetType().GetMethod("Validate_" + testNumber, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (methodInfo == null) {
            Debug.LogWarning("No validation method for test " + testNumber + " could be found");
            return;
        }

        try {
            methodInfo.Invoke(this, new object[] { responseType, responseData });
            Debug.Log("[TEST " + testNumber + "] PASSED");
        } catch (Exception afe) {
            Debug.LogWarning("[TEST " + testNumber + "] FAILED, " + afe.InnerException);
        }
    }


    /// <summary>
    /// Test 1
    /// this should pass if the response was a 403 and we're able to get the validation errors
    /// </summary>
    void Test_1() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        backendManager.PerformRequest("addscore", fields, OnBackendResponse);
    }

    void Validate_1(ResponseType responseType, JObject responseData) {
        const string emptyFieldMsg = "This field may not be blank.";

        Assert(responseType == ResponseType.ErrorFromServer, "reponseType != ErrorFromServer");
        JToken nameField = responseData["name"];
        Assert(nameField != null, "could not retrieve the namefield");
        Assert(nameField.HasValues, "namefield does not have any values");

        JToken[] fieldValidationErrors = nameField.Values().ToArray();
        string firstError = fieldValidationErrors[0].Value<string>();
        Assert(firstError.Equals(emptyFieldMsg), "error string of namefield does not equal the emptyFieldMsg");
    }

    /// <summary>
    /// Test 2
    /// this should pass if the response was a 201 and an object has been created
    /// </summary>
    void Test_2() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        fields.Add("name", "dada");
        backendManager.PerformRequest("addscore", fields, OnBackendResponse);
    }
    void Validate_2(ResponseType responseType, JObject responseData) {
        Assert(responseType == ResponseType.Success, "responseType != success, it's: " + responseType);

        JToken idToken = responseData["id"];
        Assert(idToken != null, "id field can't be found");
        Assert(idToken.Value<int>() > -1, "id value does not equal 1337");

        JToken scoreToken = responseData["score"];
        Assert(scoreToken != null, "score field can't be found");
        Assert(scoreToken.Value<int>() == 1337, "score value does not equal 1337");

        JToken nameToken = responseData["name"];
        Assert(nameToken != null, "name field can't be found");
        Assert(nameToken.Value<string>() == "dada", "name value does not equal 'dada'");
    }

    /// <summary>
    /// Test 3
    /// this should pass if the response was an error telling us the score is invalid
    /// </summary>
    void Test_3() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", "yoloswaggings");
        fields.Add("name", "dada");
        backendManager.PerformRequest("addscore", fields, OnBackendResponse);
    }
    void Validate_3(ResponseType responseType, JObject responseData) {
//        Debug.Log("[" + responseType + "] " + responseData);
        Assert(responseType == ResponseType.Success, "responseType != success, it's: " + responseType);
    }
}
