using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RouteManager : MonoBehaviour {

    public 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadStats()
    {
        SceneManager.LoadScene("stats");
    }

    public void LoadMain()
    {
        SceneManager.LoadScene("main");
    }
}
