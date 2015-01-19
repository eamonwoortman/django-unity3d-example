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
    public string AdminUsername = "admin";
    public string AdminPassword = "admin";
    public string AlternateBackendUrl = "";

    private BackendManager backendManager;

    void Start() {
        backendManager = GetComponent<BackendManager>();
        if (backendManager == null) {
            backendManager = gameObject.AddComponent<BackendManager>();
        }
        if (AlternateBackendUrl != "") {
            backendManager.DevelopmentUrl = backendManager.ProductionUrl = AlternateBackendUrl;
        }
        StartTests();
    }

    void StartTests() {
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        float waitTime = 0.1f;
        foreach (MethodInfo methodInfo in methods) {
            if (methodInfo.Name.StartsWith("Test_")) {
                Invoke(methodInfo.Name, waitTime);
                waitTime += 0.5f;
            }
        }
    }

    void Assert(bool statement, string message) {
        if (!statement) {
            throw new AssertionFailedException(message);
        }
    }

    void ValidateField<T>(JToken jsonObject, string key, T value, bool shouldEqual = true) {
        JToken fieldToken = jsonObject[key];
        Assert(fieldToken != null, key + " field can't be found");
        bool valueEquals = fieldToken.Value<T>().Equals(value);
        Assert(valueEquals == shouldEqual, "'" + key + "' field value " + (shouldEqual ? "does not equal " : "should not be equal to ") + value);
    }

    void ValidateSubfield(JToken jsonObject, string key, string value) {
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

    void OnBackendResponse(ResponseType responseType, JToken responseData, string callee) {
        string[] splittedStr = callee.Split('_');
        if (splittedStr.Length != 2) {
            Debug.LogWarning("Could not split callee string into multiple strings, callee=" + callee);
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
    /// this should pass if the response was an error telling us we need an email
    /// </summary>
    void Test_1() {
        WWWForm form = new WWWForm();
        form.AddField("username", "testuser");
        form.AddField("password", "superpassword");
        backendManager.Send(RequestType.Post, "registeruser", form, OnBackendResponse);
    }
    void Validate_1(ResponseType responseType, JToken responseData) {
        const string invalidEmailMsg = "This field may not be blank.";
        Assert(responseType == ResponseType.ErrorFromServer, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "email", invalidEmailMsg);
    }

    /// <summary>
    /// Test 2
    /// this should pass if the response was an error telling us the username should be unique
    /// </summary>
    void Test_2() {
        WWWForm form = new WWWForm();
        form.AddField("username", AdminUsername);
        form.AddField("password", "superpassword");
        form.AddField("email", "test@test.com");
        backendManager.Send(RequestType.Post, "registeruser", form, OnBackendResponse);
    }
    void Validate_2(ResponseType responseType, JToken responseData) {
        const string uniqueUsernameMsg = "This field must be unique.";
        Assert(responseType == ResponseType.ErrorFromServer, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "username", uniqueUsernameMsg);
    }

    /// <summary>
    /// Test 3
    /// this should pass if the response was successful and the response object contains an email field which equals to our email
    /// </summary>
    private string randomUsername, password, email;
    private void DeleteCreatedUser() {
        WWWForm form = new WWWForm();
        form.AddField("username", randomUsername);
        form.AddField("password", password);
        form.AddField("email", email);
        backendManager.Send(RequestType.Post, "deleteuser", form);
    }
    void Test_3() {
        WWWForm form = new WWWForm();
        randomUsername = "test" + System.Guid.NewGuid().ToString().Substring(0, 8);
        password = "superpassword";
        email = "test@test.com";
        form.AddField("username", randomUsername);
        form.AddField("password", password);
        form.AddField("email", email);
        backendManager.Send(RequestType.Post, "registeruser", form, OnBackendResponse);
    }
    void Validate_3(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != Success, it's: " + responseType);
        ValidateField<string>(responseData, "email", "test@test.com");
        DeleteCreatedUser();
    }

    /// <summary>
    /// Test 4
    /// this should pass if the response was successful and the response object contains a token field which is not empty
    /// </summary>
    private string authToken;
     void Test_4() {
        WWWForm form = new WWWForm();
        form.AddField("username", AdminUsername);
        form.AddField("password", AdminPassword);
        backendManager.Send(RequestType.Post, "getauthtoken", form, OnBackendResponse);
    }
    void Validate_4(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != Success, it's: " + responseType);
        ValidateField<string>(responseData, "token", "", false);
        authToken = responseData.Value<string>("token");
    }

    /// <summary>
    /// Test 5
    /// this should pass if the response contains an error about having invalid credentials
    /// </summary>
    void Test_5() {
        WWWForm form = new WWWForm();
        form.AddField("username", AdminUsername);
        form.AddField("password", "someotherpassword");
        backendManager.Send(RequestType.Post, "getauthtoken", form, OnBackendResponse);
    }
    void Validate_5(ResponseType responseType, JToken responseData) {
        const string invalidCredentialsMsg = "Unable to log in with provided credentials.";
        Assert(responseType == ResponseType.ErrorFromServer, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "non_field_errors", invalidCredentialsMsg);
    }

    /// <summary>
    /// Test 6
    /// this should pass if the response was a 401, authentication failed
    /// </summary>
    void Test_6() {
        WWWForm form = new WWWForm();
        form.AddField("score", 1337);
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse);
    }
    void Validate_6(ResponseType responseType, JToken responseData) {
        const string noAuthCredentialsMsg = "Authentication credentials were not provided.";
        Assert(responseType == ResponseType.ErrorFromServer, "reponseType != ErrorFromServer");
        ValidateField<string>(responseData, "detail", noAuthCredentialsMsg);
    }

    /// <summary>
    /// Test 7
    /// this should pass if the response was a 403 and we're able to get the validation errors
    /// </summary>
    void Test_7() {
        WWWForm form = new WWWForm();
        form.AddField("TEST", "TEST");
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse, authToken);
    }
    void Validate_7(ResponseType responseType, JToken responseData) {
        const string emptyFieldMsg = "This field is required.";
        Assert(responseType == ResponseType.ErrorFromServer, "reponseType != ErrorFromServer");
        ValidateSubfield(responseData, "score", emptyFieldMsg);
    }

    /// <summary>
    /// Test 8
    /// this should pass if the response was a 201 and an object has been created
    /// </summary>
    void Test_8() {
        WWWForm form = new WWWForm();
        form.AddField("score", 1337);
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse, authToken);
    }
    void Validate_8(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != success, it's: " + responseType);
        ValidateField<int>(responseData, "id", -1, false);
        ValidateField<int>(responseData, "score", 1337);
    }

    /// <summary>
    /// Test 9
    /// this should pass if the response was an error telling us the score is invalid
    /// </summary>
    void Test_9() {
        WWWForm form = new WWWForm();
        form.AddField("score", "yoloswaggings");
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse, authToken);
    }
    void Validate_9(ResponseType responseType, JToken responseData) {
        const string invalidIntegerMsg = "A valid integer is required.";
        Assert(responseType == ResponseType.ErrorFromServer, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "score", invalidIntegerMsg);
    }

    /// <summary>
    /// Test 10
    /// this should pass if the response contains an array of scores
    /// </summary>
    void Test_10() {
        backendManager.Send(RequestType.Get, "score", null, OnBackendResponse, authToken);
    }
    void Validate_10(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != Success, it's: " + responseType + ", data="+responseData);
        JArray jArray = (JArray)responseData;
        Assert(jArray != null, "responseData was not a valid JArray");
        Assert(jArray.Count > 0, "jsonArray does not have any elements");
        JToken token = jArray.GetItem(0);
        ValidateField<int>(token, "score", -1, false);
    }

    /// <summary>
    /// Test 11
    /// this should pass if we can post a new savegame and the reponse was succesful
    /// </summary>
    private const string LONG_SAVE_STR = "Lorem ipsum dolor sit amet, id dicant quidam delicatissimi eos, nostrud epicuri fabellas nec cu. Ei mucius aliquam corrumpit mea, perfecto molestiae democritum an vim. Saperet electram contentiones per at, ne homero luptatum eam, vel cu singulis molestiae instructior. Eam novum detracto senserit cu, ad brute nihil salutandi nam.Lorem ipsum dolor sit amet, id dicant quidam delicatissimi eos, nostrud epicuri fabellas nec cu. Ei mucius aliquam corrumpit mea, perfecto molestiae democritum an vim. Saperet electram contentiones per at, ne homero luptatum eam, vel cu singulis molestiae instructior. Eam novum detracto senserit cu, ad brute nihil salutandi nam.Lorem ipsum dolor sit amet, id dicant quidam delicatissimi eos, nostrud epicuri fabellas nec cu. Ei mucius aliquam corrumpit mea, perfecto molestiae democritum an vim. Saperet electram contentiones per at, ne homero luptatum eam, vel cu singulis molestiae instructior. Eam novum detracto senserit cu, ad brute nihil salutandi nam.";
    private string ToBase64(string inputText) {
        byte[] bytesToEncode = System.Text.Encoding.UTF8.GetBytes(inputText);
        return Convert.ToBase64String(bytesToEncode);
    }
    void Test_11() {
        WWWForm form = new WWWForm();
        form.AddField("name", "best save1");
        form.AddField("type", "TestDataType");
        form.AddBinaryData("file", System.Text.Encoding.UTF8.GetBytes(LONG_SAVE_STR));
        backendManager.Send(RequestType.Post, "savegame", form, OnBackendResponse, authToken);
    }
    void Validate_11(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != Success, it's: " + responseType);
        //Debug.Log("ReponseData=" + responseData);
    }
}
