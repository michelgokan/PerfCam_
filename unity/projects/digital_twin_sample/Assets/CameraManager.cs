using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using System.IO;
using System;
using UnityEngine.Video; // Add this line to use VideoPlayer


public class CameraManager : MonoBehaviour { 
    // List of camera pairs, each containing a digital twin camera and its corresponding real camera data 
    public List<CameraPair> cameraPairs;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the cameraPairs list and CSV data containers
        foreach (CameraPair cameraPair in cameraPairs)
        {
            cameraPair.Initialize(); // Initialize the VideoPlayer
        }
        Debug.Log("Started CameraManager");
    }

    // Method to add a new camera pair
    public void AddCameraPair(Camera digitalTwinCamera, string realCameraName, string realCameraFilePath, string boundingBoxesCSV, string edgeCounterCSV)
    {
        RealCameraData realCameraData = new RealCameraData(realCameraName, realCameraFilePath, boundingBoxesCSV, edgeCounterCSV);
        CameraPair newPair = new CameraPair(digitalTwinCamera, realCameraData);
        newPair.Initialize(); // Ensure the VideoPlayer is initialized
        cameraPairs.Add(newPair);
    }

    // Class to store information about real cameras
    [System.Serializable]
    public class RealCameraData
    {
        public string cameraName; // Optional: Name of the real camera
        public string filePath;   // File path to the MP4 file representing the real camera footage
        public string boundingBoxesCSV; // File path to the bounding boxes CSV file
        public string edgeCounterCSV; // File path to the edge counter CSV file
        public BoundingBoxToConsider[] boundingBoxesToConsider;

        // Constructor
        public RealCameraData(string name, string path, string boundingBoxesCSV, string edgeCounterCSV)
        {
            cameraName = name;
            filePath = path;
            this.boundingBoxesCSV = boundingBoxesCSV;
            this.edgeCounterCSV = edgeCounterCSV;
        }

        [System.Serializable]
        public class BoundingBoxToConsider
        {
            public int imageStartXToConsider = 0;
            public int imageStartYToConsider = 0;
            public int imageEndXToConsider = int.MaxValue;
            public int imageEndYToConsider = int.MaxValue;
        }
    }

    // Class to represent a pair of digital twin camera and real camera data
    [System.Serializable]
    public class CameraPair
    {
        public Camera digitalTwinCamera;  // The digital twin camera in Unity
        public RealCameraData realCamera; // The real camera data
        // Dictionaries to hold CSV data
        private Dictionary<string, List<BoundingBoxData>> boundingBoxData;
        private List<EdgeCounterData> edgeCounterData;
        private List<string> timestamps;
        private int lastLoadedTimestampIndex = 0;
        private VideoPlayer videoPlayer;
        private float videoWidth;
        private float videoHeight;


        // Property for lastLoadedTimestampIndex
        public int LastLoadedTimestampIndex
        {
            get { return lastLoadedTimestampIndex; }
            set { lastLoadedTimestampIndex = value; }
        }

        public Dictionary<string, List<BoundingBoxData>> getBoundingBoxData()
        {
            return this.boundingBoxData;
        }

        public List<EdgeCounterData> getEdgeCounterData()
        {
            return this.edgeCounterData;
        }

        public CameraPair(){
            boundingBoxData = new Dictionary<string, List<BoundingBoxData>>();
            edgeCounterData = new List<EdgeCounterData>();
        }

        // Constructor
        public CameraPair(Camera digitalCamera, RealCameraData realCameraData)
        {
            digitalTwinCamera = digitalCamera;
            realCamera = realCameraData;
            boundingBoxData = new Dictionary<string, List<BoundingBoxData>>();
            edgeCounterData = new List<EdgeCounterData>();

            LoadBoundingBoxData();
            LoadEdgeCounterData();
            // InitializeVideoPlayer();
        }

        private void InitializeVideoPlayer()
        {
            GameObject videoPlayerObject = new GameObject("VideoPlayer");
            videoPlayer = videoPlayerObject.AddComponent<VideoPlayer>();
            videoPlayer.url = realCamera.filePath; // Assuming this is the path to the MP4 file
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }

        public void Initialize()
        {
            Debug.Log("Video Player Initialization...");
            InitializeVideoPlayer();
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            videoWidth = vp.texture.width;
            videoHeight = vp.texture.height;
            Debug.Log($"Video Dimensions: {videoWidth} x {videoHeight}");
        }

        public float GetRealCameraVideoWidth()
        {
            return videoWidth;
        }

        public float GetRealCameraVideoHeight()
        {
            return videoHeight;
        }

        // Method to get bounding boxes for a specific timestamp
        public List<BoundingBoxData> GetBoundingBoxesByTimestamp(string currentTimestamp)
        {
            // Parse the current timestamp into a TimeSpan for comparison
            TimeSpan current = TimeSpan.Parse(currentTimestamp);
            string lastValidTimestamp = null;

            // Debug.Log($"Searching for the last timestamp less than: {currentTimestamp} (last loaded timestamp index = {this.lastLoadedTimestampIndex})" );

            // Iterate through the timestamps starting from lastLoadedTimestampIndex
            for (int i = this.lastLoadedTimestampIndex; this.timestamps != null && i < this.timestamps.Count; i++)
            {
                TimeSpan timestamp = TimeSpan.Parse(timestamps[i]);
                // Debug.Log($"Checking timestamp: {timestamps[i]}");

                // If the timestamp is greater than or equal to the current timestamp, break the loop
                if (timestamp > current)
                {
                    // Debug.Log($"Timestamp {timestamps[i]} is greater than {currentTimestamp}. Breaking the loop.");
                    break;
                }

                // Update last valid timestamp if the current one is less than the current timestamp
                lastValidTimestamp = timestamps[i];
                this.lastLoadedTimestampIndex = i;
                // Debug.Log($"Updated last valid timestamp to: {lastValidTimestamp}");
            }

            // If a valid timestamp was found, return its bounding box data
            if (lastValidTimestamp != null && boundingBoxData.ContainsKey(lastValidTimestamp))
            {
                // Debug.Log($"Returning bounding box data for timestamp: {lastValidTimestamp}");
                return boundingBoxData[lastValidTimestamp];
            }

            // Debug.Log("No valid timestamp found. Returning an empty list.");
            // Return an empty list if no suitable timestamp is found
            return new List<BoundingBoxData>();
        }


        // Method to load bounding box data from a CSV file
        // Method to load bounding box data from a CSV file
        public void LoadBoundingBoxData()
        {
            // Log the current working directory
            Debug.Log("Current Working Directory: " + Environment.CurrentDirectory);

            Debug.Log("Starting parsing " + this.realCamera.boundingBoxesCSV);
            if (!File.Exists(this.realCamera.boundingBoxesCSV))
            {
                Debug.LogError($"File not found: {this.realCamera.boundingBoxesCSV}");
                return;
            }

            var lines = File.ReadAllLines(this.realCamera.boundingBoxesCSV);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                var values = ParseCsvLine(lines[i]);

                // Ensure we have the expected number of elements
                if (values.Length < 4)
                {
                    Debug.LogError("Malformed CSV line: " + lines[i]);
                    continue;
                }

                // Parse the bounding box string to get the coordinates
                // Debug.Log($"Parsing {BoundingBox}");
                string[] coordinates = values[1].Trim('[', ']').Split(',');
                float xMin = float.Parse(coordinates[0]);
                float yMin = float.Parse(coordinates[1]);
                float xMax = float.Parse(coordinates[2]);
                float yMax = float.Parse(coordinates[3]);
                
                bool toConsider = false;

                foreach(var box in this.realCamera.boundingBoxesToConsider)
                {
                    if( xMin >= box.imageStartXToConsider && yMin <= box.imageStartYToConsider && xMax <= box.imageEndXToConsider && yMax >= box.imageEndYToConsider)
                    {
                        toConsider = true;
                        break;
                    }
                }

                // if( xMin >= this.realCamera.imageStartXToConsider && yMin <= this.realCamera.imageStartYToConsider && xMax <= this.realCamera.imageEndXToConsider && yMax >= this.realCamera.imageEndYToConsider)
                if(toConsider)
                {
                    // Debug.Log("Inside for!");
                    var data = new BoundingBoxData
                    {
                        Timestamp = values[0],
                        BoundingBoxStartX = (int)xMin,
                        BoundingBoxStartY = (int)yMin,
                        BoundingBoxEndX = (int)xMax,
                        BoundingBoxEndY = (int)yMax,
                        Confidence = float.Parse(values[2]),
                        ClassName = values[3]
                    };

                    if (!this.boundingBoxData.ContainsKey(data.Timestamp))
                    {
                        this.boundingBoxData[data.Timestamp] = new List<BoundingBoxData>();
                        (this.timestamps ??= new List<string>()).Add(data.Timestamp);
                    }

                    this.boundingBoxData[data.Timestamp].Add(data);
                }
            }

            Debug.Log("Total lines parsed: " + lines.Length.ToString());
        }

        private string[] ParseCsvLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string currentValue = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes; // Toggle state
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentValue.Trim());
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }

            // Add the last value
            if (!string.IsNullOrEmpty(currentValue))
            {
                result.Add(currentValue.Trim());
            }

            return result.ToArray();
        }

        public List<string> getTimestamps()
        {
            return this.timestamps;
        }

        // Method to load edge counter data from a CSV file
        public void LoadEdgeCounterData()
        {
            if (!File.Exists(this.realCamera.edgeCounterCSV))
            {
                Debug.LogError($"File not found: {this.realCamera.edgeCounterCSV}");
                return;
            }

            var lines = File.ReadAllLines(this.realCamera.edgeCounterCSV);
            var dataList = new List<EdgeCounterData>();

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                var values = lines[i].Split(',');
                var data = new EdgeCounterData
                {
                    Timestamp = values[0],
                    CounterName = values[1],
                    CountIn = int.Parse(values[2]),
                    CountOut = int.Parse(values[3])
                };
                dataList.Add(data);
            }

            edgeCounterData = dataList;
        }

        public class BoundingBoxData
        {
            public string Timestamp;
            public int BoundingBoxStartX;
            public int BoundingBoxStartY;
            public int BoundingBoxEndX;
            public int BoundingBoxEndY;
            public float Confidence;
            public string ClassName;

            // Override the ToString method
            public override string ToString()
            {
                return $"{Timestamp},[{BoundingBoxStartX},{BoundingBoxStartY},{BoundingBoxEndX},{BoundingBoxEndY}],{Confidence},{ClassName}";
            }

            public Vector3 GetCenterPositionFromBoundingBox()
            {
                // Calculate the center
                Vector3 center = new Vector3((BoundingBoxStartX + BoundingBoxEndX) / 2, (BoundingBoxStartY + BoundingBoxEndY) / 2, 0);
                return center;
            }
        }

        // Class to store information about edge counter data
        public class EdgeCounterData
        {
            public string Timestamp;
            public string CounterName;
            public int CountIn;
            public int CountOut;

            // Override the ToString method
            public override string ToString()
            {
                return $"{Timestamp},{CounterName},{CountIn},{CountOut}";
            }
        }
    }
}