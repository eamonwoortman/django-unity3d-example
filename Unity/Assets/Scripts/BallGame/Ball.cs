using UnityEngine;
using System.Collections;
using System;

public class Ball : MonoBehaviour {

    public Action OnHit;

	// Use this for initialization
	void Start () {
        gameObject.name = "Ball";
	}
	
	// Update is called once per frame
	void Update () {
	
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
