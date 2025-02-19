using UnityEngine;
using UnityEngine.UI;

public class ToggleRawImage : MonoBehaviour
{
    private RawImage rawImage;

    void Start()
    {
        // Get the RawImage component attached to the GameObject
        rawImage = GetComponent<RawImage>();

        // Check if the RawImage component is found
        if (rawImage == null)
        {
            Debug.LogError("No RawImage component found on this GameObject.");
        }
    }

    void Update()
    {
        // Check if the 'M' key is pressed
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Toggle the enabled state of the RawImage component
            if (rawImage != null)
            {
                rawImage.enabled = !rawImage.enabled;
            }
        }
    }
}