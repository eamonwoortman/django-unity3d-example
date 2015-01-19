// Copyright (c) 2015 Eamon Woortman
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public abstract class BaseGame<T> : MonoBehaviour {

    [SerializeField]
    private SavegameMenu saveMenu;

    [SerializeField]
    private LoginMenu loginMenu;

    [SerializeField]
    protected BackendManager backendManager;

    public T Data;

    protected bool IsLoggedIn { get; private set; }

    protected abstract void Deserialize(T gameData);
    protected abstract T Serialize();


    protected virtual void Awake() {
        Data = (T)Activator.CreateInstance(typeof(T));
        
        if (loginMenu == null) {
            loginMenu = GetOrCreateComponent<LoginMenu>();
        }
        if (saveMenu == null) {
            saveMenu = GetOrCreateComponent<SavegameMenu>();
        }
        if (backendManager == null) {
            backendManager = GetOrCreateComponent<BackendManager>();
        }
    }

    protected virtual void Start() {
        IsLoggedIn = false;

        loginMenu.enabled = true;
        saveMenu.enabled = false;
        saveMenu.SavegameType = typeof(T).Name;

        backendManager.OnLoggedIn += delegate {
            Invoke("EnableSaveMenu", 1.0f);
        };

        saveMenu.OnSaveButtonPressed += delegate (string filename, int savegameId) {
            Savegame savegame = new Savegame() { Id = savegameId, Name = filename, 
                Type = typeof(T).Name, File = JsonConvert.SerializeObject(Serialize()) };
            backendManager.SaveGame(savegame);
        };

        saveMenu.OnLoadButtonPressed += delegate(string filename) {
            StartCoroutine(DownloadSaveFile(filename));
        };
    }

    protected bool CanClick() {
        foreach (BaseMenu menu in FindObjectsOfType<BaseMenu>()) {
            if (menu.IsMouseOver()) {
                return false;
            }
        }
        return true;
    }

    protected virtual void EnableSaveMenu() {
        saveMenu.LoadSavegames();
        loginMenu.enabled = false;
        saveMenu.enabled = true;
        IsLoggedIn = true;
    }

    private U GetOrCreateComponent<U>() where U : Component {
        U comp = FindObjectOfType<U>();
        if (comp == null) {
            comp = gameObject.AddComponent<U>();
        }
        return comp;
    }

    private IEnumerator DownloadSaveFile(string file) {
        WWW www = new WWW(file);
        yield return www;

        T data = JsonConvert.DeserializeObject<T>(www.text);
        Deserialize(data);

        saveMenu.SetStatus("Game is loaded.");
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
