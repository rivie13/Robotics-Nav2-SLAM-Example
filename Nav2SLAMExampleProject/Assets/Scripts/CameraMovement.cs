using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{public float panSpeed = 0.5f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    private Vector3 lastMousePosition;

    void Update()
    {
        HandleMouseDrag();
        HandleZoom();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Pan direction based on camera's right and forward vectors (ignoring Y)
            Vector3 right = transform.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up); // flatten forward to ground plane

            Vector3 move = (right * -delta.x + forward * -delta.y) * panSpeed * Time.deltaTime;
            transform.position += move;

            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 direction = transform.up;
            Vector3 newPosition = transform.position - direction * scroll * zoomSpeed;

            // Clamp zoom (based on height for top-down)
            float clampedY = Mathf.Clamp(newPosition.y, minZoom, maxZoom);
            newPosition.y = clampedY;

            transform.position = newPosition;
        }
    }
}