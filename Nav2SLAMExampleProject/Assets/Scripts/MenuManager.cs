using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject simulationSelectPanel;
    
    [Header("Settings Fields")]
    public Dropdown robotModelDropdown;
    public Dropdown graphicsQualityDropdown;
    
    [Header("Simulation Settings")]
    private string selectedSimulation = "SimpleWarehouseScene";
    
    void Start()
    {
        // Set up graphics quality dropdown
        if (graphicsQualityDropdown != null)
        {
            graphicsQualityDropdown.ClearOptions();
            graphicsQualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            graphicsQualityDropdown.value = QualitySettings.GetQualityLevel();
        }
        
        // Load saved settings
        LoadSettings();
        
        // Show main menu panel
        ShowPanel(mainMenuPanel);
    }
    
    public void ShowPanel(GameObject panelToShow)
    {
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (simulationSelectPanel != null) simulationSelectPanel.SetActive(false);
        
        // Show the selected panel
        if (panelToShow != null) panelToShow.SetActive(true);
    }
    
    public void SelectSimulation(string simulationName)
    {
        selectedSimulation = simulationName;
        Debug.Log($"Selected simulation: {simulationName}");
    }
    
    public void StartSimulation()
    {
        // Save current settings before starting
        SaveSettings();
        
        // Load the selected simulation scene
        SceneManager.LoadScene(selectedSimulation);
    }
    
    public void QuitApplication()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        PlayerPrefs.Save();
    }
    
    public void SaveSettings()
    {
        if (robotModelDropdown != null)
            PlayerPrefs.SetInt("RobotModel", robotModelDropdown.value);
        
        if (graphicsQualityDropdown != null)
            PlayerPrefs.SetInt("GraphicsQuality", graphicsQualityDropdown.value);
        
        PlayerPrefs.SetString("SelectedSimulation", selectedSimulation);
        PlayerPrefs.Save();
    }
    
    public void LoadSettings()
    {
        if (robotModelDropdown != null)
            robotModelDropdown.value = PlayerPrefs.GetInt("RobotModel", 0);
        
        if (graphicsQualityDropdown != null)
            graphicsQualityDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        
        selectedSimulation = PlayerPrefs.GetString("SelectedSimulation", "SimpleWarehouseScene");
    }
} 