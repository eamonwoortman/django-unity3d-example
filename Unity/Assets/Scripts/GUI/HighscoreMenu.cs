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
