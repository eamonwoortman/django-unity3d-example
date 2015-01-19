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
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class BallGame : BaseGame<JeuDeBoulesData> {

    public const int MAX_TURNS = 5;
    private const float BALL_VELOCITY_THRESHOLD = 0.005f;

    [SerializeField]
    private Ball defaultBall;

    [SerializeField]
    private Transform dartStartPosition;

    [SerializeField]
    private Transform crosshair;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private HighscoreMenu highscoreMenu;

    [SerializeField]
    private GUIText turnText;

    [SerializeField]
    private GUIText scoreText;

    [SerializeField]
    private Transform targetBall;

    private List<Ball> balls;

    protected override void Start() {
        base.Start();

        highscoreMenu.enabled = false;
        highscoreMenu.OnCancel += delegate {
            ResetGame();
        };

        backendManager.OnPostScoreSucces += delegate {
            backendManager.GetAllScores();
        };

        backendManager.OnScoresLoaded += delegate(List<Score> scores) {
            highscoreMenu.Scores = scores;
            highscoreMenu.Loading = false;
        };
        balls = new List<Ball>();
    }
	
	void Update () {
        Data.Score = 0;
        foreach (Ball ball in balls) {
            Vector3 distance = ball.transform.position - targetBall.position;
            Data.Score += Mathf.Max(0.0f, 5.0f - distance.magnitude);
        }
        Data.Score *= 10;

        turnText.text = Data.Turn + "/" + MAX_TURNS + " turns";
        scoreText.text = "Score: " + (int)Data.Score;

        if (Input.GetMouseButtonDown(0) && !IsMouseOverMenu() && Data.Turn < MAX_TURNS && IsLoggedIn) {
            FireCurrentBall();
            Data.Turn++;

            if (Data.Turn == MAX_TURNS)
                OnGameFinished();
        }

        crosshair.gameObject.SetActive(IsLoggedIn);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 100, groundLayer)){
            if(hit.collider.tag == "Board"){
                crosshair.position = hit.point + new Vector3(0, 0.01f, 0);
            }
        }
	}

    protected override bool IsMouseOverMenu() {
        return base.IsMouseOverMenu() || highscoreMenu.IsMouseOver();
    }

    private void FireCurrentBall()
    {
        Vector3 target = crosshair.position - defaultBall.transform.position;
        target.y = 0;

        Ball ball = InitializeBall();

        ball.rigidbody.isKinematic = false;
        ball.collider.enabled = true;
        ball.BallData.IsThrown = true;

        ball.rigidbody.AddForce(target * 80);

        balls.Add(ball);

        defaultBall.transform.Rotate(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }

    /// <summary>
    /// Will initialize a new Ball gameobject, which will be cloned from the default Ball gameobject which is always in the players 'hand'
    /// </summary>
    /// <returns>The newly created Ball</returns>
    private Ball InitializeBall() {
        GameObject clonedBall = Instantiate(defaultBall.gameObject, dartStartPosition.position, dartStartPosition.rotation) as GameObject;
        Ball ball = clonedBall.GetComponent<Ball>();

        return ball;
    }

    /// <summary>
    /// Loads a saved game. It will remove all current balls and will load the ones from the save file. It will also set the score and the turns.
    /// </summary>
    /// <param name="json"></param>
    protected override void Deserialize(JeuDeBoulesData gameData) {
        ResetGame();

        Data = gameData;

        // Now lets loop through the balldata and create an Ball gameobject in our scene, and set its position to that of the BallData
        foreach (BallData ballData in Data.balls) {

            if (!ballData.IsThrown)
                continue;

            Ball ball = InitializeBall();
            ball.transform.position = ballData.Position;
            ball.rigidbody.isKinematic = false;
            ball.collider.enabled = true;

            balls.Add(ball);
        }
    }

    protected override JeuDeBoulesData Serialize() {
        // Fill our data with an array containing the BallData from all balls in the scene
        Data.balls = balls.Select(ball => ball.BallData).ToArray();

        return Data;
    }

    private void OnGameFinished() {
        HideSaveMenu();

        highscoreMenu.enabled = true;
        highscoreMenu.Loading = true;

        StartCoroutine(PostScores());
    }

    private IEnumerator PostScores() {
        yield return new WaitForFixedUpdate();
        // Every 0.5 second, check if velocity of balls is below the BALL_VELOCITY_THRESHOLD, if so, then post scores. 
        while (balls.Where(ball => ball.rigidbody.velocity.sqrMagnitude > BALL_VELOCITY_THRESHOLD).ToArray().Length != 0)
            yield return new WaitForSeconds(0.5f);

        backendManager.PostScore((int)Data.Score);
    }

    private void RemoveBalls() {
        foreach (Ball ball in balls) {
            Destroy(ball.gameObject);
        }

        balls.Clear();
    }


    public void ResetGame() {
        RemoveBalls();

        Data.Score = 0;
        Data.Turn = 0;

        ShowSaveMenu();
        highscoreMenu.enabled = false;
    }
}
