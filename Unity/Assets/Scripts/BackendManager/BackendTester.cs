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
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections;

class AssertionFailedException : Exception {
    public AssertionFailedException(string message) : base(message) { }
}

public class BackendTester : MonoBehaviour {
    public string AdminUsername = "admin";
    public string AdminPassword = "admin";
    public string AlternateBackendUrl = "";

    private BackendManager backendManager;
    private System.Diagnostics.Stopwatch stopwatch;
    private float totaltime;
    
    private void Start() {
        backendManager = GetComponent<BackendManager>();
        if (backendManager == null) {
            backendManager = gameObject.AddComponent<BackendManager>();
        }
        if (AlternateBackendUrl != "") {
            backendManager.DevelopmentUrl = backendManager.ProductionUrl = AlternateBackendUrl;
        }
        stopwatch = new System.Diagnostics.Stopwatch();
        StartCoroutine(StartTests());
    }

    private bool StartTest(int testNumber) {
        MethodInfo method = GetType().GetMethod("Test_"+testNumber, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) {
            return false;
        }

        Debug.Log("Starting test " + testNumber + "...");
        stopwatch.Start();
        method.Invoke(this, null);
        return true;
    }

    private IEnumerator StartTests() {
        WWW www = new WWW(backendManager.BackendUrl);
        while (!www.isDone) {
            yield return false;
        }
        if (www.error != null) {
            Debug.LogError("Could not reach backend server, are you sure it's online?");
            yield break;
        }
        StartTest(1);
    }

    private void Assert(bool statement, string message) {
        if (!statement) {
            throw new AssertionFailedException(message);
        }
    }

    private void ValidateField<T>(JToken jsonObject, string key, T value, bool shouldEqual = true) {
        JToken fieldToken = jsonObject[key];
        Assert(fieldToken != null, key + " field can't be found");
        bool valueEquals = fieldToken.Value<T>().Equals(value);
        Assert(valueEquals == shouldEqual, "'" + key + "' field value " + (shouldEqual ? "does not equal " : "should not be equal to ") + value);
    }

    private void ValidateSubfield(JToken jsonObject, string key, string value) {
        JToken fieldToken = jsonObject[key];
        Assert(fieldToken != null, key + " field can't be found");
        Assert(fieldToken.HasValues, key + " field does not have any values");

        JToken[] fieldValidationErrors = fieldToken.Values().ToArray();
        bool found = false;
        foreach (JToken token in fieldValidationErrors) {
            string tokenValue = token.Value<string>();
            if (tokenValue.Contains(value)) {
                found = true;
                break;
            }
        }
        Assert(found, "error strings of '"+key+"' field does not contain the value '" + value + "'");
    }

    private void OnBackendResponse(ResponseType responseType, JToken responseData, string callee) {
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
            stopwatch.Stop();
            totaltime += stopwatch.ElapsedMilliseconds;
            Debug.Log("[TEST " + testNumber + "] PASSED in " + stopwatch.ElapsedMilliseconds + " ms.");
        } catch (Exception ex) {
            stopwatch.Stop();
            totaltime += stopwatch.ElapsedMilliseconds;
            Debug.LogWarning("[TEST " + testNumber + "] FAILED");
            Debug.LogWarning("[TEST " + testNumber + "] EXCEPTION: " + ex.InnerException);
        }
        if (stopwatch.IsRunning) {
            stopwatch.Stop();
        }
        stopwatch.Reset();

        //now find and start a new test
        if (!StartTest(testNumber + 1)) {
            Debug.Log("All tests are done! All tests completed in "+totaltime+" ms.");
        }
    }

    /// <summary>
    /// Test 1
    /// this should pass if the response was an error telling us we need an email
    /// </summary>
    private void Test_1() {
        WWWForm form = new WWWForm();
        form.AddField("username", "testuser");
        form.AddField("password", "superpassword");
        backendManager.Send(RequestType.Post, "user", form, OnBackendResponse);
    }
    private void Validate_1(ResponseType responseType, JToken responseData) {
        const string invalidEmailMsg = "This field may not be blank";
        Assert(responseType == ResponseType.RequestError, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "email", invalidEmailMsg);
    }

    /// <summary>
    /// Test 2
    /// this should pass if the response was an error telling us the username should be unique
    /// </summary>
    private void Test_2() {
        WWWForm form = new WWWForm();
        form.AddField("username", AdminUsername);
        form.AddField("password", "superpassword");
        form.AddField("email", "test@test.com");
        backendManager.Send(RequestType.Post, "user", form, OnBackendResponse);
    }
    private void Validate_2(ResponseType responseType, JToken responseData) {
        const string uniqueUsernameMsg = "This field must be unique.";
        Assert(responseType == ResponseType.RequestError, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "username", uniqueUsernameMsg);
    }

    /// <summary>
    /// Test 3
    /// this should pass if the response was successful and the response object contains an email field which equals to our email
    /// </summary>
    private string randomUsername, password, email;
    private void DeleteCreatedUser(int userId) {
        WWWForm form = new WWWForm();
        form.AddField("username", randomUsername);
        form.AddField("password", password);
        form.AddField("email", email);
        backendManager.Send(RequestType.Delete, "user/" + userId + "/", form);
    }
    private void Test_3() {
        WWWForm form = new WWWForm();
        randomUsername = "test" + System.Guid.NewGuid().ToString().Substring(0, 8);
        password = "superpassword";
        email = "test@test.com";
        form.AddField("username", randomUsername);
        form.AddField("password", password);
        form.AddField("email", email);
        backendManager.Send(RequestType.Post, "user", form, OnBackendResponse);
    }
    private void Validate_3(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != Success, it's: " + responseType);
        ValidateField<string>(responseData, "email", "test@test.com");
        int userId = responseData.Value<int>("id");
        DeleteCreatedUser(userId);
    }

    /// <summary>
    /// Test 4
    /// this should pass if the response was successful and the response object contains a token field which is not empty
    /// </summary>
    private string authToken;
     private void Test_4() {
        WWWForm form = new WWWForm();
        form.AddField("username", AdminUsername);
        form.AddField("password", AdminPassword);
        backendManager.Send(RequestType.Post, "getauthtoken", form, OnBackendResponse);
    }
    private void Validate_4(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != Success, it's: " + responseType);
        ValidateField<string>(responseData, "token", "", false);
        authToken = responseData.Value<string>("token");
    }

    /// <summary>
    /// Test 5
    /// this should pass if the response contains an error about having invalid credentials
    /// </summary>
    private void Test_5() {
        WWWForm form = new WWWForm();
        form.AddField("username", AdminUsername);
        form.AddField("password", "someotherpassword");
        backendManager.Send(RequestType.Post, "getauthtoken", form, OnBackendResponse);
    }
    private void Validate_5(ResponseType responseType, JToken responseData) {
        const string invalidCredentialsMsg = "Unable to log in with provided credentials.";
        Assert(responseType == ResponseType.RequestError, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "non_field_errors", invalidCredentialsMsg);
    }

    /// <summary>
    /// Test 6
    /// this should pass if the response was a 401, authentication failed
    /// </summary>
    private void Test_6() {
        WWWForm form = new WWWForm();
        form.AddField("score", 1337);
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse);
    }
    private void Validate_6(ResponseType responseType, JToken responseData) {
        const string noAuthCredentialsMsg = "Authentication credentials were not provided.";
        Assert(responseType == ResponseType.RequestError, "reponseType != ErrorFromServer");
        ValidateField<string>(responseData, "detail", noAuthCredentialsMsg);
    }

    /// <summary>
    /// Test 7
    /// this should pass if the response was a 403 and we're able to get the validation errors
    /// </summary>
    private void Test_7() {
        WWWForm form = new WWWForm();
        form.AddField("TEST", "TEST");
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse, authToken);
    }
    private void Validate_7(ResponseType responseType, JToken responseData) {
        const string emptyFieldMsg = "This field is required.";
        Assert(responseType == ResponseType.RequestError, "reponseType != ErrorFromServer");
        ValidateSubfield(responseData, "score", emptyFieldMsg);
    }

    /// <summary>
    /// Test 8
    /// this should pass if the response was a 201 and an object has been created
    /// </summary>
    private void Test_8() {
        WWWForm form = new WWWForm();
        form.AddField("score", 1337);
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse, authToken);
    }
    private void Validate_8(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != success, it's: " + responseType);
        ValidateField<int>(responseData, "id", -1, false);
        ValidateField<int>(responseData, "score", 1337);
    }

    /// <summary>
    /// Test 9
    /// this should pass if the response was an error telling us the score is invalid
    /// </summary>
    private void Test_9() {
        WWWForm form = new WWWForm();
        form.AddField("score", "yoloswaggings");
        backendManager.Send(RequestType.Post, "score", form, OnBackendResponse, authToken);
    }
    private void Validate_9(ResponseType responseType, JToken responseData) {
        const string invalidIntegerMsg = "A valid integer is required.";
        Assert(responseType == ResponseType.RequestError, "responseType != ErrorFromServer, it's: " + responseType);
        ValidateSubfield(responseData, "score", invalidIntegerMsg);
    }

    /// <summary>
    /// Test 10
    /// this should pass if the response contains an array of scores
    /// </summary>
    private void Test_10() {
        backendManager.Send(RequestType.Get, "score", null, OnBackendResponse, authToken);
    }
    private void Validate_10(ResponseType responseType, JToken responseData) {
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
    private void DeleteSavegame(int savegameId) {
        backendManager.Send(RequestType.Delete, "savegame/" + savegameId + "/", null, null, authToken);
    }
    private void Test_11() {
        WWWForm form = new WWWForm();
        form.AddField("name", "best save1");
        form.AddField("type", "TestDataType");
        form.AddBinaryData("file", System.Text.Encoding.UTF8.GetBytes(LONG_SAVE_STR));
        backendManager.Send(RequestType.Post, "savegame", form, OnBackendResponse, authToken);
    }
    private void Validate_11(ResponseType responseType, JToken responseData) {
        Assert(responseType == ResponseType.Success, "responseType != Success, it's: " + responseType);
        int savegameId = responseData.Value<int>("id");
        DeleteSavegame(savegameId);
    }
}
