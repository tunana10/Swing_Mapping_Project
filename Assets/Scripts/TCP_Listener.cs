using System.Net.Sockets;
using System.IO;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class TCP_Listener : MonoBehaviour
{
    TcpClient tcpClient;
    Thread receiveThread;
    string latestMessage = "";

    float smoothedAngle = 0;
    public float smoothFactor = 0.05f;
    public float deadzone = 0.2f; // 微小變化範圍，這數值可手動微調

    Queue<float> angleBuffer = new Queue<float>();
    int bufferSize = 5; // 越大越平滑，但延遲越高

    // 可手動調整的映射參數
    public float inputMin = -5f;
    public float inputMax = 5f;
    public float outputMin = -45f;
    public float outputMax = 45f;

    float lastAppliedAngle = 0; // 加在 class 裡

    float GetMovingAverage(float newVal)
    {
        angleBuffer.Enqueue(newVal);
        if (angleBuffer.Count > bufferSize)
            angleBuffer.Dequeue();
        float sum = 0;
        foreach (float val in angleBuffer)
            sum += val;
        return sum / angleBuffer.Count;
    }


    void Start()
    {
        tcpClient = new TcpClient("127.0.0.1", 12345);  // Python TCP Server
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("連線到 Python TCP Server 成功");
    }

    void ReceiveLoop()
    {
        StreamReader reader = new StreamReader(tcpClient.GetStream());
        while (true)
        {
            try
            {
                latestMessage = reader.ReadLine();
            }
            catch
            {
                break;
            }
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(latestMessage))
        {
            if (float.TryParse(latestMessage, out float rawAngle))
            {
                float mappedAngle = Map(rawAngle, inputMin, inputMax, outputMin, outputMax);
                float averaged = GetMovingAverage(mappedAngle);

                // 如果變化大於 deadzone 才更新
                if (Mathf.Abs(averaged - lastAppliedAngle) > deadzone)
                {
                    smoothedAngle = Mathf.Lerp(smoothedAngle, averaged, smoothFactor);
                    transform.rotation = Quaternion.Euler(smoothedAngle, 0, 0);
                    lastAppliedAngle = smoothedAngle;
                }

                Debug.Log($"原始: {rawAngle:F2}, 映射後: {mappedAngle:F2}, 平滑: {smoothedAngle:F2}");
            }
        }
    }

    // 映射函數
    float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return Mathf.Clamp01((value - inMin) / (inMax - inMin)) * (outMax - outMin) + outMin;
    }

    void OnApplicationQuit()
    {
        tcpClient?.Close();
        receiveThread?.Abort();
    }
}
