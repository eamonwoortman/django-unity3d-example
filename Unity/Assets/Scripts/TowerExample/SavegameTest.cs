using UnityEngine;
using System.Collections;

public class SavegameTest : MonoBehaviour {
    private LoginMenu loginMenu;
    private SavegameMenu savegameMenu;
    private string authToken;

	void Start () {
        loginMenu = GetComponent<LoginMenu>();
        loginMenu.HasLoggedIn += LoggedIn;

        savegameMenu = GetComponent<SavegameMenu>();
        savegameMenu.enabled = false;

        
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
