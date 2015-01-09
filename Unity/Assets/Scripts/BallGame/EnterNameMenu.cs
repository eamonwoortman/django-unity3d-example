using UnityEngine;
using System.Collections;
using System;

public class EnterNameMenu : BaseMenu {

    public Action OnCancel;
    public Action<string> OnNameEntered;

    private string name = "";

    public EnterNameMenu() {
        windowRect = new Rect(Screen.width  / 2 - 100, Screen.height / 2 - 100, 200, 200);
    }
    
    private void ShowWindow(int id) {
        GUILayout.Label("Enter name");

        name = GUILayout.TextField(name);

        GUI.enabled = (name != "");
        if (GUILayout.Button("Next")) {
            if (OnNameEntered != null) {
                OnNameEntered(name);
            }
        }

        GUI.enabled = true;
        if (GUILayout.Button("Cancel")) {
            if (OnCancel != null) {
                OnCancel();
            }
        }
        GUI.enabled = true;
    }
    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(0, windowRect, ShowWindow, "Highscore");
    }
	
}
