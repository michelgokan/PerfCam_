using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    public Rigidbody rb;
    int fire1Input;
    int fire2Input;

    float zRotation;

    [Header("Tilt Settings")]
    public float tiltAngle = 10f; // Angle to tilt per input


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        fire1Input = Input.GetButton("Fire1") ? 1 : 0;
        fire2Input = Input.GetButton("Fire2") ? 1 : 0;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
        yRotation = yRotation + mouseX;
        xRotation = xRotation - mouseY;
        xRotation = Mathf.Clamp (xRotation, -90f, 90f);
        
    
        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);

        orientation.rotation  = Quaternion.Euler(0, yRotation, 0);

        // Debug.Log("xRotation = " + xRotation + ", transform.rotation = " + transform.rotation);
        // Debug.Log("yRotation = " + yRotation);
        
        //Tilt();
    }

     private void Tilt()
    {   
        Vector3 tiltRight = Vector3.forward * tiltAngle * fire1Input;
        Vector3 tiltLeft = Vector3.forward * -tiltAngle * fire2Input;

        if(fire1Input == 1){
            float rot = tiltAngle * Time.deltaTime;
            // rb.AddTorque(tiltRight, ForceMode.Force);
            // transform.Rotate(Vector3.forward, rot);
            // Debug.Log("rotating left " + rb + " " + tiltAngle * Time.deltaTime);
            // xRotation = Mathf.Clamp (xRotation, -90f, 90f);

            zRotation += rot;
            // Debug.Log("rotating left " + rb + " " + tiltRight);
        }
        else if (fire2Input == 1) {
            float rot = -tiltAngle * Time.deltaTime;
            // rb.AddTorque(tiltLeft, ForceMode.Force);
            // transform.Rotate(Vector3.forward, rot);
            // Debug.Log("rotating right " + rb + " " + tiltAngle * Time.deltaTime);
            zRotation += rot;
            // Debug.Log("rotating left " + rb + " " + tiltLeft);
        }
        else {
            // rb.angularVelocity = Vector3.zero;
            // zRotation = 0;
            // zRotation = 0;
        }
    }
}
