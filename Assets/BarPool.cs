using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarPool : MonoBehaviour
{
    public static List<GameObject> bars;
    public GameObject bar;
    public int pooledMax = 50;
	// Use this for initialization
	void Start () {
		bars = new List<GameObject>();
	    for (int i = 0; i < pooledMax; i++)
	    {
            GameObject g = Instantiate(bar,new Vector3(0,0.0f),Quaternion.identity);
            g.SetActive(false);
            bars.Add(g);
	    }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
