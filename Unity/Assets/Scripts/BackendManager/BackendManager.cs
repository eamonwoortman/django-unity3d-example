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

#define UNITY_4_PLUS
#define UNITY_5_PLUS

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
    #define UNITY_4_X
    #undef UNITY_5_PLUS
#elif UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
    #define UNITY_5_X
#endif

using UnityEngine;
using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

//---- Public Enums ----//
public enum ResponseType {
    /// <summary>
    /// ClientError, the client could not perform the request(eg. could not reach destination host)
    /// </summary>
    ClientError,
    /// <summary>
    /// PageNotFound, the page could not be found(invalid url)
    /// </summary>
    PageNotFound,
    /// <summary>
    /// The server returned an error regarding the request, which can be invalid post data or invalid authentication for example
    /// </summary>
    RequestError,
    /// <summary>
    /// The server returned an error but the response body could not be parsed
    /// </summary>
    ParseError,
    /// <summary>
    /// Success, if the server returns content it will be parsed into a JObject or JArray.
    /// </summary>
    Success,
}

/// <summary>
/// The type of the request(sets the HTTP method)
/// </summary>
public enum RequestType {
    /// <summary>
    /// GET
    /// </summary>
    Get,
    /// <summary>
    /// POST
    /// </summary>
    Post,
    /// <summary>
    /// PUT
    /// </summary>
    Put,
    /// <summary>
    /// DELETE
    /// </summary>
    Delete
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


    //---- Public Properties ----//
    public string BackendUrl {
        get {
            return UseProduction ? ProductionUrl : DevelopmentUrl;
        }
    }

    //---- URLS ----//
    public bool UseProduction = false;
    public bool Secure;
    public string ProductionUrl = "http://foobar:8000/api/";
    public string DevelopmentUrl = "http://localhost:8000/api/";

    //---- Private Methods ----//

    /// <summary>Performs a request to the backend.</summary>
    /// <param name="type">The request type(Get, Post, Update, Delete)</param>
    /// <param name="command">Command that is pasted after the url to backend. For example: "localhost:8000/api/" + command</param>
    /// <param name="wwwForm">A WWWForm to send with the request</param>
    /// <param name="onResponse">A callback which will be called when we retrieve the response</param>
    /// <param name="authToken">An optional authToken which, when set will be put in the Authorization header</param>
    public void Send(RequestType type, string command, WWWForm wwwForm, RequestResponseDelegate onResponse = null, string authToken = "") {
        WWW request;
#if UNITY_5_PLUS
        Dictionary<string, string> headers;
#else
        Hashtable headers;
#endif
        byte[] postData;
        string url = BackendUrl + command;

        if (Secure) {
            url = url.Replace("http", "https");
        }

        if (wwwForm == null) {
            wwwForm = new WWWForm();
            postData = new byte[] { 1 };
        } else {
            postData = wwwForm.data;
        }

        headers = wwwForm.headers;
        
        //make sure we get a json response
        headers.Add("Accept", "application/json");

        //also add the correct request method
        headers.Add("X-UNITY-METHOD", type.ToString().ToUpper());

        //also, add the authentication token, if we have one
        if (authToken != "") {
            //for more information about token authentication, see: http://www.django-rest-framework.org/api-guide/authentication/#tokenauthentication
            headers.Add("Authorization", "Token " + authToken);
        }
        request = new WWW(url, postData, headers);

        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        string callee = stackTrace.GetFrame(1).GetMethod().Name;
        StartCoroutine(HandleRequest(request, onResponse, callee));
    }
    
    private IEnumerator HandleRequest(WWW request, RequestResponseDelegate onResponse, string callee) {
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
                onResponse(ResponseType.ClientError, null, callee);
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
                    if (statusCode == 404) {
                        //404's should not be treated as unparsable
                        Debug.LogWarning("Page not found: " + request.url);
                        onResponse(ResponseType.PageNotFound, null, callee);
                    } else {
                        Debug.Log("Could not parse the response, request.text=" + request.text);
                        Debug.Log("Exception=" + ex.ToString());
                        onResponse(ResponseType.ParseError, null, callee);
                    }
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
                onResponse(ResponseType.RequestError, responseObj, callee);
            }
            yield break;
        }
         
        //deal with successful responses
        if (onResponse != null) {
            onResponse(ResponseType.Success, responseObj, callee);
        }
    }
}
