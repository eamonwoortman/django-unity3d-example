using UnityEngine;
using System.Collections;

public class BlockSpawner : MonoBehaviour {
    public GameObject BlockPrefab;
    private GameMenu gameMenu;

    void Awake() {
        gameMenu = FindObjectOfType<GameMenu>();
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
        if (gameMenu.InRect(mp)) {
            return false;
        }
        return true;
    }
	void Update () {
        if (!CanClick()) {
            return;
        }
        if (Input.GetMouseButtonUp(0)) {
            Vector3 worldPos = GetPosition();
            Instantiate(BlockPrefab, worldPos, transform.rotation);
        }
        if (Input.GetMouseButtonUp(1)) {
            Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f) && hit.transform.name.Contains("Block")) {
                Rigidbody rigidbody = hit.transform.GetComponent<Rigidbody>();
                if (rigidbody != null) {
                    rigidbody.AddForce(Vector3.forward * 100, ForceMode.Impulse);
                }
            }
        }
	}
}
