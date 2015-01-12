using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BackendManager {
    public delegate void LoginFailed(string errorMsg);
    public delegate void LoggedIn();
    public LoggedIn OnLoggedIn;
    public LoginFailed OnLoginFailed;

    public delegate void SaveGameSuccess();
    public delegate void SaveGameFailed(string errorMsg);
    public SaveGameSuccess OnSaveGameSucces;
    public SaveGameFailed OnSaveGameFailed;

    public delegate void GamesLoaded(List<Savegame> games);
    public delegate void GamesLoadedFailed(string errorMsg);
    public GamesLoaded OnGamesLoaded;
    public GamesLoadedFailed OnGamesLoadedFailed;

    private string authenticationToken = "";
    
    public void Login(string username, string password) {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("username", username);
        fields.Add("password", password);
        PerformRequest("getauthtoken", fields, OnLoginResponse);
    }

    private void OnLoginResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            authenticationToken = responseData.Value<string>("token");
            if (OnLoggedIn != null) {
                OnLoggedIn();
            }
        } else if (responseType == ResponseType.RequestError) {
            if (OnLoginFailed != null) {
                OnLoginFailed("Could not reach the server. Please try again later.");
            }
        } else {
            JToken fieldToken = responseData["non_field_errors"];
            if (fieldToken == null || !fieldToken.HasValues) {
                if (OnLoginFailed != null) {
                    OnLoginFailed("Login failed: unknown error.");
                }
            } else {
                string errors = "";
                JToken[] fieldValidationErrors = fieldToken.Values().ToArray();
                foreach (JToken validationError in fieldValidationErrors) {
                    errors += validationError.Value<string>();
                }
                if (OnLoginFailed != null) {
                    OnLoginFailed("Login failed: " + errors);
                }
            }
        }
    }

    public void SaveGame(string name, string file) {
        WWWForm form = new WWWForm();
        form.AddField("name", name);
        form.AddBinaryData("file", System.Text.Encoding.UTF8.GetBytes(file));
        PerformFormRequest("savegame", form, OnSaveGame, authenticationToken);
    }

    private void OnSaveGame(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            if (OnSaveGameSucces != null) {
                OnSaveGameSucces();
            }
        } else if (responseType == ResponseType.RequestError) {
            if (OnSaveGameFailed != null) {
                OnSaveGameFailed("Could not reach the server. Please try again later.");
            }
        } else {
            string[] errors;
            if (!ContainsSubfield(responseData, "name", out errors) && OnSaveGameFailed != null) {
                OnSaveGameFailed("Request failed: " + responseData + " - Name was not provided");
            } else if(OnSaveGameFailed != null){
                OnSaveGameFailed("Request failed: " + responseType + " - " + responseData["detail"]);
            }
        }
    }

    public void LoadGames() {
        PerformRequest("savegame", null, OnLoadGames, authenticationToken);
    }

    private void OnLoadGames(ResponseType responseType, JToken responseData, string callee)
    {
        Debug.Log(responseData.ToString());
        if (responseType == ResponseType.Success) {
            if (OnGamesLoaded != null) {
                OnGamesLoaded(JsonConvert.DeserializeObject<List<Savegame>>(responseData.ToString()));
            }
        } else {
            if (OnGamesLoadedFailed != null) {
                OnGamesLoadedFailed("Could not reach the server. Please try again later.");
            }
        }
    }

    private bool ContainsSubfield(JToken jsonObject, string key, out string[] values) {
        JToken fieldToken = jsonObject[key];
        values = new string[0];

        if (fieldToken == null || !fieldToken.HasValues) {
            return true;
        }

        values = fieldToken.Values().ToArray().Select(token => token.Value<string>()).ToArray();

        return values.Length == 0;
    }
}
