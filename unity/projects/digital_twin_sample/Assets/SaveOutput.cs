using System.Collections;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

public class SaveOutput : MonoBehaviour
{
    private Camera targetCamera;
    private RenderTexture renderTexture;
    private string outputDirectory = "CapturedFrames";
    private int frameRate = 10;
    private int frameCount = 1800; // Number of frames to capture

    // Full path to the FFmpeg executable
    private string ffmpegPath = "/opt/homebrew/bin/ffmpeg"; // Update this with the full path from `which ffmpeg`

    private void Start()
    {
        // Set the target camera to the camera this script is attached to
        targetCamera = GetComponent<Camera>();

        outputDirectory += gameObject.name;

        // Set the render texture to the target texture of the target camera
        renderTexture = targetCamera.targetTexture;

        // Ensure the output directory exists
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // Debugging: Check if the FFmpeg path is correct
        if (!File.Exists(ffmpegPath))
        {
            UnityEngine.Debug.LogError($"FFmpeg not found at {ffmpegPath}");
        }
        else
        {
            UnityEngine.Debug.Log($"FFmpeg found at {ffmpegPath}");
        }

        // Start capturing frames
        StartCoroutine(CaptureFrames());
    }

    private IEnumerator CaptureFrames()
    {
        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForEndOfFrame();
            CaptureFrame(i);
        }

        // After capturing frames, create a video from the images
        CreateVideo(Path.Combine(outputDirectory, "frame_%04d.png"), Path.Combine(outputDirectory, "output.mp4"));
    }

    private void CaptureFrame(int frameIndex)
    {
        RenderTexture.active = renderTexture;
        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = null;

        byte[] bytes = image.EncodeToPNG();
        string filename = Path.Combine(outputDirectory, string.Format("frame_{0:0000}.png", frameIndex));
        File.WriteAllBytes(filename, bytes);

        Destroy(image);
    }

    private void CreateVideo(string inputPattern, string outputFileName)
    {
        // Verify FFmpeg path
        if (!File.Exists(ffmpegPath))
        {
            UnityEngine.Debug.LogError($"FFmpeg not found at {ffmpegPath}");
            return;
        }

        // Ensure full path for the input pattern
        string fullInputPattern = Path.GetFullPath(inputPattern);
        string fullOutputFileName = Path.GetFullPath(outputFileName);

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = ffmpegPath; // Use the full path to FFmpeg
        startInfo.Arguments = $"-r {frameRate} -f image2 -s {renderTexture.width}x{renderTexture.height} -i \"{fullInputPattern}\" -vcodec libx264 -crf 25 -pix_fmt yuv420p \"{fullOutputFileName}\" -y";
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardError = true; // Capture error output
        startInfo.RedirectStandardOutput = true; // Capture standard output
        startInfo.WorkingDirectory = Path.GetFullPath(outputDirectory); // Set working directory

        UnityEngine.Debug.Log(startInfo.FileName + " " + startInfo.Arguments);

        // Explicitly set the environment variable for PATH
        startInfo.EnvironmentVariables["PATH"] = "/opt/homebrew/bin:" + startInfo.EnvironmentVariables["PATH"];

        using (Process proc = new Process())
        {
            proc.StartInfo = startInfo;
            proc.Start();

            // Capture output (for debugging)
            string output = proc.StandardOutput.ReadToEnd();
            // string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            UnityEngine.Debug.Log("FFmpeg output: " + output);
        }
    }
}
