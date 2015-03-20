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

using System;
using UnityEngine;

public class Ball : MonoBehaviour {
    public BallData Data;

    [SerializeField]
    private Cubemap cubemap;
    private float nextChangeTime;
    private Color startColor;
    private Color targetColor;
    
    private void Awake(){
        startColor = targetColor = GetComponent<Renderer>().material.GetColor("_SpecColor");

        //let's not render the GUI on the cubemap
		if(cubemap != null) {
			Camera.main.GetComponent<GUILayer>().enabled = false;
			Camera.main.RenderToCubemap(cubemap);
			Camera.main.GetComponent<GUILayer>().enabled = true;
		}
        gameObject.name = "Ball";
        Data = new BallData();
    }

    private void Update() {
        Color currColor = GetComponent<Renderer>().material.GetColor("_SpecColor");
        Color newColor = Color.Lerp(currColor, targetColor, Time.deltaTime * 2f);
        GetComponent<Renderer>().material.SetColor("_SpecColor", newColor);

        if (Time.time > nextChangeTime) {
            if (targetColor == startColor) {
                targetColor = new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
            } else {
                targetColor = startColor;
            }

            nextChangeTime = Time.time + 1.5f;
        }
        Data.Position = transform.position;
    }
}
