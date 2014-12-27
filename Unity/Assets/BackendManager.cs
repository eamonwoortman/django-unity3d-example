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


//---- Public Enums ----//
public enum ResponseType {
    Success,
    ErrorFromClient,
    ErrorFromServer,
    ParseError,
    BackendDisabled,
    RequestError
}

public partial class BackendManager : MonoBehaviour {
    //---- Public Delegates ----//
    /// <summary>
    /// The response delegate
    /// </summary>
    /// <param name="responseType"></param>
    /// <param name="jsonResponse">the json object of the response</param>
    /// <param name="callee">the name of the method doing the request(used for testing)</param>
    public delegate void RequestResponseDelegate(ResponseType responseType, JObject jsonResponse, string callee);


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
    public void PerformRequest(string command, Dictionary<string, object> fields = null, RequestResponseDelegate onResponse = null) {
        string url = hostUrl + command;
        WWW request;
        WWWForm wwwForm = new WWWForm();
        Hashtable ht = new Hashtable();
        //make sure we get a json response
        ht.Add("Accept", "application/json");

        if (fields != null) {
            foreach (KeyValuePair<string, object> pair in fields) {
                wwwForm.AddField(pair.Key, pair.Value.ToString());
            }
            request = new WWW(url, wwwForm.data, ht);
        } else {
            request = new WWW(url);
        }

        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        string callee = stackTrace.GetFrame(1).GetMethod().Name;
        StartCoroutine(HandleRequest(request, onResponse, callee));
    }

    public void PerformRequest(string command, byte[] data, RequestResponseDelegate onResponse = null) {
        string url = hostUrl + command;
        WWW request;
        if (data != null) {
            request = new WWW(url, data);
        } else {
            request = new WWW(url);
        }
        StartCoroutine(HandleRequest(request, onResponse, ""));
    }

    IEnumerator HandleRequest(WWW request, RequestResponseDelegate onResponse, string callee) {
        //Wait till request is done
        while (true) {
            if (request.isDone) {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        //catch proper client errors(eg. can't reach the server)
        if (!String.IsNullOrEmpty(request.error)) {
            if (onResponse != null) {
                onResponse(ResponseType.RequestError, null, callee);
            }
            yield break;
        }

        string status = request.responseHeaders["REAL_STATUS"];
        int statusCode = int.Parse(status.Split(' ')[0]);
        JObject responseObj = JObject.Parse(request.text);

        //if any other error occurred(probably 4xx range), see http://www.django-rest-framework.org/api-guide/status-codes/
        if (statusCode < 200 || statusCode > 206) {
            if (onResponse != null) {
                onResponse(ResponseType.ErrorFromServer, responseObj, callee);
            }
            yield break;
        }
         
        //deal with successful responses
        if (onResponse != null) {
            onResponse(ResponseType.Success, responseObj, callee);
        }
    }
}