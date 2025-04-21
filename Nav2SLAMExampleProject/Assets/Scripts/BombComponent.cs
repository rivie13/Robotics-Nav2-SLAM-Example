using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BombComponent : MonoBehaviour
{
    // Bomb properties
    [Header("Bomb Properties")]
    public string bombType = "IED";
    public enum ThreatLevel { Low, Medium, High, Critical }
    public ThreatLevel threatLevel = ThreatLevel.Medium;
    public int wireCount = 3;
    public bool isArmed = true;
    
    // Timer settings
    [Header("Timer Settings")]
    public bool hasTimer = true;
    public float initialTime = 60f;
    public float timerRemaining = 60f;
    public bool timerRunning = false;
    
    // Visual components
    [Header("Visual Components")]
    public GameObject displayObject;
    public TextMesh timerDisplay;
    public Light statusLight;
    public Color armedColor = Color.red;
    public Color disarmedColor = Color.green;
    public Color warningColor = new Color(1f, 0.5f, 0f);
    
    // Wires
    [Header("Wires")]
    public List<GameObject> wireObjects = new List<GameObject>();
    public List<int> correctWireIndices = new List<int>();
    
    // Events
    [Header("Events")]
    public UnityEvent onDisarmed;
    public UnityEvent onExploded;
    public UnityEvent onTimerTick;
    
    // Sound effects
    [Header("Sound Effects")]
    public AudioSource timerTickSound;
    public AudioSource explosionSound;
    public AudioSource disarmSound;
    
    // Internal variables
    private bool wasDisarmed = false;
    private float lastTickTime = 0f;
    
    void Start()
    {
        // Initialize the bomb
        InitializeBomb();
        
        // Tag this object as explosive
        gameObject.tag = "Explosive";
        
        // Start timer if armed and has timer
        if (isArmed && hasTimer && timerRunning)
        {
            timerRemaining = initialTime;
        }
        
        // Initialize timer display
        UpdateTimerDisplay();
        
        // Initialize status light
        UpdateStatusLight();
    }
    
    void Update()
    {
        // Update timer if running
        if (isArmed && hasTimer && timerRunning)
        {
            // Decrease timer
            timerRemaining -= Time.deltaTime;
            
            // Check for timer tick event (once per second)
            if (Mathf.Floor(timerRemaining) != Mathf.Floor(lastTickTime))
            {
                onTimerTick?.Invoke();
                if (timerTickSound != null)
                {
                    timerTickSound.Play();
                }
            }
            
            // Store last tick time
            lastTickTime = timerRemaining;
            
            // Update timer display
            UpdateTimerDisplay();
            
            // Check for explosion
            if (timerRemaining <= 0)
            {
                Explode();
            }
            
            // Update status light (gets more red as time runs out)
            UpdateStatusLight();
        }
    }
    
    void InitializeBomb()
    {
        // Generate random correct wire indices if empty
        if (correctWireIndices.Count == 0 && wireObjects.Count > 0)
        {
            int correctWireCount = Mathf.Max(1, wireCount / 2);
            for (int i = 0; i < correctWireCount; i++)
            {
                int randomIndex = Random.Range(0, wireObjects.Count);
                if (!correctWireIndices.Contains(randomIndex))
                {
                    correctWireIndices.Add(randomIndex);
                }
                else
                {
                    i--; // Try again
                }
            }
        }
    }
    
    void UpdateTimerDisplay()
    {
        if (timerDisplay != null)
        {
            int minutes = Mathf.FloorToInt(timerRemaining / 60f);
            int seconds = Mathf.FloorToInt(timerRemaining % 60f);
            timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Change color based on time remaining
            if (timerRemaining <= 10f)
            {
                timerDisplay.color = Color.red;
            }
            else if (timerRemaining <= 30f)
            {
                timerDisplay.color = warningColor;
            }
            else
            {
                timerDisplay.color = Color.white;
            }
        }
    }
    
    void UpdateStatusLight()
    {
        if (statusLight != null)
        {
            if (!isArmed)
            {
                statusLight.color = disarmedColor;
            }
            else if (hasTimer && timerRunning)
            {
                // Blend from green to red as timer runs down
                float timerRatio = timerRemaining / initialTime;
                statusLight.color = Color.Lerp(armedColor, warningColor, timerRatio);
            }
            else
            {
                statusLight.color = armedColor;
            }
        }
    }
    
    public void Disarm()
    {
        if (!isArmed || wasDisarmed) return;
        
        isArmed = false;
        wasDisarmed = true;
        timerRunning = false;
        
        // Update visuals
        UpdateStatusLight();
        
        // Play sound
        if (disarmSound != null)
        {
            disarmSound.Play();
        }
        
        // Trigger event
        onDisarmed?.Invoke();
        
        Debug.Log("Bomb disarmed successfully!");
    }
    
    public void Explode()
    {
        if (!isArmed || wasDisarmed) return;
        
        // Play explosion sound
        if (explosionSound != null)
        {
            explosionSound.Play();
        }
        
        // Trigger event
        onExploded?.Invoke();
        
        Debug.Log("Bomb exploded!");
        
        // Optionally destroy the object
        // Destroy(gameObject);
    }
    
    public void CutWire(int wireIndex)
    {
        if (!isArmed || wasDisarmed) return;
        
        // Check if this wire should be cut
        if (correctWireIndices.Contains(wireIndex))
        {
            // Correct wire
            Debug.Log("Correct wire cut!");
            
            // Hide the wire object
            if (wireIndex < wireObjects.Count && wireObjects[wireIndex] != null)
            {
                wireObjects[wireIndex].SetActive(false);
            }
            
            // Remove from correct wires list
            correctWireIndices.Remove(wireIndex);
            
            // Check if all correct wires are cut
            if (correctWireIndices.Count == 0)
            {
                Disarm();
            }
        }
        else
        {
            // Wrong wire - cause explosion
            Debug.Log("Wrong wire cut!");
            Explode();
        }
    }
    
    public void StartTimer()
    {
        if (isArmed && hasTimer)
        {
            timerRunning = true;
        }
    }
    
    public void StopTimer()
    {
        timerRunning = false;
    }
    
    public float GetTimeRemaining()
    {
        return timerRemaining;
    }
    
    public bool IsDisarmed()
    {
        return !isArmed;
    }
    
    public bool IsTimerRunning()
    {
        return timerRunning;
    }
} 