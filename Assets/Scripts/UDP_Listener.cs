using System.Net.Sockets;
using System.IO;
using System.Threading;
using UnityEngine;

public class UDP_Listener : MonoBehaviour
{
    TcpClient tcpClient;
    Thread receiveThread;
    string latestMessage = "";

    float smoothedAngle = 0;
    float smoothFactor = 0.05f;

    void Start()
    {
        tcpClient = new TcpClient("127.0.0.1", 12345);  // Python 開的 TCP 端口
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
                latestMessage = reader.ReadLine();  // 持續讀資料
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
            Debug.Log("來自 ESP32（經 Python）： " + latestMessage);
            // 你可以在這裡解析 float 並控制物體
            if (float.TryParse(latestMessage, out float angle))
            {
                smoothedAngle = Mathf.Lerp(smoothedAngle,angle,smoothFactor);
                transform.rotation = Quaternion.Euler(smoothedAngle, 0, 0);
            }
                
        }
    }

    void OnApplicationQuit()
    {
        tcpClient?.Close();
        receiveThread?.Abort();
    }
}
