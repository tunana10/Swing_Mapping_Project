using UnityEngine;

public class MyMessageListener : MonoBehaviour
{
    float smoothedAngle = 0;
    float smoothFactor = 0.05f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //this.gameObject.transform.Rotate(angle,0,0);
        this.gameObject.transform.rotation = Quaternion.Euler(smoothedAngle, 0, 0);
    }

    // Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string msg)
    {
        Debug.Log("Arrived: " + msg);

        //angle = float.Parse(msg);

        if (float.TryParse(msg.Trim(), out float parsedAngle))
        {          
            smoothedAngle = Mathf.Lerp(smoothedAngle, parsedAngle, smoothFactor);
        }

    }
    // Invoked when a connect/disconnect event occurs. The parameter 'success'
    // will be 'true' upon connection, and 'false' upon disconnection or
    // failure to connect.
    void OnConnectionEvent(bool success)
    {
        Debug.Log(success ? "Device connected" : "Device disconnected");
    }
}
