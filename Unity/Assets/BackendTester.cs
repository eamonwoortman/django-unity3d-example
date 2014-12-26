using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class BackendTester : MonoBehaviour {
    private BackendManager backendManager;
    void Start() {
        backendManager = GetComponent<BackendManager>();
        if (backendManager == null) {
            backendManager = gameObject.AddComponent<BackendManager>();
        }
        StartTests();
    }
    void StartTests() {
        Test1();
    }

    void Test1() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        backendManager.PerformRequest("addscore", fields, OnTestResponse);
    }

    void Test2() {
        Dictionary<string, object> fields = new Dictionary<string, object>();
        fields.Add("score", 1337);
        fields.Add("name", "dada");
        backendManager.PerformRequest("addscore", fields, OnTestResponse);
    }

    void OnTestResponse(ResponseType responseType, JObject responseData) {
        Debug.Log("responseType=" + responseType + ", " + responseData);
        if (responseType == ResponseType.ErrorFromServer) {
            foreach (KeyValuePair<string, JToken> pair in responseData) {
                Debug.Log("Token[" + pair.Key + "] " + pair.Value);
            }
        }
    }

}
