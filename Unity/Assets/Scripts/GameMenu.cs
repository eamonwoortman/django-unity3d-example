using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {
    public static Rect WindowRect = new Rect(10, 10, 400, 400);
    void Awake() {

    }

    void OnGUI() {
        GUILayout.BeginArea(WindowRect);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Push me")) {
        }
        GUILayout.Label("THIS IS A LABEL");
        GUILayout.Toggle(true, "TOGGLE");
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
