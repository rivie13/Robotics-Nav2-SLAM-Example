using UnityEngine;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;

public class CameraCapture : MonoBehaviour
{
    public Camera targetCamera;
    public RenderTexture renderTexture;
    private Texture2D texture2D;
    private ROSConnection ros;
    private string camera_topic = "/camera/image_raw";
    private string yolo_topic_classification = "/yolo/classification";

    void Start()
    {
        targetCamera.targetTexture = renderTexture;
        texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(camera_topic);
        ros.Subscribe<StringMsg>(yolo_topic_classification, ClassificationCallback);
        Debug.Log("SUBSCRIBED TO YOLO TOPIC CLASSIFICATION "+yolo_topic_classification);
        Debug.Log("PUBLISHING IMAGE MSG TO ROS2" + camera_topic);

    }

    void Update()
    {
       
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
            //Debug.Log("Fire classification received");
        }
    }
}