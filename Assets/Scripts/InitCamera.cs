using UnityEngine;
using System.Collections;

public class InitCamera : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
	
	}
	
	void Awake()
	{
		Screen.SetResolution(500, 500, false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
