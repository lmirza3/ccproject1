using UnityEngine;
using System.Collections;

public class BodyMovement : MonoBehaviour {
    
    // Update is called once per frame
    void Update () {
        transform.rotation = Quaternion.identity;
        transform.Rotate(Vector3.up, transform.parent.eulerAngles.y);
    }
}
