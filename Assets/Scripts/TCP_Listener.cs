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
    public float deadzone = 0.2f; // �L�p�ܤƽd��A�o�ƭȥi��ʷL��

    Queue<float> angleBuffer = new Queue<float>();
    int bufferSize = 5; // �V�j�V���ơA������V��

    // �i��ʽվ㪺�M�g�Ѽ�
    public float inputMin = -5f;
    public float inputMax = 5f;
    public float outputMin = -45f;
    public float outputMax = 45f;

    float lastAppliedAngle = 0; // �[�b class ��

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
        Debug.Log("�s�u�� Python TCP Server ���\");
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

                // �p�G�ܤƤj�� deadzone �~��s
                if (Mathf.Abs(averaged - lastAppliedAngle) > deadzone)
                {
                    smoothedAngle = Mathf.Lerp(smoothedAngle, averaged, smoothFactor);
                    transform.rotation = Quaternion.Euler(smoothedAngle, 0, 0);
                    lastAppliedAngle = smoothedAngle;
                }

                Debug.Log($"��l: {rawAngle:F2}, �M�g��: {mappedAngle:F2}, ����: {smoothedAngle:F2}");
            }
        }
    }

    // �M�g���
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
