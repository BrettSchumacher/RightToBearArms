using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public bool stationary = false;
    public float scale = 10f;

    Transform cam;
    Vector2 startingCam;
    Vector2 startingPos;

    float fac;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        startingCam = cam.position;
        startingPos = transform.position;

        float z = transform.localPosition.z / scale;
        fac = 1f - Mathf.Sqrt(z * z + 1f) + z;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (stationary)
        {
            transform.position = startingPos + (Vector2)cam.position - startingCam;
            return;
        }

        float dx = cam.position.x - startingCam.x;
        transform.position = new Vector3(startingPos.x + fac * dx, transform.position.y, transform.position.z);
    }
}
