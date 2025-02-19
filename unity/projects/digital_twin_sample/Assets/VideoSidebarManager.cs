using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using TMPro;

public class VideoSidebarManager : MonoBehaviour
{
    public GameObject videoPlayerPrefab; // Assign your prefab here
    public Transform contentPanel;       // Assign the Content transform of the Scroll View
    public TMP_Text timelineTime;
    private float TimeElapsed = 0f;
    public List<CameraManager.CameraPair> cameraPairs;

    void Start()
    {
        Debug.Log("Starting VideoSidebarManager");
        foreach (var pair in cameraPairs)
        {
            CreateVideoPlayer(pair.realCamera.filePath, pair.realCamera.cameraName);
            pair.LoadBoundingBoxData();
            pair.LoadEdgeCounterData();
            pair.Initialize();
        }
    }

    void Update()
    {
        TimeElapsed += Time.deltaTime;
        updateTimer(TimeElapsed);
    }

    private void CreateVideoPlayer(string filePath, string cameraName)
    {
        // Instantiate the video player prefab
        GameObject videoPlayerGO = Instantiate(videoPlayerPrefab, contentPanel);

        // Set the name or label if needed
        Text label = videoPlayerGO.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = cameraName;
        }

        // Create a unique RenderTexture for this video player
        RenderTexture renderTexture = new RenderTexture(256, 256, 0); // Adjust size as needed

        // Configure the VideoPlayer component
        VideoPlayer videoPlayer = videoPlayerGO.GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.url = filePath;
            videoPlayer.targetTexture = renderTexture; // Assign the unique render texture
            videoPlayer.Play();
        }

        // Assign the Render Texture to the RawImage
        RawImage rawImage = videoPlayerGO.GetComponentInChildren<RawImage>();
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
        }
    }

    void updateTimer(float currentTime)
    {
        float hours = Mathf.FloorToInt(currentTime / 3600);
        float minutes = Mathf.FloorToInt((currentTime % 3600) / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);
        float milliseconds = (currentTime % 1) * 1000;

        timelineTime.text = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", hours, minutes, seconds, milliseconds);
    }
}