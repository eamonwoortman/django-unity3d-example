using UnityEngine;
using System.Collections;

public class BaseMenu : MonoBehaviour {
    public GUISkin Skin;
    protected Rect windowRect;

    public bool InRect(Vector3 mousePosition) {
        return windowRect.Contains(mousePosition);
    }

    public bool IsMouseOver() {
        Vector3 mp = Input.mousePosition;
        mp.y = Mathf.Abs(mp.y - Screen.height);
        return InRect(mp);
    }
}
