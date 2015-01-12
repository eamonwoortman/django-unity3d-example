using UnityEngine;
using System.Collections;
using System;

public class Ball : MonoBehaviour {

    [SerializeField]
    private Cubemap cubemap;

    public BallData BallData {
        get {
            ballData.Position = transform.position;
            return ballData;
        }

        set {
            ballData = value;
            transform.position = value.Position;
        }
    }

    private BallData ballData;

    void Awake(){
        ballData = new BallData();
    }

	void Start () {
        gameObject.name = "Ball";
        Camera.main.RenderToCubemap(cubemap);
	}
}
