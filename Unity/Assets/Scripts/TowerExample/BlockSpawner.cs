using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockSpawner : MonoBehaviour {
    public GameObject BlockPrefab;
    public float PushForce = 50f;

    private SavegameMenu gameMenu;
    private List<Block> blocks = new List<Block>();

    public void Reset() {
        int i = blocks.Count;
        while (--i >= 0) {
            Block block = blocks[i];
            blocks.Remove(block);
            Destroy(block.gameObject);
        }
        blocks.Clear();
    }

    public void Spawn(BlockData blockData) {
        GameObject gob = (GameObject)Instantiate(BlockPrefab, blockData.Position, blockData.Rotation);
        blocks.Add(gob.GetComponent<Block>());
    }

    private void Awake() {
        gameMenu = FindObjectOfType<SavegameMenu>();
        if (BlockPrefab == null) {
            Debug.LogWarning("BlockPrefab == null");
            enabled = false;
        }
    }

    private Vector3 GetPosition() {
        Vector3 position = transform.position;
        BoxCollider bc = (BoxCollider)collider;
        float width = bc.bounds.max.x - bc.bounds.min.x;
        position.x = (bc.bounds.min.x) + (width * (Input.mousePosition.x / Screen.width));
        return position;
    }

    private bool CanClick() {
        Vector3 mp = Input.mousePosition;
        mp.y = Mathf.Abs(mp.y - Screen.height);

        if (gameMenu == null) {
            return true;
        }
        if (gameMenu.IsMouseOver()) {
            return false;
        }
        return true;
    }

    private void Update() {
        if (!CanClick()) {
            return;
        }
        if (Input.GetMouseButtonUp(0)) {
            Vector3 worldPos = GetPosition();
            GameObject gob = (GameObject)Instantiate(BlockPrefab, worldPos, transform.rotation);
            blocks.Add(gob.GetComponent<Block>());
        }
        if (Input.GetMouseButtonUp(1)) {
            Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f) && hit.transform.name.Contains("Block")) {
                Rigidbody rigidbody = hit.transform.GetComponent<Rigidbody>();
                if (rigidbody != null) {
                    rigidbody.AddForce(Vector3.forward * PushForce, ForceMode.Impulse);
                    GameObject.Destroy(hit.transform.gameObject, 2f);
                }
            }
        }
	}
}
