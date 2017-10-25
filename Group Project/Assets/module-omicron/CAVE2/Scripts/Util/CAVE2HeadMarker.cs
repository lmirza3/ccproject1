﻿using UnityEngine;
using System.Collections;

public class CAVE2HeadMarker : MonoBehaviour {

    [SerializeField]
    int headID = 1;

    LineRenderer headToGroundLine;
    LineRenderer forwardLine;

    [SerializeField]
    bool showLine = false;

    [SerializeField]
    Material lineMaterial;

	// Use this for initialization
	void Start () {
        if (!CAVE2.OnCAVE2Display())
        {
            headToGroundLine = gameObject.AddComponent<LineRenderer>();
            headToGroundLine.startWidth = 0.02f;
            headToGroundLine.endWidth = 0.02f;
            headToGroundLine.material = lineMaterial;

        
            GameObject forwardReference = new GameObject("Head-ForwardRef");
            forwardReference.transform.parent = transform;
            forwardLine = forwardReference.AddComponent<LineRenderer>();

            forwardLine.startWidth = 0.02f;
            forwardLine.endWidth = 0.02f;
            forwardLine.material = lineMaterial;
        }

        CAVE2.RegisterHeadObject(headID, gameObject);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        transform.localPosition = CAVE2.GetHeadPosition(1);
        transform.localRotation = CAVE2.GetHeadRotation(1);

        if (!CAVE2.OnCAVE2Display())
        {
            forwardLine.enabled = showLine;

            headToGroundLine.SetPosition(0, new Vector3(transform.position.x, transform.parent.position.y, transform.position.z));
            headToGroundLine.SetPosition(1, transform.position);

        
            forwardLine.SetPosition(0, transform.position);
            forwardLine.SetPosition(1, transform.position + transform.forward * 0.2f);
        }
    }
}
