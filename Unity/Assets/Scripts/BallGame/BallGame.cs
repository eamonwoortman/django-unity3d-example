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
    private Ball currentBall;

    [SerializeField]
    private Transform dartStartPosition;

    [SerializeField]
    private Transform crosshair;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private SavegameMenu menu;

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

    public void ResetGame()
    {
        foreach (Ball ball in balls) {
            Destroy(ball.gameObject);
        }

        Score = 0;
        Turn = 0;
    }

    private void Start() {
        menu.OnSaveButtonPressed += delegate {
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

        if (Input.GetMouseButtonUp(0) && !menu.IsMouseOver() && Turn < MAX_TURNS) {
            FireCurrentBall();
            Turn++;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 999, groundLayer)){
            if(hit.collider.tag == "Board"){
                crosshair.position = hit.point + new Vector3(0, 0.01f, 0);
            }
        }
	}

    private void FireCurrentBall()
    {
        Vector3 target = crosshair.position - currentBall.transform.position;
        target.y = 0;

        currentBall.rigidbody.isKinematic = false;
        currentBall.rigidbody.AddForce(target * 80);
        currentBall.collider.enabled = true;
        currentBall.BallData.IsThrown = true;

        balls.Add(currentBall);

        currentBall = InitializeBall();
    }

    private Ball InitializeBall() {
        GameObject newDartObject = Instantiate(currentBall.gameObject, dartStartPosition.position, dartStartPosition.rotation) as GameObject;
        Ball ball = newDartObject.GetComponent<Ball>();
        ball.rigidbody.isKinematic = true;
        ball.collider.enabled = false;

        return ball;
    }

    public void Load(string json) {
        BallData[] data = JsonConvert.DeserializeObject<BallData[]>(json);

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

    public void Save() {
        BallData[] data = balls.Select(ball => ball.BallData).ToArray();
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        backendManager.PerformRequest("score", null, OnRequestDone); 
    }

    private void OnRequestDone(ResponseType responseType, JToken jsonResponse, string callee)
    {
        Debug.Log(jsonResponse);
    }
}
