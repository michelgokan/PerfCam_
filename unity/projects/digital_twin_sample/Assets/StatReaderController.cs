using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatReaderController : MonoBehaviour
{
    private GameObject cube;

    // Start is called before the first frame update
    void Start()
    {
        // Create a new GameObject
        this.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        // Set the position of the cube relative to the parent
        this.cube.transform.localPosition = new Vector3(1, 1, 1);

        // Optionally, set the size of the cube
        this.cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.2f); // Adjust the scale as needed

        // Set this GameObject as the parent of the cube
        this.cube.transform.parent = this.transform;
    }
}
