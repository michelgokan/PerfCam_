using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{

    CharacterController cha;

    Vector3 move_speed;

    // Start is called before the first frame update
    void Start()
    {
        cha=GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        move_speed=new Vector3(Input.GetAxis("Horizontal"), 2, Input.GetAxis("Vertical"));
        cha.SimpleMove(move_speed);
    }
}
