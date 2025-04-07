using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombComponent : MonoBehaviour
{
    public Material defaultTexture;
    public Material armedTexture;
    public Material defusedTexture;

    private Renderer renderer;
    private bool usingTexture1 = true; //this is to switch betwwn the bomb texture and the red texture
    private bool armed = true;  //flag to see if bomb is defused or not

    private float timePassed = 0f; // Keeps track of how much time has passed
    public float initialSwitchInterval = 2.0f; // Initial interval for switching textures
    public float minSwitchInterval = 0.2f; // The minimum interval to switch textures
    public float decreaseRate = 0.1f; // Rate at which the interval decreases over time

    void Start()
    {
        // Get the Renderer component of the cube
        renderer = GetComponent<Renderer>();

        // Start the texture switching with an initial interval
        StartCoroutine(SwitchTextureCoroutine());
    }

    IEnumerator SwitchTextureCoroutine()
    {
        while (armed)
        {
            // Wait for the current interval before switching textures
            yield return new WaitForSeconds(initialSwitchInterval);

            // Switch the texture
            SwitchTexture();

            // Update time passed
            timePassed += initialSwitchInterval;

            // Dynamically decrease the interval (up to a minimum interval)
            initialSwitchInterval = Mathf.Max(minSwitchInterval, initialSwitchInterval - decreaseRate);
        }
    }

    void SwitchTexture()
    {
        // Switch between textures
        if (armed)
        {
            if (usingTexture1)
            {
                renderer.material = defaultTexture;
            }
            else
            {
                renderer.material = armedTexture;
            }

            usingTexture1 = !usingTexture1;
        }

    }

    public void Defused()
    {
        armed = false;
        renderer.material = defusedTexture;
    }

}
