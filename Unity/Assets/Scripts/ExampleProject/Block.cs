using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
    public BlockData Data = new BlockData();
    private Transform myTransform;

    private void Awake() {
        myTransform = transform;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.name.Equals("Ground")) {
            Destroy(gameObject, 1f);
        }
    }

    private void Update() {
        Data.Position = myTransform.position;
        Data.Rotation = myTransform.rotation;
    }
}
