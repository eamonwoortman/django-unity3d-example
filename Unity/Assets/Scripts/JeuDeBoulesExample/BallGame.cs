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
    private Transform targetCube;

    [SerializeField]
    private Transform cubeSpawner;

    private List<Ball> balls;
    private float Score;

    protected override void Start() {
        base.Start();

        balls = new List<Ball>();
        highscoreMenu.enabled = false;

        // Setup a delegate which will trigger when we succesfully posted a new highscore to the server.
        backendManager.OnPostScoreSucces += OnPostScoreSuccess;

        // Setup a delegate for when we close the highscore screen. This will reset the game and set it up for a new round of play
        highscoreMenu.OnClose += ResetGame;
    }

    private void OnPostScoreSuccess() {
        // Do a GET request on the server for all the highscores. Whenever this is successfull, the highscore menu will automatticlly be triggered and opened
        backendManager.GetAllScores();
    }

    private void UpdateScore() {
        Score = 0;
        foreach (Ball ball in balls) {
            Vector3 distance = ball.transform.position - targetCube.position;
            Score += Mathf.Max(0.0f, 5.0f - distance.magnitude);
        }
        Score *= 10;
    }

	private void Update () {
        UpdateScore();

        turnText.text = "Turns: " + Data.Turn + " / " + MAX_TURNS;
        scoreText.text = "Score: " + (int)Score;

        if (Input.GetMouseButtonDown(0) && !IsMouseOverMenu() && Data.Turn < MAX_TURNS && isLoggedIn) {
            FireCurrentBall();
            Data.Turn++;

            if (Data.Turn == MAX_TURNS)
                OnGameFinished();
        }

        crosshair.gameObject.SetActive(isLoggedIn);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 100, groundLayer)){
            if(hit.collider.tag == "Board"){
                crosshair.position = hit.point + new Vector3(0, 0.01f, 0);
            }
        }

        defaultBall.transform.LookAt(crosshair);
        defaultBall.transform.Rotate(0, 180, 90);
	}

    protected override bool IsMouseOverMenu() {
        return base.IsMouseOverMenu() || highscoreMenu.IsMouseOver();
    }

    private void FireCurrentBall()
    {
        Vector3 target = crosshair.position - defaultBall.transform.position;
        target.y = 0;

        Ball ball = InitializeBall();

        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Collider>().enabled = true;
        ball.GetComponent<Rigidbody>().AddForce(target * 80);

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

            Ball ball = InitializeBall();
            ball.transform.position = ballData.Position;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Collider>().enabled = true;

            balls.Add(ball);
        }

        targetCube.position = Data.TargetPosition;
        targetCube.rotation = Data.TargetRotation;
    }

    protected override JeuDeBoulesData Serialize() {
        // Fill our data with an array containing the BallData from all balls in the scene
        Data.balls = balls.Select(ball => ball.Data).ToArray();

        Data.TargetPosition = targetCube.position;
        Data.TargetRotation = targetCube.rotation;

        return Data;
    }

    private void OnGameFinished() {
        HideSaveMenu();

        StartCoroutine(PostScores());
    }

    private IEnumerator PostScores() {
        yield return new WaitForFixedUpdate();

        // Every 0.5 second, check if velocity of balls is below the BALL_VELOCITY_THRESHOLD, if so, then post scores. 
        while (balls.Where(ball => ball.GetComponent<Rigidbody>().velocity.sqrMagnitude > BALL_VELOCITY_THRESHOLD).ToArray().Length != 0)
            yield return new WaitForSeconds(2f);

        highscoreMenu.enabled = true;
        highscoreMenu.CurrentScore = Score;

        // Post our final score to the back-end. When this request is succesfull, it will trigger the ExampleBackend.OnPostScoreSucces() delegate. 
        backendManager.PostScore((int)Score);
    }

    private void RemoveBalls() {
        foreach (Ball ball in balls) {
            Destroy(ball.gameObject);
        }

        balls.Clear();
    }

    public void ResetGame() {
        RemoveBalls();

        Data.Turn = 0;

        ShowSaveMenu();
        highscoreMenu.enabled = false;

        // Set our target cube at the random position and rotation
        targetCube.position = cubeSpawner.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-0.5f, 0.5f));
        targetCube.rotation = Random.rotation;
    }

}
