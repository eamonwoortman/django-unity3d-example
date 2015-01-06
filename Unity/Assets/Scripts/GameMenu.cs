using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {
    public GUISkin Skin;
    private Rect windowRect = new Rect(10, 10, 400, 400);

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
