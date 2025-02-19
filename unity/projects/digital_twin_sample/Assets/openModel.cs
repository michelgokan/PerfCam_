using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class openModel : MonoBehaviour
{
    public Button m_YourFirstButton;
    public GameObject model;
    public GameObject phantom;
    public int panelNumber;

    void Start()
    {
        m_YourFirstButton.onClick.AddListener(TaskOnClick);

        model.SetActive(false);
        phantom.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("key 0 was pressed - all panels are opened");
            activateObj();
        }

        switch (panelNumber)
        {
            case 1:
                if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
                {
                    Debug.Log("key 1 was pressed");
                    activateObj();
                }
                break;

            case 2:
                if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
                {
                    Debug.Log("key 2 was pressed");
                    activateObj();
                }
                break;

            case 3:
                if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
                {
                    Debug.Log("key 3 was pressed");
                    activateObj();
                }
                break;

            case 4:
                if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
                {
                    Debug.Log("key 4 was pressed");
                    activateObj();
                }
                break;

            case 5:
                if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
                {
                    Debug.Log("key 5 was pressed");
                    activateObj();
                }
                break;

            case 6:
                if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
                {
                    Debug.Log("key 6 was pressed");
                    activateObj();
                }
                break;

            case 7:
                if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
                {
                    Debug.Log("key 7 was pressed");
                    activateObj();
                }
                break;

            case 0:
                Debug.Log("key 0 was pressed");
                activateObj();

            break;
        }       
    }

    void TaskOnClick()
    {
        //Output this to console when Button1 or Button3 is clicked
        Debug.Log("You have clicked the button!");
        activateObj();
    }

    void activateObj()
    {
        model.SetActive(!model.active);
        phantom.SetActive(!phantom.active);
    }
}
