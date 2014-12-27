using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

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
        Test1();
        Test2();
        Test3();
    }

    void Assert(bool statement, int testnumber, string message) {
        if (!statement) {
            throw new AssertionFailedException("[TEST " + testnumber + " FAILED] " + message);
        }
    }
    void ValidateField(int testNumber, JObject jsonObject, string key, object value) {
        JToken fieldToken = jsonObject[key];
        Assert(fieldToken != null, testNumber, key + " field can't be found");
        Assert(fieldToken.Value<object>() == value, testNumber, "field value does not equal " + value);
    }
    /// <summary>
    /// Test 1
    /// this should pass if the response was a 403 and we're able to get the validation errors
    /// </summary>
    void Test1() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        backendManager.PerformRequest("addscore", fields, OnTest1Response);
    }

    void OnTest1Response(ResponseType responseType, JObject responseData, string callee) {
        const string emptyFieldMsg = "This field may not be blank.";

        Assert(responseType == ResponseType.ErrorFromServer, 1, "reponseType != ErrorFromServer");
        JToken nameField = responseData["name"];
        Assert(nameField != null, 1, "could not retrieve the namefield");
        Assert(nameField.HasValues, 1, "namefield does not have any values");

        JToken[] fieldValidationErrors = nameField.Values().ToArray();
        string firstError = fieldValidationErrors[0].Value<string>();
        Assert(firstError.Equals(emptyFieldMsg), 1, "error string of namefield does not equal the emptyFieldMsg");
        
        Debug.Log("Test 1 passed");
    }

    /// <summary>
    /// Test 2
    /// this should pass if the response was a 201 and an object has been created
    /// </summary>
    void Test2() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        fields.Add("name", "dada");
        backendManager.PerformRequest("addscore", fields, OnTest2Response);
    }
    void OnTest2Response(ResponseType responseType, JObject responseData, string callee) {
        Assert(responseType == ResponseType.Success, 2, "responseType != success, it's: " + responseType);

        JToken idToken = responseData["id"];
        Assert(idToken != null, 2, "id field can't be found");
        Assert(idToken.Value<int>() > -1, 2, "id value does not equal 1337");

        JToken scoreToken = responseData["score"];
        Assert(scoreToken != null, 2, "score field can't be found");
        Assert(scoreToken.Value<int>() == 1337, 2, "score value does not equal 1337");

        JToken nameToken = responseData["name"];
        Assert(nameToken != null, 2, "name field can't be found");
        Assert(nameToken.Value<string>() == "dada", 2, "name value does not equal 'dada'");
        
        Debug.Log("Test 2 passed");
    }


    /// <summary>
    /// Test 3
    /// this should pass if the response was an error telling us the score is invalid
    /// </summary>
    void Test3() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", "yoloswaggings");
        fields.Add("name", "dada");
        backendManager.PerformRequest("addscore", fields, OnTest3Response);
    }
    void OnTest3Response(ResponseType responseType, JObject responseData, string callee) {
        Debug.Log("[" + responseType + "] " + responseData);
        Assert(responseType == ResponseType.Success, 2, "responseType != success, it's: " + responseType);

        Debug.Log("Test 3 passed");
    }

}
