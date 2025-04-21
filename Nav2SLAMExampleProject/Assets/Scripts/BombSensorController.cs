using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;

public class BombSensorController : MonoBehaviour
{
    // Settings
    [Header("Sensor Settings")]
    public float detectionRadius = 0.5f;
    public LayerMask detectionLayers;
    public string explosiveTag = "Explosive";
    public float sensorUpdateRate = 0.1f; // seconds
    
    // ROS parameters
    [Header("ROS Settings")]
    public string sensorTopic = "bomb_robot/bomb_detector";
    public string detectionDetailsTopic = "bomb_robot/bomb_details";
    private ROSConnection ros;
    
    // Detection feedback
    [Header("Visual Feedback")]
    public Light detectionLight;
    public Color normalColor = Color.green;
    public Color detectionColor = Color.red;
    public float pulseRate = 2f; // Hz
    
    // Internal variables
    private bool bombDetected = false;
    private GameObject detectedBomb;
    private float timer = 0f;
    private float pulseTimer = 0f;
    
    // Debug
    [Header("Debug")]
    public bool drawDebugSphere = true;
    
    void Start()
    {
        // Set up ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        
        // Initialize detectionLight if not set
        if (detectionLight == null)
        {
            detectionLight = GetComponentInChildren<Light>();
            if (detectionLight == null && transform.childCount > 0)
            {
                GameObject lightObj = new GameObject("DetectionLight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.zero;
                detectionLight = lightObj.AddComponent<Light>();
                detectionLight.type = LightType.Point;
                detectionLight.range = 0.2f;
                detectionLight.intensity = 1f;
            }
        }
        
        // Initialize light color
        if (detectionLight != null)
        {
            detectionLight.color = normalColor;
        }
    }
    
    void Update()
    {
        // Update sensor at specified rate
        timer += Time.deltaTime;
        if (timer >= sensorUpdateRate)
        {
            timer = 0f;
            ScanForExplosives();
        }
        
        // Update visual feedback
        UpdateVisualFeedback();
        
        // Debug visualization
        if (drawDebugSphere)
        {
            Color debugColor = bombDetected ? Color.red : Color.green;
            debugColor.a = 0.3f;
            Debug.DrawRay(transform.position, transform.forward * 0.2f, debugColor);
            DebugExtension.DrawWireSphere(transform.position, debugColor, detectionRadius);
        }
    }
    
    void ScanForExplosives()
    {
        // Reset detection state
        bombDetected = false;
        detectedBomb = null;
        
        // Scan for objects with explosive tag
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayers);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(explosiveTag))
            {
                bombDetected = true;
                detectedBomb = collider.gameObject;
                break;
            }
        }
        
        // Publish detection state to ROS
        PublishDetectionState();
        
        // Publish details if bomb detected
        if (bombDetected && detectedBomb != null)
        {
            PublishBombDetails();
        }
    }
    
    void PublishDetectionState()
    {
        // Create boolean message
        BoolMsg detectionMsg = new BoolMsg(bombDetected);
        
        // Publish message
        ros.Publish(sensorTopic, detectionMsg);
    }
    
    void PublishBombDetails()
    {
        if (detectedBomb == null) return;
        
        // Get bomb details from the detected object
        BombComponent bombComponent = detectedBomb.GetComponent<BombComponent>();
        
        // Create string message with bomb details
        string bombDetails = "Unknown explosive device";
        
        if (bombComponent != null)
        {
            bombDetails = string.Format(
                "Bomb Type: {0}\n" +
                "Threat Level: {1}\n" +
                "Timer: {2:0.00} sec\n" +
                "Wires: {3}",
                bombComponent.bombType,
                bombComponent.threatLevel,
                bombComponent.timerRemaining,
                bombComponent.wireCount
            );
        }
        
        // Publish string message
        StringMsg detailsMsg = new StringMsg(bombDetails);
        ros.Publish(detectionDetailsTopic, detailsMsg);
    }
    
    void UpdateVisualFeedback()
    {
        if (detectionLight == null) return;
        
        if (bombDetected)
        {
            // Pulse the light when bomb is detected
            pulseTimer += Time.deltaTime * pulseRate * 2 * Mathf.PI; // Convert to radians
            if (pulseTimer > 2 * Mathf.PI) pulseTimer -= 2 * Mathf.PI;
            
            float pulseIntensity = Mathf.Sin(pulseTimer) * 0.5f + 0.5f; // 0 to 1
            detectionLight.color = Color.Lerp(normalColor, detectionColor, pulseIntensity);
            detectionLight.intensity = Mathf.Lerp(0.5f, 1.5f, pulseIntensity);
        }
        else
        {
            // Reset to normal color
            detectionLight.color = normalColor;
            detectionLight.intensity = 0.5f;
        }
    }
    
    // Editor-only visualization
    void OnDrawGizmos()
    {
        Gizmos.color = bombDetected ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

// Helper class for debug visualization
public static class DebugExtension
{
    public static void DrawWireSphere(Vector3 position, Color color, float radius = 1.0f)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawWireDisc(position, Vector3.up, radius);
        UnityEditor.Handles.DrawWireDisc(position, Vector3.right, radius);
        UnityEditor.Handles.DrawWireDisc(position, Vector3.forward, radius);
        #endif
    }
} 