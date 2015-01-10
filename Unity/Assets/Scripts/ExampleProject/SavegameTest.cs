using UnityEngine;
using System.Collections;

public class SavegameTest : MonoBehaviour {
    private LoginMenu loginMenu;
    private SavegameMenu savegameMenu;
    private string authToken;

	// Use this for initialization
	void Start () {
        loginMenu = GetComponent<LoginMenu>();
        loginMenu.OnLoggedIn += LoggedIn;

        savegameMenu = GetComponent<SavegameMenu>();
        savegameMenu.enabled = false;
	}

    private void LoggedIn(string authToken) {
        this.authToken = authToken;
        Invoke("DisableLoginMenu", 1);
    }

    private void DisableLoginMenu() {
        loginMenu.enabled = false;
        savegameMenu.enabled = true;
    }
}
