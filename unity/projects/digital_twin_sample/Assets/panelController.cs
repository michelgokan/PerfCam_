using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

// Example usage:
// 1) Attach this script to a GameObject in your scene
// 2) Populate "panelValues" array in the Inspector with
//    (a) Each UI Text field
//    (b) CSV file paths (if available)
// 3) The 'timelineTime' text object is assumed to be updated in real-time
//    with a string in the format "HH:MM:SS.xxx" (or "HH:MM:SS")

public class panelController : MonoBehaviour
{
    public PanelElementData[] panelValues;
    public TMP_Text timelineTime;

    // We’ll store CSV data for each PanelElementData in these structures
    // (You could also store them in dictionaries keyed by the PanelElementData, etc.)
    private Dictionary<PanelElementData, List<AvailabilityRow>> availabilityData 
        = new Dictionary<PanelElementData, List<AvailabilityRow>>();

    private Dictionary<PanelElementData, List<OeeMinuteRow>> oeeData
        = new Dictionary<PanelElementData, List<OeeMinuteRow>>();
    
    private Dictionary<PanelElementData, List<StopRow>> stopData
        = new Dictionary<PanelElementData, List<StopRow>>();

    // For bad products from multiple CSVs:
    private Dictionary<PanelElementData, List<BadProductRow>> tripleDoublePickupData 
        = new Dictionary<PanelElementData, List<BadProductRow>>();
    private Dictionary<PanelElementData, List<BadProductRow>> triplePickupData 
        = new Dictionary<PanelElementData, List<BadProductRow>>();
    private Dictionary<PanelElementData, List<BadProductRow>> pickupData 
        = new Dictionary<PanelElementData, List<BadProductRow>>();
    private Dictionary<PanelElementData, List<BadProductRow>> doubleHandedPickupData 
        = new Dictionary<PanelElementData, List<BadProductRow>>();
    private Dictionary<PanelElementData, List<BadProductRow>> doublePickupData 
        = new Dictionary<PanelElementData, List<BadProductRow>>();

    private System.Random rnd = new System.Random();

    // -------------------- CSV Row Classes --------------------
    [Serializable]
    public class AvailabilityRow
    {
        // Matching the format: Start Timestamp,End Timestamp,Availability (%),Stop Time
        // Example: 0:00:00,0:01:00,95.5,5
        public TimeSpan startTime;
        public TimeSpan endTime;
        public float availabilityPercent;
        public float stopTime;

        public AvailabilityRow(string startTimestamp, string endTimestamp, string availabilityStr, string stopTimeStr)
        {
            startTime = ParseTime(startTimestamp);
            endTime = ParseTime(endTimestamp);
            float.TryParse(availabilityStr, out availabilityPercent);
            float.TryParse(stopTimeStr, out this.stopTime);
        }
    }

    [Serializable]
    public class OeeMinuteRow
    {
        // Matching the format:
        // minute,availability,performance,quality,oee,availability_precent,stop_time_sec,
        // defective_count,good_count,total_count,run_time_sec,edge_1_count,edge_2_count,
        // gt_edge_1,gt_edge_2
        //
        // "Every row represents one minute."
        // We'll parse the first column as an integer minute
        // Then we’ll store whatever fields we need.
        public int minute;
        public float availability;
        public float performance;
        public float quality;
        public float oee;
        public float availability_percent;
        public float stop_time_sec;
        public int defective_count;
        public int good_count;
        public int total_count;
        // ... you could parse the rest as needed.

        public OeeMinuteRow(string[] cols)
        {
            // Assuming each line is something like:
            // "0,0.95,0.90,1.0,0.85,95,10,2,10,12,50,5,7,8,9"
            // Adjust indexing to your actual CSV structure
            int.TryParse(cols[0], out minute);
            float.TryParse(cols[1], out availability);
            float.TryParse(cols[2], out performance);
            float.TryParse(cols[3], out quality);
            float.TryParse(cols[4], out oee);
            float.TryParse(cols[5], out availability_percent);
            float.TryParse(cols[6], out stop_time_sec);
            int.TryParse(cols[7], out defective_count);
            int.TryParse(cols[8], out good_count);
            int.TryParse(cols[9], out total_count);
            // parse the rest as needed
        }
    }

    [Serializable]
    public class StopRow
    {
        // Format: "ID,Start Timestamp,End Timestamp"
        // We'll ignore the ID except for reading
        // We'll store the start and end times as TimeSpan.
        public TimeSpan startTime;
        public TimeSpan endTime;

        public StopRow(string idStr, string startTimestamp, string endTimestamp)
        {
            startTime = ParseTime(startTimestamp);
            endTime = ParseTime(endTimestamp);
        }
    }

    [Serializable]
    public class BadProductRow
    {
        // Format: "ID,Start Timestamp,End Timestamp,Occurrences Count"
        public TimeSpan startTime;
        public TimeSpan endTime;
        public int occurrences;

        public BadProductRow(string[] cols)
        {
            // Example of CSV row: "1,0:02:00,0:03:00,5"
            // Adjust parsing as needed
            // ID = cols[0], Start Timestamp = cols[1], End Timestamp = cols[2], Occurrences = cols[3]
            startTime = ParseTime(cols[1]);
            endTime  = ParseTime(cols[2]);
            int.TryParse(cols[3], out occurrences);
        }
    }

    // ---------------------------------------------------------

    void Start()
    {
        // Load data from CSVs for each PanelElementData
        foreach (var panel in panelValues)
        {
            // 1) availabilityCSVPerMinuteCSVPath
            if (!string.IsNullOrEmpty(panel.availabilityCSVPerMinuteCSVPath))
            {
                var list = LoadAvailabilityData(panel.availabilityCSVPerMinuteCSVPath);
                availabilityData[panel] = list;
            }
            else
            {
                availabilityData[panel] = new List<AvailabilityRow>();
            }

            // 2) OEE per minute CSV path (throughput, good/bad products, etc.)
            if (!string.IsNullOrEmpty(panel.OEEPerMinuteCSVPath))
            {
                var list = LoadOeeMinuteData(panel.OEEPerMinuteCSVPath);
                oeeData[panel] = list;
            }
            else
            {
                oeeData[panel] = new List<OeeMinuteRow>();
            }

            // 3) stopsCSVPath
            if (!string.IsNullOrEmpty(panel.stopsCSVPath))
            {
                var list = LoadStopsData(panel.stopsCSVPath);
                stopData[panel] = list;
            }
            else
            {
                stopData[panel] = new List<StopRow>();
            }

            // 4) Bad product occurrences
            //    Each occurrence = 3 bad products for tripleDoublePickupCSVPath and triplePickupCSVPath,
            //    = 1 for pickupCSVPath, 2 for doubleHandedPickupCSVPath/doublePickupCSVPath.
            //    We'll load them similarly.

            if (!string.IsNullOrEmpty(panel.tripleDoublePickupCSVPath))
                tripleDoublePickupData[panel] = LoadBadProductData(panel.tripleDoublePickupCSVPath);
            else
                tripleDoublePickupData[panel] = new List<BadProductRow>();

            if (!string.IsNullOrEmpty(panel.triplePickupCSVPath))
                triplePickupData[panel] = LoadBadProductData(panel.triplePickupCSVPath);
            else
                triplePickupData[panel] = new List<BadProductRow>();

            if (!string.IsNullOrEmpty(panel.pickupCSVPath))
                pickupData[panel] = LoadBadProductData(panel.pickupCSVPath);
            else
                pickupData[panel] = new List<BadProductRow>();

            if (!string.IsNullOrEmpty(panel.doubleHandedPickupCSVPath))
                doubleHandedPickupData[panel] = LoadBadProductData(panel.doubleHandedPickupCSVPath);
            else
                doubleHandedPickupData[panel] = new List<BadProductRow>();

            if (!string.IsNullOrEmpty(panel.doublePickupCSVPath))
                doublePickupData[panel] = LoadBadProductData(panel.doublePickupCSVPath);
            else
                doublePickupData[panel] = new List<BadProductRow>();
        }

        // Now invoke updatePanel repeatedly
        InvokeRepeating(nameof(updatePanel), 0f, 0.1f);
    }

    void Update()
    {
        // If you'd rather do everything from Update, you can do so.
        // But here we use InvokeRepeating for demonstration.
    }

    void updatePanel()
    {
        // 1) Convert current timelineTime.text into a TimeSpan
        //    timelineTime.text is in format XX:XX:XX.XXX or XX:XX:XX
        //    We'll parse using ParseTime (defined below).
        TimeSpan currentTime = ParseTime(timelineTime.text);

        // 2) Figure out how many "whole minutes" have elapsed
        //    For example, if timelineTime is 00:02:15.123, minuteIndex = 2
        int minuteIndex = currentTime.Hours * 60 + currentTime.Minutes;

        foreach (PanelElementData panel in panelValues)
        {
            // For convenience, we’ll get references to the loaded data
            var availData = availabilityData[panel];
            var stops     = stopData[panel];
            var oeeRows   = oeeData[panel];

            // ---------- STATUS ----------
            // If the current timestamp is within any stop interval, set status to false;
            // otherwise, true.
            if (panel.ElementValueText.name.IndexOf("STATUS", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                bool isStopped = false;
                foreach (var stop in stops)
                {
                    if (currentTime >= stop.startTime && currentTime < stop.endTime)
                    {
                        isStopped = true;
                        break;
                    }
                }
                panel.setStatus(!isStopped);
            }

            // ---------- OEE PERCENT ----------
            if (panel.ElementValueText.name.IndexOf("OEEPercent", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // We can get it from the OEE CSV (oee_per_minute.csv).
                // We'll assume the row with "minute == minuteIndex" is the correct row
                // If out of range, handle gracefully.
                OeeMinuteRow row = GetOeeRowByMinute(oeeRows, minuteIndex);
                if (row != null)
                {
                    // For example, display row.oee as a percent
                    panel.ElementValueText.text = (row.oee * 100f).ToString("F2") + " %";
                }
                else
                {
                    panel.ElementValueText.text = "--";
                }
            }

            // ---------- UPTIME ----------
            // Possibly from availabilityCSVPerMinuteCSVPath or from OEE CSV
            if (panel.ElementValueText.name.IndexOf("Uptime", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // If you are reading from the "availability" CSV (like "Availability (%)"),
                // you can do something like:
                AvailabilityRow aRow = GetAvailabilityRowByMinute(availData, minuteIndex);
                if (aRow != null)
                {
                    // Show the availability percent as "Uptime"
                    panel.ElementValueText.text = aRow.availabilityPercent.ToString("F2") + " %";
                }
                else
                {
                    panel.ElementValueText.text = "--";
                }
            }

            // ---------- DOWNTIME ----------
            // Possibly from the availability CSV or OEE CSV "stop_time_sec"
            if (panel.ElementValueText.name.IndexOf("Downtime", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // If you want to get "stop time" from the availability CSV:
                AvailabilityRow aRow = GetAvailabilityRowByMinute(availData, minuteIndex);
                if (aRow != null)
                {
                    // aRow.stopTime is presumably in seconds or minutes, depending on your CSV
                    panel.ElementValueText.text = aRow.stopTime.ToString("F2");
                }
                else
                {
                    panel.ElementValueText.text = "--";
                }
            }

            // ---------- THROUGHPUT ----------
            // from OEE per minute CSV’s “total_count” or something similar
            if (panel.ElementValueText.name.IndexOf("Throughput", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                OeeMinuteRow row = GetOeeRowByMinute(oeeRows, minuteIndex);
                if (row != null)
                {
                    panel.ElementValueText.text = row.total_count.ToString();
                }
                else
                {
                    panel.ElementValueText.text = "0";
                }
            }

            // ---------- GOOD PRODUCTS ----------
            if (panel.ElementValueText.name.IndexOf("GoodProducts", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                OeeMinuteRow row = GetOeeRowByMinute(oeeRows, minuteIndex);
                if (row != null)
                {
                    panel.ElementValueText.text = row.good_count.ToString();
                }
                else
                {
                    panel.ElementValueText.text = "0";
                }
            }

            // ---------- BAD PRODUCTS ----------
            if (panel.ElementValueText.name.IndexOf("BadProducts", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // We can get the "defective_count" from OEE
                // plus the aggregated counts from the 5 CSVs storing occurrences
                // (We add up occurrences * each respective factor.)
                OeeMinuteRow row = GetOeeRowByMinute(oeeRows, minuteIndex);
                int totalBadFromOee = (row != null) ? row.defective_count : 0;

                // Also read from the 5 separate CSVs
                int tripleDoublePickupBad = CountBadOccurrences(
                    tripleDoublePickupData[panel], currentTime, 3
                );
                int triplePickupBad = CountBadOccurrences(
                    triplePickupData[panel], currentTime, 3
                );
                int pickupBad = CountBadOccurrences(
                    pickupData[panel], currentTime, 1
                );
                int doubleHandedBad = CountBadOccurrences(
                    doubleHandedPickupData[panel], currentTime, 2
                );
                int doublePickupBad = CountBadOccurrences(
                    doublePickupData[panel], currentTime, 2
                );

                int totalBad = totalBadFromOee 
                               + tripleDoublePickupBad
                               + triplePickupBad
                               + pickupBad
                               + doubleHandedBad
                               + doublePickupBad;

                panel.ElementValueText.text = totalBad.ToString();
            }
        }
    }

    // -------------------- Utility Methods --------------------
    private static TimeSpan ParseTime(string timeStr)
    {
        // Safely parse strings like "HH:MM:SS", "HH:MM:SS.xxx", etc.
        // If parsing fails, default to TimeSpan.Zero
        TimeSpan ts;
        if (TimeSpan.TryParse(timeStr, out ts))
            return ts;
        return TimeSpan.Zero;
    }

    private OeeMinuteRow GetOeeRowByMinute(List<OeeMinuteRow> rows, int minuteIndex)
    {
        // In your CSV, "minute" might start from 0, 1, etc.
        // We'll just do a simple find
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].minute == minuteIndex)
                return rows[i];
        }
        return null;
    }

    private AvailabilityRow GetAvailabilityRowByMinute(List<AvailabilityRow> rows, int minuteIndex)
    {
        // We know each row covers a minute range from Start Timestamp to End Timestamp.
        // For example: 0:00:00 to 0:01:00 is minute 0
        //              0:01:00 to 0:02:00 is minute 1, etc.
        // We can also do direct indexing if the CSV is strictly per-minute sequential.
        // Or we can do a search:
        TimeSpan minuteStart = TimeSpan.FromMinutes(minuteIndex);
        foreach (var row in rows)
        {
            if (minuteStart >= row.startTime && minuteStart < row.endTime)
            {
                return row;
            }
        }
        return null;
    }

    private int CountBadOccurrences(List<BadProductRow> data, TimeSpan currentTime, int badPerOccurrence)
    {
        // This example checks if the currentTime is within the row’s start/end,
        // and then multiplies by "badPerOccurrence".
        // But your logic might differ (maybe you want minute-based indexing).
        // You could do a search for the row that covers that minute, etc.

        int totalBad = 0;
        foreach (var row in data)
        {
            if (currentTime >= row.startTime && currentTime < row.endTime)
            {
                totalBad += row.occurrences * badPerOccurrence;
            }
        }
        return totalBad;
    }

    // -------------------- CSV Loading Methods --------------------
    private List<AvailabilityRow> LoadAvailabilityData(string filePath)
    {
        var list = new List<AvailabilityRow>();
        if (!File.Exists(filePath)) return list;

        // Example CSV line:
        // Start Timestamp,End Timestamp,Availability (%),Stop Time
        // 0:00:00,0:01:00,99.5,0.5
        var lines = File.ReadAllLines(filePath);
        // If the first line is a header, start from i=1
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 4) continue;
            var row = new AvailabilityRow(cols[0], cols[1], cols[2], cols[3]);
            list.Add(row);
        }
        return list;
    }

    private List<OeeMinuteRow> LoadOeeMinuteData(string filePath)
    {
        var list = new List<OeeMinuteRow>();
        if (!File.Exists(filePath)) return list;

        // Example CSV line:
        // minute,availability,performance,quality,oee,availability_precent,stop_time_sec,
        // defective_count,good_count,total_count, ...
        var lines = File.ReadAllLines(filePath);
        // If the first line is a header, start from i=1
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = line.Split(',');
            // Adjust if your CSV has fewer or more columns
            if (cols.Length < 10) continue; 
            list.Add(new OeeMinuteRow(cols));
        }
        return list;
    }

    private List<StopRow> LoadStopsData(string filePath)
    {
        var list = new List<StopRow>();
        if (!File.Exists(filePath)) return list;

        // Format: "ID,Start Timestamp,End Timestamp"
        var lines = File.ReadAllLines(filePath);
        // If first line is header, start from i=1
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 3) continue;
            var row = new StopRow(cols[0], cols[1], cols[2]);
            list.Add(row);
        }
        return list;
    }

    private List<BadProductRow> LoadBadProductData(string filePath)
    {
        var list = new List<BadProductRow>();
        if (!File.Exists(filePath)) return list;

        // Format: "ID,Start Timestamp,End Timestamp,Occurrences Count"
        var lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 4) continue;
            var row = new BadProductRow(cols);
            list.Add(row);
        }
        return list;
    }

    // ----------------------------------------------------------

    [System.Serializable]
    public class PanelElementData
    {
        public Text ElementValueText;  // The UI text field to update
        public string availabilityCSVPerMinuteCSVPath;
        public string productsPassingPerMinuteCSVPath; // (Unused in example, but you can adapt)
        public string OEEPerMinuteCSVPath;
        public string stopsCSVPath;
        
        // Additional CSVs for bad product occurrences:
        public string tripleDoublePickupCSVPath;  // each occurrence = 3 bad products
        public string triplePickupCSVPath;        // each occurrence = 3
        public string pickupCSVPath;              // each occurrence = 1
        public string doubleHandedPickupCSVPath;  // each occurrence = 2
        public string doublePickupCSVPath;        // each occurrence = 2

        public void setStatus(bool status)
        {
            this.ElementValueText.text = status ? "Active" : "Inactive";
            this.ElementValueText.color = status ? Color.green : Color.red;
        }
    }
}
