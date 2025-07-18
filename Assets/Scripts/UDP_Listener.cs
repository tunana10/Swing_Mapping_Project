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
        tcpClient = new TcpClient("127.0.0.1", 12345);  // Python �}�� TCP �ݤf
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
                latestMessage = reader.ReadLine();  // ����Ū���
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
            Debug.Log("�Ӧ� ESP32�]�g Python�^�G " + latestMessage);
            // �A�i�H�b�o�̸ѪR float �ñ����
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
