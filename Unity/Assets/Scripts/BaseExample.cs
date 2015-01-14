using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public abstract class BaseGame<T> : MonoBehaviour {

    [SerializeField]
    private SavegameMenu saveMenu;

    [SerializeField]
    private LoginMenu loginMenu;

    [SerializeField]
    protected BackendManager backendManager;

    public T Data;

    protected bool IsLoggedIn { get; private set; }

	public abstract void Deserialize(T gameData);
    protected abstract T Serialize();

    protected virtual void Start() {
        IsLoggedIn = false;

        loginMenu.enabled = true;
        saveMenu.enabled = false;

        backendManager.OnLoggedIn += delegate {
            backendManager.LoadGames();
            loginMenu.enabled = false;
            saveMenu.enabled = true;
            IsLoggedIn = true;
        };

        saveMenu.OnSaveButtonPressed += delegate (string filename) {
            backendManager.SaveGame(filename, JsonConvert.SerializeObject(Serialize()), typeof(T));

        };

        saveMenu.OnLoadButtonPressed += delegate(string filename) {
            StartCoroutine(DownloadSaveFile(filename));
        };
    }

    private IEnumerator DownloadSaveFile(string file) {
        WWW www = new WWW(file);
        yield return www;

        T data = JsonConvert.DeserializeObject<T>(www.text);
        Deserialize(data);
    }

    private void Save(string filename) {
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
