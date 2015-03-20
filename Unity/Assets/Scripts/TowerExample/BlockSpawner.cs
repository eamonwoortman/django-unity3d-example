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

using UnityEngine;

public class BlockSpawner : MonoBehaviour {
    public GameObject BlockPrefab;
    public float PushForce = 50f;

    private SavegameMenu gameMenu;

    public void Reset() {
        Block[] blocks = FindObjectsOfType<Block>();
        int i = blocks.Length;
        while (--i >= 0) {
            if (blocks[i].gameObject == BlockPrefab) {
                continue;
            }
            Destroy(blocks[i].gameObject);
            blocks[i] = null;
        }
    }

    public void Spawn(BlockData blockData) {
        Instantiate(BlockPrefab, blockData.Position, blockData.Rotation);
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
        BoxCollider bc = (BoxCollider)GetComponent<Collider>();
        float width = bc.bounds.max.x - bc.bounds.min.x;
        position.x = (bc.bounds.min.x) + (width * (Input.mousePosition.x / Screen.width));
        return position;
    }

    private bool CanClick() {
        if (gameMenu != null && gameMenu.IsMouseOver()) {
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
            Instantiate(BlockPrefab, worldPos, transform.rotation);
        }

        if (Input.GetMouseButtonUp(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
