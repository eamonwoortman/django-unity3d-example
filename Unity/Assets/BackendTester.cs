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
                methodInfo.Invoke(this, null);
            }
        }
    }

    void Assert(bool statement, string message) {
        if (!statement) {
            throw new AssertionFailedException(message);
        }
    }

    void ValidateField<T>(JObject jsonObject, string key, T value, bool shouldEqual = true) {
        JToken fieldToken = jsonObject[key];
        Assert(fieldToken != null, key + " field can't be found");
        bool valueEquals = fieldToken.Value<T>().Equals(value);
        Assert(valueEquals == shouldEqual, "'" + key + "' field value " + (shouldEqual ? "does not equal " : "should not be equal to ") + value);
    }

    void ValidateSubfield(JObject jsonObject, string key, string value) {
        JToken fieldToken = jsonObject[key];
        Assert(fieldToken != null, key + " field can't be found");
        Assert(fieldToken.HasValues, "score field does not have any values");

        JToken[] fieldValidationErrors = fieldToken.Values().ToArray();
        bool found = false;
        foreach (JToken token in fieldValidationErrors) {
            string tokenValue = token.Value<string>();
            if (tokenValue.Equals(value)) {
                found = true;
                break;
            }
        }
        Assert(found, "error strings of namefield does not contain the value '" + value + "'");
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
        } catch (Exception ex) {
            Debug.LogWarning("[TEST " + testNumber + "] FAILED");
            Debug.LogWarning("[TEST " + testNumber + "] EXCEPTION: " + ex.InnerException);
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
        ValidateSubfield(responseData, "name", emptyFieldMsg);
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
        ValidateField<int>(responseData, "id", -1, false);
        ValidateField<int>(responseData, "score", 1337);
        ValidateField<string>(responseData, "name", "dada");
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
        const string invalidIntegerMsg = "A valid integer is required.";
        Assert(responseType == ResponseType.ErrorFromServer, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "score", invalidIntegerMsg);
    }
}
