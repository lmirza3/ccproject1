using UnityEngine;
using System.Collections;
using getReal3D;

[AddComponentMenu("getReal3D/Updater/Head Updater")]

public class getRealHeadUpdater
    : getRealUserScript
{
    private Transform m_transform;
    
    void Awake()
    {
        m_transform = transform;
    }

    void Update()
    {
        getReal3D.Sensor headSensor = getHead();
        m_transform.localPosition = headSensor.position;
        m_transform.localRotation = headSensor.rotation;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(0.1f, 0.2f, 0.1f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(0,0,0), 1);
    }
}
