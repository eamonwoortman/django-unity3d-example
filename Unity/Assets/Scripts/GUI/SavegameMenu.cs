using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SavegameMenu : BaseMenu {
    public delegate void LoadSaveButtonPressed(string filename);
    public LoadSaveButtonPressed OnSaveButtonPressed;
    public LoadSaveButtonPressed OnLoadButtonPressed;

    public string SaveName {
        get {
            return saveName;
        }
    }

    private const string NoSavegamesFound = "No savegames found";
    private string[] savegameNames = new string[] { NoSavegamesFound };
    private int selectedNameIndex = -1;
    private string saveName = "";
    
    public SavegameMenu() {
        windowRect = new Rect(Screen.width - 210, Screen.height - 210, 200, 200);
    }

    private void Start() {
        backendManager.OnSaveGameSucces += OnSaveGameSuccess;
        backendManager.OnSaveGameFailed += OnSaveGameFailed;

        backendManager.OnGamesLoaded += OnGamesLoaded;
    }

    private void OnSaveGameSuccess() {
        
    }

    private void OnSaveGameFailed(string error) {
    }

    private void OnGamesLoaded(List<Savegame> games) {
        savegameNames = games.Select(game => game.Name).ToArray();
    }

    public void SetData(byte[] data) {

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

        GUI.enabled = (SaveName != "");
        if (GUILayout.Button("Save")) {
            if (OnSaveButtonPressed != null) {
                OnSaveButtonPressed(SaveName);
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

    public void RefreshSaveGames() {

    }
}
