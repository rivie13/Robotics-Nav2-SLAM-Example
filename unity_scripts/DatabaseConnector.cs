using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

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
    [SerializeField] private string apiBaseUrl = "http://localhost:5000/api";
    
    private int currentSimulationEventId = -1;

    // Start simulation event when robot is spawned
    public void StartSimulationEvent(string robotType, string worldType, string disasterType = null)
    {
        StartCoroutine(CreateSimulationEvent(robotType, worldType, disasterType));
    }
    
    // Complete simulation event when robot resolves disaster
    public void CompleteSimulationEvent(float resolutionTimeSeconds)
    {
        if (currentSimulationEventId != -1)
        {
            StartCoroutine(CompleteSimulationEventCoroutine(currentSimulationEventId, resolutionTimeSeconds));
        }
        else
        {
            Debug.LogError("Cannot complete simulation event: No active event");
        }
    }
    
    // Get all simulation events
    public void GetAllEvents(Action<List<SimulationEvent>> callback)
    {
        StartCoroutine(GetAllEventsCoroutine(callback));
    }
    
    // API interaction methods
    private IEnumerator CreateSimulationEvent(string robotType, string worldType, string disasterType)
    {
        var eventData = new Dictionary<string, string>
        {
            { "robot_type", robotType },
            { "world_type", worldType }
        };
        
        if (!string.IsNullOrEmpty(disasterType))
        {
            eventData.Add("disaster_type", disasterType);
        }
        
        string jsonData = JsonUtility.ToJson(eventData);
        
        using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl + "/events", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                SimulationEvent createdEvent = JsonUtility.FromJson<SimulationEvent>(request.downloadHandler.text);
                currentSimulationEventId = createdEvent.id;
                Debug.Log($"Created simulation event with ID: {currentSimulationEventId}");
            }
            else
            {
                Debug.LogError($"Error creating simulation event: {request.error}");
            }
        }
    }
    
    private IEnumerator CompleteSimulationEventCoroutine(int eventId, float resolutionTimeSeconds)
    {
        var eventData = new Dictionary<string, float>
        {
            { "resolution_time_seconds", resolutionTimeSeconds }
        };
        
        string jsonData = JsonUtility.ToJson(eventData);
        
        using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl + $"/events/{eventId}/complete", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Completed simulation event with ID: {eventId}");
                currentSimulationEventId = -1; // Reset current event
            }
            else
            {
                Debug.LogError($"Error completing simulation event: {request.error}");
            }
        }
    }
    
    private IEnumerator GetAllEventsCoroutine(Action<List<SimulationEvent>> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiBaseUrl + "/events"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                // Parse JSON array response
                SimulationEventList eventList = JsonUtility.FromJson<SimulationEventList>("{\"events\":" + jsonResponse + "}");
                callback(eventList.events);
            }
            else
            {
                Debug.LogError($"Error getting simulation events: {request.error}");
                callback(new List<SimulationEvent>());
            }
        }
    }
} 