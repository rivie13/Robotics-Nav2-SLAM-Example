using UnityEngine;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;

public class MultiCameraCapture : MonoBehaviour
{
    public Camera targetCamera;
    public RenderTexture renderTexture;
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
            Debug.LogError("Robot ID not set in MultiCameraCapture script! Please set the robotId in the Inspector.");
            enabled = false;
            return;
        }

        camera_topic = $"/{robotId}/camera/image_raw";
        yolo_topic_classification = $"/{robotId}/yolo/classification";

        if (targetCamera == null || renderTexture == null)
        {
            Debug.LogError($"{robotId}: Target Camera or Render Texture not assigned in MultiCameraCapture script!");
            enabled = false;
            return;
        }

        targetCamera.targetTexture = renderTexture;
        texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(camera_topic);
        ros.Subscribe<StringMsg>(yolo_topic_classification, ClassificationCallback);
        Debug.Log($"{robotId}: Publishing camera images to {camera_topic}, subscribing to {yolo_topic_classification}");
    }

    void Update()
    {
        //publish every 0.2 seconds. If hasn't been 0.2 seconds, do nothing. reduces cpu load
        if (Time.time - lastPublishTime < publishInterval) return;
        lastPublishTime = Time.time;
        //same as 1 robot
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
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

    void ClassificationCallback(StringMsg msg)
    {
        if (msg.data == "fire")
        {
            Debug.Log($"{robotId}: Classification is: {msg.data}");
        }
    }
}