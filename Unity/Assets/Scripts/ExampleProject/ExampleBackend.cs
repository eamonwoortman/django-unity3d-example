using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BackendManager {
    public delegate void LoginFailed(string errorMsg);
    public delegate void LoggedIn();
    public LoggedIn OnLoggedIn;
    public LoginFailed OnLoginFailed;

    private string authenticationToken = "";
    
    public void Login(string username, string password) {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("username", username);
        fields.Add("password", password);
        PerformRequest("getauthtoken", fields, OnLoginResponse);
    }

    private void OnLoginResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            authenticationToken = responseData.Value<string>("token");
            if (OnLoggedIn != null) {
                OnLoggedIn();
            }
        } else if (responseType == ResponseType.RequestError) {
            if (OnLoginFailed != null) {
                OnLoginFailed("Could not reach the server. Please try again later.");
            }
        } else {
            JToken fieldToken = responseData["non_field_errors"];
            if (fieldToken == null || !fieldToken.HasValues) {
                if (OnLoginFailed != null) {
                    OnLoginFailed("Login failed: unknown error.");
                }
            } else {
                string errors = "";
                JToken[] fieldValidationErrors = fieldToken.Values().ToArray();
                foreach (JToken validationError in fieldValidationErrors) {
                    errors += validationError.Value<string>();
                }
                if (OnLoginFailed != null) {
                    OnLoginFailed("Login failed: " + errors);
                }
            }
        }
    }
}
