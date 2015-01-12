using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class BallGame : MonoBehaviour {

    public const int MAX_TURNS = 5;

    [SerializeField]
    private Ball defaultBall;

    [SerializeField]
    private Transform dartStartPosition;

    [SerializeField]
    private Transform crosshair;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private LoginMenu loginMenu;

    [SerializeField]
    private SavegameMenu saveMenu;

    [SerializeField]
    private EnterNameMenu nameMenu;

    [SerializeField]
    private BackendManager backendManager;

    [SerializeField]
    private GUIText turnText;

    [SerializeField]
    private GUIText scoreText;

    [SerializeField]
    private Transform targetBall;

    public float Score { get; private set; }
    public int Turn { get; private set; }

    private List<Ball> balls;

    private void Start() {

        loginMenu.enabled = true;
        saveMenu.enabled = true;
        nameMenu.enabled = false;

        saveMenu.OnSaveButtonPressed += delegate {
            Save();
        };

        balls = new List<Ball>();
    }
	
	void Update () {
        Score = 0;
        foreach (Ball ball in balls) {
            Vector3 distance = ball.transform.position - targetBall.position;
            Score += Mathf.Max(0.0f, 5.0f - distance.magnitude);
        }
        Score *= 10;

        turnText.text = Turn + "/" + MAX_TURNS + " turns";
        scoreText.text = "Score: " + (int)Score;

        if (Input.GetMouseButtonDown(0) && !IsMouseOverMenu() && Turn < MAX_TURNS) {
            FireCurrentBall();
            Turn++;

            if (Turn == MAX_TURNS)
                OnGameFinished();
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 999, groundLayer)){
            if(hit.collider.tag == "Board"){
                crosshair.position = hit.point + new Vector3(0, 0.01f, 0);
            }
        }
	}

    private bool IsMouseOverMenu() {
        return saveMenu.IsMouseOver() || loginMenu.IsMouseOver() || nameMenu.IsMouseOver();
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
    public void Load(string json) {

        ResetGame();

        // Deserialize the JSON string we got from the server into a array of BallData
        BallData[] data = JsonConvert.DeserializeObject<BallData[]>(json);

        // Now lets loop through the balldata and create an Ball gameobject in our scene, and set its position to that of the BallData
        foreach (BallData ballData in data) {

            if (!ballData.IsThrown) 
                continue;

            Ball ball = InitializeBall();
            ball.transform.position = ballData.Position;
            ball.rigidbody.isKinematic = false;
            ball.collider.enabled = true;

            balls.Add(ball);
        }
    }

    /// <summary>
    /// Will save the game by serializing all game data and making a request to the server. The name of the save file will be pulled from the save game menu.
    /// </summary>
    public void Save() {

        // Create an array containing the BallData from all balls in the scene
        BallData[] data = balls.Select(ball => ball.BallData).ToArray();

        // Serialize the BallData array to a JSON string.
        string json = JsonConvert.SerializeObject(data, Formatting.Indented); // We use the Formatting.Indented just for pretty and readable JSON files

        // Make a call to our back end manager, who will do all the saving for us.
        backendManager.SaveGame(saveMenu.SaveName, json);
    }

    private void OnGameFinished() {
        saveMenu.enabled = false;
        nameMenu.enabled = true;

        nameMenu.OnCancel += delegate {
            ResetGame();
        };

        nameMenu.OnNameEntered += delegate(string name) {
            ResetGame();
        };
    }

    private void RemoveBalls() {
        foreach (Ball ball in balls) {
            Destroy(ball.gameObject);
        }

        balls.Clear();
    }


    public void ResetGame() {
        RemoveBalls();

        Score = 0;
        Turn = 0;

        saveMenu.enabled = true;
        nameMenu.enabled = false;
    }
}
