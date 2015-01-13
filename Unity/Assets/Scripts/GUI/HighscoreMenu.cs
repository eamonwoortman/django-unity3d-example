using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class HighscoreMenu : BaseMenu {

    public Action OnCancel;
    public bool Loading;
    public Score newestScore;

    public List<Score> Scores {
        get {
            return scores;
        }
        set {
            scores = value.OrderBy(s => s.Amount).ThenBy(s => s.Updated).Reverse().ToList();
            newestScore = scores.OrderByDescending(s => s.Updated).First();
        }
    }

    private List<Score> scores;

    public HighscoreMenu() {
        windowRect = new Rect(Screen.width  / 2 - 100, Screen.height / 2 - 100, 200, 200);
    }
    
    private void ShowWindow(int id) {

        if (Loading) {
            GUILayout.Label("Posting highscore..");
        } else {
            foreach (Score score in scores) {
                GUILayout.Label(score.Updated.ToShortDateString() + " - " + score.Amount.ToString() + (newestScore == score ? " <<<" : ""));
            }
            if (GUILayout.Button("OK")) {
                if (OnCancel != null) {
                    OnCancel();
                }
            }
        }
    }

    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(0, windowRect, ShowWindow, "Highscore");
    }
	
}
