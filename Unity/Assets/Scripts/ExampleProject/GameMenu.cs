using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {
    public GUISkin Skin;
    public LoadSaveButtonPressed OnSaveButtonPressed;
    public LoadSaveButtonPressed OnLoadButtonPressed;

    private Rect windowRect = new Rect(10, 10, 200, 200);
    private delegate void LoadSaveButtonPressed(string filename);
    
    public bool InRect(Vector3 mousePosition) {
        return windowRect.Contains(mousePosition);
    }

    private void ShowWindow(int id) {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Push me")) {
        }
        GUILayout.Label("THIS IS A LABEL");
        GUILayout.Toggle(true, "TOGGLE");
        GUILayout.EndHorizontal();
    }
    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(0, windowRect, ShowWindow, "superwindow");
    }
}
