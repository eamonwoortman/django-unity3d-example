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
using System.Collections;

public class BaseMenu : MonoBehaviour {
    public delegate void VoidDelegate();
    public GUISkin Skin;

    protected Rect windowRect;
    protected BackendManager backendManager;

    public bool IsMouseOver() {
        Vector3 mp = Input.mousePosition;
        mp.y = Mathf.Abs(mp.y - Screen.height);
        return InRect(mp) && enabled;
    }

    private void Awake() {
        backendManager = GetComponent<BackendManager>();
        if (backendManager == null) {
            Debug.LogWarning("BackendManager not found, disabling menu.");
            enabled = false;
        }
    }

    private bool InRect(Vector3 mousePosition) {
        return windowRect.Contains(mousePosition);
    }
}
