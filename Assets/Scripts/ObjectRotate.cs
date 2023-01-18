using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotate : MonoBehaviour
{
    int spinSpeed = 30;

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = new Vector3(0, spinSpeed * Time.deltaTime, 0);
        // transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }
}
