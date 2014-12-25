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

public partial class BackendManager : MonoBehaviour {

    void Start() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        //fields.Add("name", "dada");
        PerformRequest("addscore", fields, OnTestResponse);
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
    /// <param name="responseData">returns the json string of the response</param>
    public delegate void RequestResponseDelegate(ResponseType responseType, object jsonResponse);


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
        string url = hostUrl + command;
        WWW request;
        WWWForm wwwForm = new WWWForm();
        Hashtable ht = new Hashtable();
        ht.Add("Accept", "application/json");

        if (fields != null) {
            foreach (KeyValuePair<string, object> pair in fields) {
                wwwForm.AddField(pair.Key, pair.Value.ToString());
            }
            request = new WWW(url, wwwForm.data, ht);
        } else {
            request = new WWW(url);
        }

        StartCoroutine(HandleRequest(request, onResponse));
    }
    void PerformRequest(string command, byte[] data, RequestResponseDelegate onResponse = null) {
        string url = hostUrl + command;
        WWW request;

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

        //a proper client error(eg. can't reach the server)
        if (!String.IsNullOrEmpty(request.error)) {
            if (onResponse != null) {
                onResponse(ResponseType.RequestError, null);
            }
            yield break;
        }

        //if it's a 400 Bad Request, it's a valid error, otherwise it's a request error
        string status = request.responseHeaders["REAL_STATUS"];
        if (!status.Equals("200 OK")) {
            if (onResponse != null) {
                Dictionary<string, string[]> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(request.text);
                onResponse(ResponseType.ErrorFromServer, responseDict);
            }
            yield break;
        }
         
        //deal with successful responses
        string responseData = request.text;
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
        }
        */
    }
}