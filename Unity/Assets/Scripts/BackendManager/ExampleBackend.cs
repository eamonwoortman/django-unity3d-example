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

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class BackendManager {
    public delegate void LoginFailed(string errorMsg);
    public delegate void LoggedIn();
    public LoggedIn OnLoggedIn;
    public LoginFailed OnLoginFailed;

    public delegate void SignupFailed(string errorMsg);
    public delegate void SignupSuccess();
    public SignupSuccess OnSignupSuccess;
    public SignupFailed OnSignupFailed;

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

    // the authentication token will be set when a user has logged in
    private string authenticationToken = "";
    
    /// <summary>
    /// Does a POST request to the backend, trying to get an authentication token. On succes, it will save the auth token for further use. On success, the OnLoggedIn
    /// delegate will be called. On fail, the OnLoginFailed delegate will be called.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
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
        } else if (responseType == ResponseType.ClientError) {
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


    /// <summary>
    /// Does a POST request to the backend, trying to get an authentication token. On succes, it will save the auth token for further use. On success, the OnLoggedIn
    /// delegate will be called. On fail, the OnLoginFailed delegate will be called.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="email"></param>
    /// <param name="password"></param>
    public void Signup(string username, string email, string password) {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("email", email);
        form.AddField("password", password);
        Send(RequestType.Post, "user", form, OnSignupResponse);
    }

    private void OnSignupResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            if (OnSignupSuccess != null) {
                OnSignupSuccess();
            }
        } else if (responseType == ResponseType.ClientError) {
            if (OnSignupFailed != null) {
                OnSignupFailed("Could not reach the server. Please try again later.");
            }
        } else if (responseType == ResponseType.RequestError) {
            string errors = "";
            JObject obj = (JObject)responseData;
            foreach (KeyValuePair<string, JToken> pair in obj) {
                errors += "[" + pair.Key + "] ";
                foreach (string errStr in pair.Value) {
                    errors += errStr;
                }
                errors += '\n';
            }
            if (OnSignupFailed != null) {
                OnSignupFailed(errors);
            }
        }
    }


    /// <summary>
    /// Does a POST or PUT request to the server, depending on if the SaveGame you provide has an id or not. If the id is present, the savegame will be updated by of PUT request. Else
    /// a new savegame will be POST'ed to the server. On success, the OnSaveGameSuccess delegate will be called. On fail, the OnSaveGameFailed delegate will be called.
    /// </summary>
    /// <param name="savegame"></param>
    public void SaveGame(Savegame savegame) {
        WWWForm form = new WWWForm();
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
        } else if (responseType == ResponseType.ClientError) {
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

    /// <summary>
    /// Does a GET request at the server, getting you all the savegames of the giving samegame type. On success, the OnGamesLoaded delegate will be called. On fail, the OnGamesLoadedFailed will be called.
    /// </summary>
    /// <param name="savegameTypeName">The name of the savegame type you wish to get. Example: JeuDeBouleData</param>
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

    /// <summary>
    /// Does a GET request at the backend, getting you all scores. When succesfull, the OnScoresLoaded delegate will be called. When failing, the OnScoresLoadedFailed delegate will be called.
    /// </summary>
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

    /// <summary>
    /// Does a POST request to the backend, containing the score of the player.
    /// </summary>
    /// <param name="score"></param>
    public void PostScore(int score) {
        WWWForm form = new WWWForm();
        form.AddField("score", score);
        Send(RequestType.Post, "score", form, OnPostScoreResponse, authenticationToken);
    }

    private void OnPostScoreResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            if (OnPostScoreSucces != null) {
                OnPostScoreSucces();
            }
        } else if (responseType == ResponseType.ClientError) {
            if (OnPostScoreFailed != null) {
                OnPostScoreFailed("Could not reach the server. Please try again later.");
            }
        } else {
            if (OnPostScoreFailed != null) {
                OnPostScoreFailed("Request failed: " + responseType + " - " + responseData["detail"]);
            }
        }
    }

    /// <summary>
    /// Does a DELETE request to the backend, trying to delete the savegame with the id you provided. On success, the OnDeleteSavegameSucces will be called.
    /// On fail, the OnDeleteSavegameFailed will be called.
    /// </summary>
    /// <param name="index"></param>
    public void DeleteSavegame(int index) {
        Send(RequestType.Delete, "savegame/" + index + "/", null, OnDeleteSavegameResponse, authenticationToken);
    }

    private void OnDeleteSavegameResponse(ResponseType responseType, JToken responseData, string callee) {
        if (responseType == ResponseType.Success) {
            if (OnDeleteSavegameSucces != null) {
                OnDeleteSavegameSucces();
            }
        } else if (responseType == ResponseType.ClientError) {
            if (OnDeleteSavegameFailed != null) {
                OnDeleteSavegameFailed("Could not reach the server. Please try again later.");
            }
        } else {
            if (OnDeleteSavegameFailed != null) {
                OnDeleteSavegameFailed("Request failed: " + responseType + " - " + responseData["detail"]);
            }
        }
    }

    /// <summary>
    /// Helper method which will check and fill the given string[] array, if the given JToken has the given key
    /// </summary>
    /// <param name="jsonObject"></param>
    /// <param name="key"></param>
    /// <param name="values"></param>
    /// <returns></returns>
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
