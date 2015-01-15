using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SavegameMenu : BaseMenu {
    public delegate void LoadSaveButtonPressed(string filename);
    public LoadSaveButtonPressed OnSaveButtonPressed;
    public LoadSaveButtonPressed OnLoadButtonPressed;
    public delegate void VoidDelegate();
    public VoidDelegate OnHasSaved;

    public string SaveName {
        get {
            return saveName;
        }
    }

    private const string NoSavegamesFound = "No savegames found";
    private List<Savegame> saveGames;
    private int selectedNameIndex = -1;
    private string saveName = "";
    private string[] saveGameNames = { NoSavegamesFound };
    
    public SavegameMenu() {
        windowRect = new Rect(10, 10, 200, 200);
    }

    private void Start() {
        backendManager.OnSaveGameSucces += OnSaveGameSuccess;
        backendManager.OnSaveGameFailed += OnSaveGameFailed;
        backendManager.OnGamesLoaded += OnGamesLoaded;
    }

    private void OnSaveGameSuccess() {
        if (OnHasSaved != null) {
            OnHasSaved();
        }
    }

    private void OnSaveGameFailed(string error) {
    }

    private void OnGamesLoaded(List<Savegame> games) {
        saveGames = games;

        if(games.Count != 0)
            saveGameNames = saveGames.Select(game => game.Name).ToArray().SubArray(0, Mathf.Min(3, games.Count));
    }

    private void ShowWindow(int id) {
        GUILayout.BeginVertical();
        GUILayout.Label("Save games");
        bool savegamesFound = (saveGameNames[0] != NoSavegamesFound);
        GUI.enabled = savegamesFound;
        selectedNameIndex = GUILayout.SelectionGrid(selectedNameIndex, saveGameNames, 1);
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

        GUI.enabled = savegamesFound && selectedNameIndex != -1;
        if (GUILayout.Button("Load")) {
            if (OnLoadButtonPressed != null) {
                OnLoadButtonPressed(saveGames[selectedNameIndex].File);
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
