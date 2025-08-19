using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Gyzmo : MonoBehaviour {

    public float Radius = 1;
    public GameObject TargetCircle;
    public GameObject lupe;

    void Update()
    {
        transform.position = Camera.main.ScreenToWorldPoint(
                 new Vector3(Input.mousePosition.x,
                 Input.mousePosition.y,
                 1f));

        lupe.transform.position = transform.position;

        TargetCircle.GetComponent<Renderer>().material.SetFloat("_Radius", Radius / 2);
        TargetCircle.GetComponent<Renderer>().material.SetFloat("_SourceX", transform.position.x);
        TargetCircle.GetComponent<Renderer>().material.SetFloat("_SourceY", transform.position.y);

    }
}
