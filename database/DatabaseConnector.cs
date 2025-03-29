using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class SimulationEvent
{
    public int id;
    public string robot_type;
    public string world_type;
    public string disaster_type;
    public float? resolution_time_seconds;
    public bool completed;
    public string started_at;
    public string completed_at;
}

[Serializable]
public class SimulationEventList
{
    public List<SimulationEvent> events;
}

public class DatabaseConnector : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiUrl = "http://localhost:5000";
    
    private int currentEventId = -1;
    
    void Start()
    {
        Debug.Log("DatabaseConnector initialized");
    }
    
    /// <summary>
    /// Starts a new simulation event in the database
    /// </summary>
    /// <param name="robotType">Type of robot being used</param>
    /// <param name="worldType">Environment the robot is in</param>
    /// <param name="disasterType">Type of disaster (optional)</param>
    public void StartSimulationEvent(string robotType, string worldType, string disasterType = null)
    {
        StartCoroutine(CreateEventCoroutine(robotType, worldType, disasterType));
    }
    
    /// <summary>
    /// Marks the current simulation event as completed
    /// </summary>
    /// <param name="resolutionTimeSeconds">Time taken to resolve the disaster</param>
    public void CompleteSimulationEvent(float resolutionTimeSeconds)
    {
        if (currentEventId <= 0)
        {
            Debug.LogWarning("No active simulation event to complete");
            return;
        }
        
        StartCoroutine(CompleteEventCoroutine(currentEventId, resolutionTimeSeconds));
    }
    
    /// <summary>
    /// Gets all simulation events from the database
    /// </summary>
    /// <param name="callback">Callback to handle the events</param>
    public void GetAllEvents(Action<List<SimulationEvent>> callback)
    {
        StartCoroutine(GetEventsCoroutine(callback));
    }
    
    private IEnumerator CreateEventCoroutine(string robotType, string worldType, string disasterType)
    {
        // Create the request data
        var requestData = new Dictionary<string, string>
        {
            { "robot_type", robotType },
            { "world_type", worldType }
        };
        
        if (!string.IsNullOrEmpty(disasterType))
        {
            requestData.Add("disaster_type", disasterType);
        }
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/api/events", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error creating event: {www.error}");
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"Event created: {responseText}");
                
                // Parse the response to get the event ID
                SimulationEvent createdEvent = JsonUtility.FromJson<SimulationEvent>(responseText);
                currentEventId = createdEvent.id;
                Debug.Log($"Current event ID: {currentEventId}");
            }
        }
    }
    
    private IEnumerator CompleteEventCoroutine(int eventId, float resolutionTimeSeconds)
    {
        // Create the request data
        var requestData = new Dictionary<string, float>
        {
            { "resolution_time_seconds", resolutionTimeSeconds }
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/api/events/{eventId}/complete", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error completing event: {www.error}");
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"Event completed: {responseText}");
                
                // Reset the current event ID
                currentEventId = -1;
            }
        }
    }
    
    private IEnumerator GetEventsCoroutine(Action<List<SimulationEvent>> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/api/events"))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error getting events: {www.error}");
                callback?.Invoke(new List<SimulationEvent>());
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"Events retrieved: {responseText}");
                
                // Parse the response to get the list of events
                // Unity JSON utility doesn't directly support array deserialization
                // so we'll wrap it
                string wrappedJson = "{ \"events\": " + responseText + " }";
                SimulationEventList eventList = JsonUtility.FromJson<SimulationEventList>(wrappedJson);
                callback?.Invoke(eventList.events);
            }
        }
    }
} 