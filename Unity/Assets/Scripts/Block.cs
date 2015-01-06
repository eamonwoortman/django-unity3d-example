using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name.Equals("Ground")) {
            Destroy(gameObject, 1f);
        }
    }
}
