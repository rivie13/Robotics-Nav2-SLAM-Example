using UnityEngine;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;

// Captures and publishes camera images to Foxglove for each robot
public class MultiCameraCapture : MonoBehaviour
{
    public Camera targetCamera;
    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private ROSConnection ros;
    [SerializeField] private string robotId;
    private string camera_topic;
    private string yolo_topic_classification;

    private float publishInterval = 0.2f;
    private float lastPublishTime;

    
    void Start()
    {
        if (string.IsNullOrEmpty(robotId))
        {
            Debug.LogError("Robot ID not set!");
            enabled = false;
            return;
        }

        camera_topic = $"/{robotId}/camera/image_raw";
        yolo_topic_classification = $"/{robotId}/yolo/classification";

        if (targetCamera == null)
        {
            Debug.LogError($"{robotId}: Target Camera not assigned!");
            enabled = false;
            return;
        }

        // create unique render texture. Higher res is more lag
        renderTexture = new RenderTexture(640, 480, 24);
        renderTexture.name = $"RenderTexture_{robotId}";
        targetCamera.targetTexture = renderTexture;
        texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(camera_topic);
        ros.Subscribe<StringMsg>(yolo_topic_classification, ClassificationCallback);
        Debug.Log($"{robotId}: Publishing to {camera_topic}");
    }

    void Update()
    {
        if (Time.time - lastPublishTime < publishInterval) return; // Skip if interval not met. only pub every 0.2 sec for cpu load reasons
        lastPublishTime = Time.time;

        // capture camera image
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        // flip image vertically for foxglove display
        Color[] pixels = texture2D.GetPixels();
        Color[] flippedPixels = new Color[pixels.Length];
        for (int y = 0; y < texture2D.height; y++)
            for (int x = 0; x < texture2D.width; x++)
                flippedPixels[x + y * texture2D.width] = pixels[x + (texture2D.height - 1 - y) * texture2D.width];
        texture2D.SetPixels(flippedPixels);
        texture2D.Apply();

        // publish image data
        byte[] rawData = texture2D.GetRawTextureData();
        ImageMsg imageMsg = new ImageMsg
        {
            height = (uint)texture2D.height,
            width = (uint)texture2D.width,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(texture2D.width * 3),
            data = rawData
        };
        ros.Publish(camera_topic, imageMsg);
    }

    // log the YOLO fire detection
    void ClassificationCallback(StringMsg msg)
    {
        if (msg.data == "fire")
        {
            Debug.Log($"{robotId}: Classification is: {msg.data}");
        }
    }
}