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

    public delegate void DeleteSavegameSuccess();
    public delegate void DeleteSavegameFailed(string errorMsg);
    public DeleteSavegameSuccess OnDeleteSavegameSucces;
    public DeleteSavegameFailed OnDeleteSavegameFailed;

    public delegate void GamesLoaded(List<Savegame> games);
    public delegate void GamesLoadedFailed(string errorMsg);
    public GamesLoaded OnGamesLoaded;
    public GamesLoadedFailed OnGamesLoadedFailed;

    public delegate void ScoresLoaded(List<Score> scores);
    public delegate void ScoreLoadedFailed(string errorMsg);
    public ScoresLoaded OnScoresLoaded;
    public ScoreLoadedFailed OnScoreLoadedFailed;

    public delegate void PostScoreSucces();
    public delegate void PostScoreFailed(string errorMsg);
    public PostScoreSucces OnPostScoreSucces;
    public PostScoreFailed OnPostScoreFailed;

    private string authenticationToken = "";
    
    public void Login(string username, string password) {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        Send(RequestType.Post, "getauthtoken", form, OnLoginResponse);
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

    public void SaveGame(Savegame savegame) {
        WWWForm form = new WWWForm();
        if (savegame.Id != -1) {
            form.AddField("id", savegame.Id);
        }
        form.AddField("name", savegame.Name);
        form.AddField("type", savegame.Type);
        form.AddBinaryData("file", System.Text.Encoding.UTF8.GetBytes(savegame.File));
        if (savegame.Id == -1) {
            Send(RequestType.Post, "savegame", form, OnSaveGameResponse, authenticationToken);
        } else {
            Send(RequestType.Put, "savegame/" + savegame.Id + "/", form, OnSaveGameResponse, authenticationToken);
        }
        
    }

    private void OnSaveGameResponse(ResponseType responseType, JToken responseData, string callee) {
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

    public void LoadGames(string savegameTypeName) {
        WWWForm form = new WWWForm();
        form.AddField("SavegameType", savegameTypeName);
        Send(RequestType.Get, "savegames/", form, OnLoadGamesResponse, authenticationToken);
    }

    private void OnLoadGamesResponse(ResponseType responseType, JToken responseData, string callee)
    {
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

    public void GetAllScores() {
        Send(RequestType.Get, "score", null, OnGetAllScoresResponse, authenticationToken);
    }

    private void OnGetAllScoresResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            if (OnScoresLoaded != null) {
                OnScoresLoaded(JsonConvert.DeserializeObject<List<Score>>(responseData.ToString()));
            }
        } else {
            if (OnScoreLoadedFailed != null) {
                OnScoreLoadedFailed("Could not reach the server. Please try again later.");
            }
        }
    }

    public void PostScore(int score) {
        WWWForm form = new WWWForm();
        form.AddField("score", score);
        Send(RequestType.Get, "score", form, OnPostScoreResponse, authenticationToken);
    }

    private void OnDeleteSavegameResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            if (OnDeleteSavegameSucces != null) {
                OnDeleteSavegameSucces();
            }
        } else if (responseType == ResponseType.RequestError) {
            if (OnDeleteSavegameFailed != null) {
                OnDeleteSavegameFailed("Could not reach the server. Please try again later.");
            }
        } else {
            if (OnDeleteSavegameFailed != null) {
                OnDeleteSavegameFailed("Request failed: " + responseType + " - " + responseData["detail"]);
            }
        }
    }

    public void DeleteSavegame(int index) {
        WWWForm form = new WWWForm();
        form.AddField("id", index);
        Send(RequestType.Delete, "savegame/" + index + "/", form, OnSaveGameResponse, authenticationToken);
    }

    private void OnPostScoreResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            if (OnPostScoreSucces != null) {
                OnPostScoreSucces();
            }
        } else if (responseType == ResponseType.RequestError) {
            if (OnPostScoreFailed != null) {
                OnPostScoreFailed("Could not reach the server. Please try again later.");
            }
        } else {
            if (OnPostScoreFailed != null) {
                OnPostScoreFailed("Request failed: " + responseType + " - " + responseData["detail"]);
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
