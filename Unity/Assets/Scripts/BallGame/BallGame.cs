using UnityEngine;
using System.Collections;

public class BallGame : MonoBehaviour {

    [SerializeField]
    private Ball currentBall;

    [SerializeField]
    private Transform dartStartPosition;

    [SerializeField]
    private Transform crosshair;

    [SerializeField]
    private LayerMask groundLayer;

    public void ResetGame()
    {

    }
	
	void Update () {

        if (Input.GetMouseButtonUp(0))
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

        currentBall.OnHit = null;
        GameObject newDartObject = Instantiate(currentBall.gameObject, dartStartPosition.position, dartStartPosition.rotation) as GameObject;
        currentBall = newDartObject.GetComponent<Ball>();
        currentBall.rigidbody.isKinematic = true;
        currentBall.collider.enabled = false;
    }
}
