using UnityEngine;
using System.Collections;
using getReal3D;

[AddComponentMenu("getReal3D/Updater/Wand Updater")]

public class getRealWandUpdater
    : getRealUserScript
{
    private Transform m_transform;
    
    void Awake()
    {
        m_transform = transform;
    }
    
    void Update()
    {
        getReal3D.Sensor wandSensor = getWand();
        m_transform.localPosition = wandSensor.position;
        m_transform.localRotation = wandSensor.rotation;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(0,0,0), new Vector3(0.1f, 0.1f, 0.15f));
    }
}
