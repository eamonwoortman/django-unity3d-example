using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class BallGame : BaseGame {

    public const int MAX_TURNS = 999;

    [SerializeField]
    private Ball defaultBall;

    [SerializeField]
    private Transform dartStartPosition;

    [SerializeField]
    private Transform crosshair;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private EnterNameMenu nameMenu;

    [SerializeField]
    private GUIText turnText;

    [SerializeField]
    private GUIText scoreText;

    [SerializeField]
    private Transform targetBall;

    public GameData Data;

    private List<Ball> balls;

    protected override void Start() {
        base.Start();

        nameMenu.enabled = false;

        Data = new GameData();
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

        if (Input.GetMouseButtonDown(0) && !IsMouseOverMenu() && Data.Turn < MAX_TURNS) {
            FireCurrentBall();
            Data.Turn++;

            if (Data.Turn == MAX_TURNS)
                OnGameFinished();
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 100, groundLayer)){
            if(hit.collider.tag == "Board"){
                crosshair.position = hit.point + new Vector3(0, 0.01f, 0);
            }
        }
	}

    protected override bool IsMouseOverMenu() {
        return base.IsMouseOverMenu() || nameMenu.IsMouseOver();
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
    public override void Load(string jsonString) {

        ResetGame();

        JObject json = JObject.Parse(jsonString);

        Data = JsonConvert.DeserializeObject<GameData>(json.GetValue("game").ToString());

        // Deserialize the JSON string we got from the server into a array of BallData
        BallData[] data = JsonConvert.DeserializeObject<BallData[]>(json.GetValue("balls").ToString());

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

    protected override string Serialize() {

        // Create an array containing the BallData from all balls in the scene
        BallData[] ballData = balls.Select(ball => ball.BallData).ToArray();

        JObject jsonObject = new JObject();
        jsonObject.Add("game", JsonConvert.SerializeObject(Data));
        jsonObject.Add("balls", JsonConvert.SerializeObject(ballData));

        return jsonObject.ToString();
    }

    private void OnGameFinished() {
        HideSaveMenu();
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

        Data.Score = 0;
        Data.Turn = 0;

        ShowSaveMenu();
        nameMenu.enabled = false;
    }
}
