using UnityEngine;
using System.Collections;

public class ConfirmPopup : MonoBehaviour {
    public const int WINDOW_ID = 5;
    public const int WIDTH = 300;
    public const int HEIGHT = 160;

    public string Title = "Unnamed popup";
    public string Text = "Lorum Ipsum Yolo Swaggings";
    public bool IsNotification;

    public delegate void VoidDelegate();
    public VoidDelegate OnConfirmed;
    public VoidDelegate OnCanceled;
    public bool DontDestroyGameobject;

    private Rect windowRect;

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
        GUILayout.Window(WINDOW_ID, windowRect, ConfirmWindow, Title, GUILayout.MaxHeight(300));
    }
	
}
