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
    private const float LABEL_WIDTH = 110;
    private SignupMenu signupMenu;

    private void Start() {
        windowRect = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 75, 300, 150);
        backendManager.OnLoggedIn += OnLoggedIn;
        backendManager.OnLoginFailed += OnLoginFailed;
        
        signupMenu = gameObject.GetOrCreateComponent<SignupMenu>();
        signupMenu.enabled = false;
        signupMenu.OnCancel += OnSignupCancelOrSuccess;
        signupMenu.OnSignedUp += OnSignupCancelOrSuccess;

        if (PlayerPrefs.HasKey("x1")) {
            username = PlayerPrefs.GetString("x2").FromBase64();
            password = PlayerPrefs.GetString("x1").FromBase64();
            rememberMe = true;
        }
    }

    private void OnSignupCancelOrSuccess() {
        enabled = true;
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

        GUILayout.FlexibleSpace();
        GUILayout.Label("Status: " + status);
        GUI.enabled = filledIn;
        Event e = Event.current;
        if (filledIn && e.isKey && e.keyCode == KeyCode.Return) {
            DoLogin();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Login")) {
            DoLogin();
        }
        if (GUILayout.Button("Signup")) {
            enabled = false;
            signupMenu.enabled = true;
        }
        GUILayout.EndHorizontal();

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
        windowRect = GUILayout.Window(2, windowRect, ShowWindow, "Login menu");
    }
}