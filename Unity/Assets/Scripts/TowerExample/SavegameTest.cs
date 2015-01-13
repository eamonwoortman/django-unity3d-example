using UnityEngine;
using System.Collections;

public class SavegameTest : MonoBehaviour {
    private LoginMenu loginMenu;
    private SavegameMenu savegameMenu;
    
	void Start () {
        loginMenu = GetComponent<LoginMenu>();
        loginMenu.HasLoggedIn += LoggedIn;

        savegameMenu = GetComponent<SavegameMenu>();
        savegameMenu.enabled = false;
        savegameMenu.OnLoadButtonPressed += OnLoadButtonPressed;
        savegameMenu.OnSaveButtonPressed += OnSaveButtonPressed;
	}

    private void OnLoadButtonPressed(string filename) {

    }
    private void OnSaveButtonPressed(string filename) {

    }
    private void LoggedIn() {
        savegameMenu.LoadSavegames();
        Invoke("DisableLoginMenu", 1);
    }

    private void DisableLoginMenu() {
        loginMenu.enabled = false;
        savegameMenu.enabled = true;
    }
}
