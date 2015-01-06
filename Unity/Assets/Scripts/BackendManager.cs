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
    /// <param name="jsonResponse">the json object of the response, this can be null when no content is returned(eg. HTTP 204)</param>
    /// <param name="callee">the name of the method doing the request(used for testing)</param>
    public delegate void RequestResponseDelegate(ResponseType responseType, JToken jsonResponse, string callee);


    //---- URLS ----//
    [SerializeField]
    public bool UseProduction = false;
    [SerializeField]
    public string ProductionUrl = "http://foobar:8000/api/";
    [SerializeField]
    public string DevelopmentUrl = "http://localhost:8000/api/";


    //---- Private Properties ----//
    string hostUrl {
        get {
            return UseProduction ? ProductionUrl : DevelopmentUrl;
        }
    }


    //---- Private Methods ----//
    /// <summary>Performs a request to the backend.</summary>
    /// <param name="command">Command that is pasted after the url to backend. For example: "localhost:8000/api/" + command</param>
    /// <param name="fields">A list of fields that are send as parameters to the backend</param>
    /// <param name="onResponse">A callback which will be called when we retrieve the response</param>
    /// <param name="authToken">An optional authToken which, when set will be put in the Authorization header</param>
    public void PerformRequest(string command, Dictionary<string, object> fields = null, RequestResponseDelegate onResponse = null, string authToken = "") {
        string url = hostUrl + command;
        WWW request;
        WWWForm wwwForm = new WWWForm();
        Hashtable headers = new Hashtable();

        //make sure we get a json response
        headers.Add("Accept", "application/json");
        //also, add the authentication token, if we have one
        if (authToken != "") {
            //for more information about token authentication, see: http://www.django-rest-framework.org/api-guide/authentication/#tokenauthentication
            headers.Add("Authorization", "Token " + authToken);
        }

        if (fields != null) {
            foreach (KeyValuePair<string, object> pair in fields) {
                wwwForm.AddField(pair.Key, pair.Value.ToString());
            }
            request = new WWW(url, wwwForm.data, headers);
        } else {
            request = new WWW(url);
        }

        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        string callee = stackTrace.GetFrame(1).GetMethod().Name;
        StartCoroutine(HandleRequest(request, onResponse, callee));
    }

    /// <summary>Performs a request to the backend.</summary>
    /// <param name="command">Command that is pasted after the url to backend. For example: "localhost:8000/api/" + command</param>
    /// <param name="wwwForm">A WWWForm to send with the request</param>
    /// <param name="onResponse">A callback which will be called when we retrieve the response</param>
    /// <param name="authToken">An optional authToken which, when set will be put in the Authorization header</param>
    public void PerformFormRequest(string command, WWWForm wwwForm, RequestResponseDelegate onResponse = null, string authToken = "") {
        string url = hostUrl + command;
        WWW request;
        Hashtable headers = wwwForm.headers;
        //make sure we get a json response
        headers.Add("Accept", "application/json");
        //also, add the authentication token, if we have one
        if (authToken != "") {
            //for more information about token authentication, see: http://www.django-rest-framework.org/api-guide/authentication/#tokenauthentication
            headers.Add("Authorization", "Token " + authToken);
        }
            
        request = new WWW(url, wwwForm.data, headers);

        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        string callee = stackTrace.GetFrame(1).GetMethod().Name;
        StartCoroutine(HandleRequest(request, onResponse, callee));
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
        int statusCode = 200;
        
        if (request.responseHeaders.ContainsKey("REAL_STATUS")) {
            string status = request.responseHeaders["REAL_STATUS"];
            statusCode = int.Parse(status.Split(' ')[0]);
        }
        //if any other error occurred(probably 4xx range), see http://www.django-rest-framework.org/api-guide/status-codes/
        bool responseSuccessful = (statusCode >= 200 && statusCode <= 206);
        JToken responseObj = null;

        try {
            if (request.text.StartsWith("[")) { 
                responseObj = JArray.Parse(request.text); 
            } else { 
                responseObj = JObject.Parse(request.text); 
            }
        } catch (Exception ex) {
            if (onResponse != null) {
                if (!responseSuccessful) {
                    Debug.Log("Could not parse the response, request.text=" + request.text);
                    Debug.Log("Exception=" + ex.ToString());
                    onResponse(ResponseType.ParseError, null, callee);
                } else {
                    if (request.text == "") {
                        onResponse(ResponseType.Success, null, callee);
                    } else {
                        Debug.Log("Could not parse the response, request.text=" + request.text);
                        Debug.Log("Exception=" + ex.ToString());
                        onResponse(ResponseType.ParseError, null, callee);
                    }
                }
            }
            yield break;
        }

        if (!responseSuccessful) {
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