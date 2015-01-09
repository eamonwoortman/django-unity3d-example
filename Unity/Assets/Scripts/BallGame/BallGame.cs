using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;

public class BallGame : MonoBehaviour {

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

    public void ResetGame()
    {
    }
	
	void Update () {

        if (Input.GetMouseButtonUp(0) && !menu.IsMouseOver())
        {
            FireCurrentBall();
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 999, groundLayer))
        {
            if(hit.collider.tag == "Board")
            {
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

        currentBall = InitializeBall();

        Save();
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
            Ball ball = InitializeBall();
            ball.transform.position = ballData.Position;
            ball.rigidbody.isKinematic = false;
            ball.collider.enabled = true;
        }
    }

    public void Save() {
        BallData[] data = FindObjectsOfType<Ball>().Select(ball => ball.BallData).ToArray();
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);


    }
}
