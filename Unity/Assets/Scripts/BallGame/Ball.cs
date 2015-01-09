using UnityEngine;
using System.Collections;
using System;

public class Ball : MonoBehaviour {

    public BallData BallData { get; private set; }
    public Action OnHit;

    void Awake(){
        BallData = new BallData();
    }

	void Start () {
        gameObject.name = "Ball";
	}
	
	void Update () {
        BallData.Position = transform.position;
	}

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Board")
        {
            if (OnHit != null)
            {
                OnHit.Invoke();
                OnHit = null;
            }
        }
    }
}
