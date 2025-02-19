using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hideWhileFar : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform camera, origin;
    private Vector2 originXZ;
    private float distanceFromCamera;

    void Start()
    {
        originXZ = new Vector2 (origin.position.x,origin.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromCamera = Vector2.Distance(new Vector2(camera.position.x,camera.position.z),originXZ);

        if (distanceFromCamera>6)
        {
            origin.gameObject.SetActive(false);
            Debug.Log("too far");
        }
        else
        {
            origin.gameObject.SetActive(true);
            Debug.Log("close enough");
        }
    }
}
