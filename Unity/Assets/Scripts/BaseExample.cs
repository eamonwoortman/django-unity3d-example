using UnityEngine;
using System.Collections;

public abstract class BaseGame : MonoBehaviour {

    [SerializeField]
    private SavegameMenu saveMenu;

    [SerializeField]
    private LoginMenu loginMenu;

    [SerializeField]
    private BackendManager backendManager;

	public abstract void Load(string jsonString);
    protected abstract string Serialize();

    protected virtual void Start() {
        loginMenu.enabled = true;
        saveMenu.enabled = false;

        backendManager.OnLoggedIn += delegate {
            backendManager.LoadGames();
            loginMenu.enabled = false;
            saveMenu.enabled = true;
        };

        saveMenu.OnSaveButtonPressed += delegate (string filename) {
            Save(filename);
        };

        saveMenu.OnLoadButtonPressed += delegate(string filename) {
            StartCoroutine(DownloadSaveFile(filename));
        };

    }

    private IEnumerator DownloadSaveFile(string file) {
        WWW www = new WWW(file);
        yield return www;
        Load(www.text);
    }

    private void Save(string filename) {
        backendManager.SaveGame(filename, Serialize());
    }

    protected virtual bool IsMouseOverMenu() {
        return saveMenu.IsMouseOver() || loginMenu.IsMouseOver();
    }

    protected void ShowSaveMenu() {
        saveMenu.enabled = true;
    }

    protected void HideSaveMenu() {
        saveMenu.enabled = false;
    }
}
