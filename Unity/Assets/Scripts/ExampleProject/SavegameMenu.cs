using UnityEngine;
using System.Collections;

public class SavegameMenu : BaseMenu {
    public delegate void LoadSaveButtonPressed(string filename);
    public LoadSaveButtonPressed OnSaveButtonPressed;
    public LoadSaveButtonPressed OnLoadButtonPressed;

    private const string NoSavegamesFound = "No savegames found";
    private string[] savegameNames = new string[] { NoSavegamesFound };
    private int selectedNameIndex = -1;
    private string saveName = "";

    public SavegameMenu() {
        windowRect = new Rect(10, 10, 200, 200);
    }
    
    private void ShowWindow(int id) {
        GUILayout.BeginVertical();
        GUILayout.Label("Save games");
        bool savegamesFound = (savegameNames[0] != NoSavegamesFound);
        GUI.enabled = savegamesFound;
        selectedNameIndex = GUILayout.SelectionGrid(selectedNameIndex, savegameNames, 1);
        GUI.enabled = true;
        GUILayout.Space(100);

        saveName = GUILayout.TextField(saveName);
        GUILayout.BeginHorizontal();

        GUI.enabled = (saveName != "");
        if (GUILayout.Button("Save")) {
            if (OnSaveButtonPressed != null) {
                OnSaveButtonPressed(saveName);
            }
        }

        GUI.enabled = savegamesFound;
        if (GUILayout.Button("Load")) {
            if (OnLoadButtonPressed != null) {
                OnLoadButtonPressed(savegameNames[selectedNameIndex]);
            }
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(0, windowRect, ShowWindow, "Load/save menu");
    }

}
