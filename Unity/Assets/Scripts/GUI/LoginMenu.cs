using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

public class LoginMenu : BaseMenu {
    public delegate void LoggedIn();
    public LoggedIn HasLoggedIn;
    private string status = "";
    private string username = "", password = "";
    private bool loggingIn = false;
    private float nextStatusChange;
    private int dotNumber = 1;
    private bool rememberMe = false;
    private bool hasFocussed = false;
    private const float LABEL_WIDTH = 100;

    private void Start() {
        windowRect = new Rect(10, 10, 300, 150);
        backendManager.OnLoggedIn += OnLoggedIn;
        backendManager.OnLoginFailed += OnLoginFailed;
        
        if (PlayerPrefs.HasKey("x1")) {
            username = PlayerPrefs.GetString("x2").FromBase64();
            password = PlayerPrefs.GetString("x1").FromBase64();
            rememberMe = true;
        }
    }

    private void SaveCredentials() {
        PlayerPrefs.SetString("x2", username.ToBase64());
        PlayerPrefs.SetString("x1", password.ToBase64());
    }

    private void RemoveCredentials() {
        if (PlayerPrefs.HasKey("x1")) {
            PlayerPrefs.DeleteAll();
        }
    }

    private void OnLoginFailed(string error) {
        status = "Login error: " + error;
        loggingIn = false;
    }

    private void OnLoggedIn() {
        status = "Logged in!";
        loggingIn = false;

        if (rememberMe) {
            SaveCredentials();
        } else {
            RemoveCredentials();
        }

        if (HasLoggedIn != null) {
            HasLoggedIn();
        }
    }


    private void DoLogin() {
        if (loggingIn) {
            Debug.LogWarning("Already logging in, returning.");
            return;
        }
        loggingIn = true;
        backendManager.Login(username, password);
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
        
        GUILayout.Label(status);
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
            status = "Logging in";
            for (int i = 0; i < dotNumber; i++) {
                status += ".";
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