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
using System;
using System.Collections.Generic;
using System.Linq;

public class HighscoreMenu : BaseMenu {
    public VoidDelegate OnClose;
    public float CurrentScore;

    private const int Height = 300;
    private const int Width = 300;
    private bool loading;
    private List<Score> scores;
    private Score newestScore;

    public HighscoreMenu() {
        windowRect = new Rect(Screen.width / 2 - Width / 2, Screen.height / 2 - Height / 2, Width, Height);
    }

    private void Start() {
        loading = true;
        backendManager.OnScoresLoaded += OnScoresLoaded;
    }

    private void OnScoresLoaded(List<Score> newScores) {
        scores = newScores.OrderBy(s => s.Amount).ThenBy(s => s.Updated).Reverse().ToList();
        newestScore = scores.OrderByDescending(s => s.Updated).FirstOrDefault(s => s.Amount == (int)CurrentScore);
        loading = false;
    }

    private void DrawHeader() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("#", GUILayout.Width(15));
        GUILayout.Label("Name", GUILayout.Width(75));
        GUILayout.Label("Date", GUILayout.Width(75));
        GUILayout.Label("Score", GUILayout.Width(40));
        GUILayout.EndHorizontal();
    }

    private void DrawScore(int place, Score score) {
        GUILayout.BeginHorizontal();
        GUILayout.Label(place + ".", GUILayout.Width(20));
        GUILayout.Label(score.Owner_Name, GUILayout.Width(75));
        GUILayout.Label(score.Updated.ToShortDateString(), GUILayout.Width(75));
        GUILayout.Label(score.Amount.ToString(), GUILayout.Width(50));
        if (newestScore != null && newestScore == score) {
            GUILayout.Label(" <<<");
        }
        GUILayout.EndHorizontal();
    }

    private void DrawLine() {
        const int num = 40;
        string lineStr = "";
        for (int i = 0; i < num; i++) {
            lineStr += "_";
        }
        GUILayout.Label(lineStr);
    }

    private void ShowWindow(int id) {
        DrawHeader();
        DrawLine();

        GUILayout.Space(10);

        if (loading) {
            GUILayout.Label("Loading..");
            return;
        }

        if (scores == null) {
            return;
        }

        int place = 1;
        foreach (Score score in scores) {
            DrawScore(place++, score);
        }

        if (newestScore == null) {
            DrawLine();
            GUILayout.Label("Your score of " + (int)CurrentScore + " didn't make it to the board!");
        }
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("OK")) {
            if (OnClose != null) {
                OnClose();
            }
        }
    }

    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(1, windowRect, ShowWindow, "Highscore");
    }
}
