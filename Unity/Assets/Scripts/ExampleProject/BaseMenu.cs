using UnityEngine;
using System.Collections;

public class BaseMenu : MonoBehaviour {
    public GUISkin Skin;
    protected Rect windowRect;
    protected BackendManager backendManager;

    public bool InRect(Vector3 mousePosition) {
        return windowRect.Contains(mousePosition);
    }

    public bool IsMouseOver() {
        Vector3 mp = Input.mousePosition;
        mp.y = Mathf.Abs(mp.y - Screen.height);
        return InRect(mp);
    }

    private void Awake() {
        backendManager = GetComponent<BackendManager>();
        if (backendManager == null) {
            Debug.LogWarning("BackendManager not found, disabling menu.");
            enabled = false;
        }
    }
}
