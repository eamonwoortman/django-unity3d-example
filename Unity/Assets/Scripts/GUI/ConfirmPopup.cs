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

public class ConfirmPopup : BaseMenu {
    public const int WINDOW_ID = 5;
    public const int WIDTH = 300;
    public const int HEIGHT = 160;

    public bool IsNotification;
    public bool DontDestroyGameobject;

    public string Title = "Unnamed popup";
    public string Text = "Lorum Ipsum Yolo Swaggings";
    
    public VoidDelegate OnConfirmed;
    public VoidDelegate OnCanceled;
    
    public static ConfirmPopup Create(string title, string text, bool isNotification = false) {
        GameObject gob = new GameObject("ConfirmPopup - " + title);
        ConfirmPopup popup = gob.AddComponent<ConfirmPopup>();
        popup.Title = title;
        popup.Text = text;
        popup.IsNotification = isNotification;
        return popup;
    }

    public void Close() {
        OnCanceled = null;
        OnConfirmed = null;
        if (DontDestroyGameobject) {
            Destroy(this);
        } else {
            Destroy(gameObject);
        }
    }

    private void Awake() {
        windowRect = new Rect(Screen.width / 2 - WIDTH / 2, Screen.height / 2 - HEIGHT / 2, WIDTH, HEIGHT);
    }

    private void ConfirmWindow(int id) {
        GUILayout.BeginVertical();

        GUILayout.Label(Text);
        GUILayout.FlexibleSpace();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Okay")) {
            if (OnConfirmed != null) {
                OnConfirmed();
            }
            Close();
        }

        if (!IsNotification && GUILayout.Button("Cancel")) {
            if (OnCanceled != null) {
                OnCanceled();
            }
            Close();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void OnGUI() {
        GUILayout.Window(0, windowRect, ConfirmWindow, Title, GUILayout.MaxHeight(300));
    }
}
