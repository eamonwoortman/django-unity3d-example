using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

public class LoginMenu : BaseMenu {
    public delegate void LoggedIn(string authToken);
    public LoggedIn OnLoggedIn;
    public string Status = "";
    private string username = "", password = "";
    private bool loggingIn = false;
    private float nextStatusChange;
    private int dotNumber = 1;
    private bool rememberMe = false;
    private bool hasFocussed = false;
    private const float LABEL_WIDTH = 100;

    private void Start() {
        windowRect = new Rect(10, 10, 300, 150);
        if (PlayerPrefs.HasKey("x1")) {
            username = FromBase64(PlayerPrefs.GetString("x2"));
            password = FromBase64(PlayerPrefs.GetString("x1"));
            rememberMe = true;
        }
    }

    private string FromBase64(string inputString) {
        byte[] bytes = Convert.FromBase64String(inputString);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    private string ToBase64(string inputString) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(inputString);
        return Convert.ToBase64String(bytes);
    }

    private void SaveCredentials() {
        PlayerPrefs.SetString("x2", ToBase64(username));
        PlayerPrefs.SetString("x1", ToBase64(password));
    }

    private void RemoveCredentials() {
        if (PlayerPrefs.HasKey("x1")) {
            PlayerPrefs.DeleteAll();
        }
    }

    private void OnBackendResponse(ResponseType responseType, JToken responseData, string callee) {
        loggingIn = false;
        if (responseType == ResponseType.Success) {
            string authToken = responseData.Value<string>("token");
            if (rememberMe) {
                SaveCredentials();
            } else {
                RemoveCredentials();
            }

            if (OnLoggedIn != null) {
                OnLoggedIn(authToken);
            }
            Status = "Logged in!";
        } else if (responseType == ResponseType.RequestError) {
            Status = "Could not reach the server. Please try again later.";
        } else {
            JToken fieldToken = responseData["non_field_errors"];
            if (fieldToken == null || !fieldToken.HasValues) {
                Status = "Login failed: unknown error";
            } else {
                string errors = "";
                JToken[] fieldValidationErrors = fieldToken.Values().ToArray();
                foreach (JToken validationError in fieldValidationErrors) {
                    errors += validationError.Value<string>();
                }
                Status = "Login failed: " + errors;
            }
        }
    }

    private void DoLogin() {
        if (loggingIn) {
            Debug.LogWarning("Already logging in, returning.");
            return;
        }
        loggingIn = true;
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("username", username);
        fields.Add("password", password);
        backendManager.PerformRequest("getauthtoken", fields, OnBackendResponse);
    }

    private void ShowWindow(int id) {
        GUILayout.BeginVertical();
        GUILayout.Label("Please enter your username and password");
        bool filledIn = (username != "" && password != "");

        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("usernameField");
        GUILayout.Label("Username", GUILayout.Width(LABEL_WIDTH));
        username = GUILayout.TextField(username, 30);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Password", GUILayout.Width(LABEL_WIDTH));
        password = GUILayout.PasswordField(password, '*', 30);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Remember me?", GUILayout.Width(LABEL_WIDTH));
        rememberMe = GUILayout.Toggle(rememberMe, "");
        GUILayout.EndHorizontal();
        
        GUILayout.Label(Status);
        GUI.enabled = filledIn;
        Event e = Event.current;
        if (filledIn && e.isKey && e.keyCode == KeyCode.Return) {
            DoLogin();
        }

        if (GUILayout.Button("Login")) {
            DoLogin();
        }
        GUI.enabled = true;
        
        GUILayout.EndVertical();

        if (!hasFocussed) {
            GUI.FocusControl("usernameField");
            hasFocussed = true;
        }
    }

    private void Update() {
        if(!loggingIn) {
            return;
        }

        if (Time.time > nextStatusChange) {
            nextStatusChange = Time.time + 0.5f;
            Status = "Logging in";
            for (int i = 0; i < dotNumber; i++) {
                Status += ".";
            }
            if (++dotNumber > 3) {
                dotNumber = 1;
            }
        }

    }

    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(1, windowRect, ShowWindow, "Login menu");
    }

}
