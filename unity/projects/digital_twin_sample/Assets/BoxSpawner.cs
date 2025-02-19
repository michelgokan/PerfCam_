using UnityEngine;
using UnityEngine.Splines;

public class SplineBoxSpawner : MonoBehaviour
{
    public GameObject boxPrefab; // Assign this in the Unity Inspector to your box prefab
    public SplineContainer splineContainer; // Reference to the SplineContainer component
    public int samplePoints = 100; // Number of samples per spline for accurate snapping
    public float additionalOrientation = 0; // Additional orientation in degrees
    public VideoSidebarManager videoSidebarManager;
    private string currentTimestamp;

    void Start()
    {

    }

    void RemoveAllBoxes()
    {
        foreach (var box in GameObject.FindGameObjectsWithTag("Box"))
        {
            Destroy(box);
        }
    }

    void Update()
    {
        this.currentTimestamp = videoSidebarManager.timelineTime.text;
        
        // Remove all previous boxes
        RemoveAllBoxes();

        foreach (var cameraPair in videoSidebarManager.cameraPairs){
            // Get bounding boxes for the current timestamp
            var boundingBoxes = cameraPair.GetBoundingBoxesByTimestamp(currentTimestamp);

            // Process each bounding box
            foreach (var boundingBox in boundingBoxes)
            {
                Vector3 centerPosition = boundingBox.GetCenterPositionFromBoundingBox();
                
                // Convert the center position from video frame to digitalTwinCamera frame
                Vector3 convertedPosition = ConvertPosition(cameraPair, centerPosition);

                SpawnBoxOnSpline(cameraPair, convertedPosition);
            }
        }
    }

    Vector3 ConvertPosition(CameraManager.CameraPair cameraPair, Vector3 originalPosition)
    {
        Camera digitalTwinCamera = cameraPair.digitalTwinCamera;

        // Assuming both have the same aspect ratio and alignment
        float screenWidth = digitalTwinCamera.pixelWidth;
        float screenHeight = digitalTwinCamera.pixelHeight;
        float videoWidth = cameraPair.GetRealCameraVideoWidth();
        float videoHeight = cameraPair.GetRealCameraVideoHeight();

        // Scale the position
        float scaledX = originalPosition.x / videoWidth * screenWidth;
        // Invert the Y-coordinate to account for the different origins 
        // (you know, since the MP4 (0,0) coordinates start from top left and Unity's (0,0) start from bottom left)
        float scaledY = (videoHeight - originalPosition.y) / videoHeight * screenHeight;

        return new Vector3(scaledX, scaledY, originalPosition.z);
    }

    void SpawnBoxOnSpline(CameraManager.CameraPair cameraPair, Vector3 position)
    {
        // Validate the position before using it
        if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z) ||
            float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
        {
            // Debug.LogError("Invalid screen position: " + position);
            return;
        }
        
        Ray ray = cameraPair.digitalTwinCamera.ScreenPointToRay(position);

        (Vector3 closestPoint, float closestT) = FindClosestPointInView(cameraPair, ray);

        // Instantiate the box at the closest point with the correct orientation
        if (closestPoint != Vector3.zero)
        {
            // Get the tangent at the position on the spline
            Vector3 tangent = splineContainer.EvaluateTangent(closestT);

            // Calculate the initial rotation aligned with the tangent
            Quaternion initialRotation = Quaternion.LookRotation(tangent, Vector3.up);

            // Create an additional rotation of 90 degrees plus the additionalOrientation
            Quaternion additionalRotation = Quaternion.Euler(0, 90 + additionalOrientation, 0);

            // Combine the rotations
            Quaternion finalRotation = initialRotation * additionalRotation;

            Instantiate(boxPrefab, closestPoint, finalRotation);
        }
        else
        {
            // Debug.Log("No points on the spline are visible in the camera's view and near the mouse click.");
        }
    }

    (Vector3, float) FindClosestPointInView(CameraManager.CameraPair cameraPair, Ray ray)
    {
        float closestDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;
        float closestT = 0f;

        Camera camera = cameraPair.digitalTwinCamera; // Use digital twin camera

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

        for (int i = 0; i <= samplePoints; i++)
        {
            float t = i / (float)samplePoints;
            Vector3 pointOnSpline = splineContainer.EvaluatePosition(t);

            if (GeometryUtility.TestPlanesAABB(frustumPlanes, new Bounds(pointOnSpline, Vector3.one * 0.1f)))
            {
                float distanceToRay = Vector3.Cross(ray.direction, pointOnSpline - ray.origin).magnitude;

                if (distanceToRay < closestDistance)
                {
                    closestDistance = distanceToRay;
                    closestPoint = pointOnSpline;
                    closestT = t;
                }
            }
        }

        return (closestPoint, closestT);
    }
}