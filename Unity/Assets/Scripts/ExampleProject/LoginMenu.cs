using UnityEngine;
using System.Collections;

public class LoginMenu : BaseMenu {
    public delegate void LoginButtonPressed(string username, string password);
    public LoginButtonPressed OnLoginButtonPressed;
    public string Status = "";
    private string username = "", password = "";

    public LoginMenu() {
        windowRect = new Rect(10, 10, 300, 150);
    }
    
    private void ShowWindow(int id) {
        GUILayout.BeginVertical();
        GUILayout.Label("Please enter your username and password");
        bool filledIn = (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password));

        GUILayout.BeginHorizontal();
        GUILayout.Label("username", GUILayout.Width(80));
        username = GUILayout.TextField(username);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("password", GUILayout.Width(80));
        password = GUILayout.TextField(password);
        GUILayout.EndHorizontal();

        GUILayout.Label(Status);
        GUI.enabled = filledIn;
        if (GUILayout.Button("Login")) {
            if (OnLoginButtonPressed != null) {
                OnLoginButtonPressed(username, password);
            }
        }
        GUI.enabled = true;
        
        GUILayout.EndVertical();
    }
    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(1, windowRect, ShowWindow, "Login menu");
    }
}
