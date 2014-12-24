using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json.Linq;

class ScoreObject {
    public string name;
    public int score;
}

public partial class BackendManager : MonoBehaviour {

    void Start() {
        ScoreObject so = new ScoreObject() { name = "Bas", score = 1337 };
        string jsonObject = JsonConvert.SerializeObject(so);

        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        fields.Add("name", "dada");

        PerformRequest("addscore", fields);
        //PerformRequest("addscore", System.Text.Encoding.UTF8.GetBytes(jsonObject));
    }
    void OnTestResponse(ResponseType responseType, object responseData) {
        Debug.Log("responseType=" + responseType + ", " + responseData);
    }

    //---- Public Enums ----//
    public enum ResponseType {
        Success,
        TimedOut,
        ErrorFromClient,
        ErrorFromServer,
        ParseError,
        BackendDisabled,
        RequestError
    }

    //---- Public Delegates ----//
    /// <summary>
    /// The response delegate
    /// </summary>
    /// <param name="responseType"></param>
    /// <param name="responseData">can return a string or JObject</param>
    public delegate void RequestResponseDelegate(ResponseType responseType, object responseData);


    //---- URLS ----//
    [SerializeField]
    bool useProduction = false;
    [SerializeField]
    string productionUrl = "http://localhost:8000/api/";
    [SerializeField]
    string developmentUrl = "http://localhost:8000/api/";


    //---- Private Properties ----//
    string hostUrl {
        get {
            return useProduction ? productionUrl : developmentUrl;
        }
    }


    //---- Private Methods ----//
    /// <summary>Performs a request to the backend.</summary>
    /// <param name="command">Command that is pasted after the url to backend. For example: "localhost:8000/api/" + command</param>
    /// <param name="fields">A list of fields that are send as parameters to the backend</param>
    /// <param name="onSucces">Will be callend on success</param>
    /// <param name="onError">Will be called when an error occurred during the request</param>
    void PerformRequest(string command, Dictionary<string, object> fields = null, RequestResponseDelegate onResponse = null) {
        if (onResponse != null) {
            onResponse(ResponseType.BackendDisabled, null);
            return;
        }

        WWWForm wwwForm = new WWWForm();
        WWW request;

        string url = hostUrl + command;

        if (fields != null) {
            foreach (KeyValuePair<string, object> pair in fields) {
                wwwForm.AddField(pair.Key, pair.Value.ToString());
            }
            request = new WWW(url, wwwForm);
        } else {
            request = new WWW(url);
        }

        StartCoroutine(HandleRequest(request, onResponse));
    }
    void PerformRequest(string command, byte[] data, RequestResponseDelegate onResponse = null) {
        if (onResponse != null) {
            onResponse(ResponseType.BackendDisabled, null);
            return;
        }

        WWWForm wwwForm = new WWWForm();
        WWW request;

        string url = hostUrl + command;

        if (data != null) {
            request = new WWW(url, data);
        } else {
            request = new WWW(url);
        }

        StartCoroutine(HandleRequest(request, onResponse));
    }
    IEnumerator HandleRequest(WWW request, RequestResponseDelegate onResponse) {
        //Wait till request is done
        while (true) {
            if (request.isDone) {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        if (!String.IsNullOrEmpty(request.error)) {
            Debug.LogWarning(request.error);
            Debug.LogWarning(request.text);

            if (onResponse != null) {
                onResponse(ResponseType.RequestError, null);
            }
            yield break;
        }

        string responseData = request.text;

        //Check if a local error occurred
        if (!string.IsNullOrEmpty(request.error)) {
            if (onResponse != null) {
                onResponse(ResponseType.ErrorFromClient, "Error from client: " + request.error);
            }
        } else {
            foreach(KeyValuePair<string, string> pair in request.responseHeaders) {
                Debug.Log("Header[" + pair.Key + "] " + pair.Value);
            }
            /*
            try {
                JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(responseData);
                if (onResponse != null) {
                    //Check if status is succes
                    if (response.status.Equals("success")) {
                        onResponse(ResponseType.Success, response.data);
                    } else {
                        onResponse(ResponseType.ErrorFromServer, response.data);
                    }
                }
                //Debug.Log("Response: " + response.ToString());
            } catch (JsonReaderException jre) {
                Debug.LogError("Could not parse response: " + responseData);
                Debug.LogError("JsonReaderException caught: " + jre.ToString());
                onResponse(ResponseType.ErrorFromClient, jre.ToString());
            } catch (Exception ex) {
                Debug.LogError("Might not be able to parse response: " + responseData);
                Debug.LogError("Unknonwn exception caught: " + ex.ToString());
                onResponse(ResponseType.ErrorFromClient, ex.ToString());
            }*/
        }
    }
}