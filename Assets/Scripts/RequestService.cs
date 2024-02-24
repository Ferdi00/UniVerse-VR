using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RequestService : MonoBehaviour
{
    public static void SendDataToGoogle(string url, DataToSend dataToSend, string apiKey, Action<string> requestReceived,
        Action<BadRequestData> errorReceived)
    {
        var headers = new Dictionary<string, string>();
        headers.Add("X-Goog-Api-Key", apiKey);
        headers.Add("Content-Type", "application/json; charset=utf-8");
        Post(url, JsonUtility.ToJson(dataToSend), requestReceived,  errorReceived, headers);
    }
    
    private static async void Post(string url, string bodyJsonString, Action<string> requestReceived,
        Action<BadRequestData> errorReceived, Dictionary<string, string> headers = null)
    {
        var request = new UnityWebRequest(url, "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }
        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }
        
        // De aqui quité los ? de errorReceived y requestReceived
        if (HasError(request, out var badRequest))
        {
            Debug.Log("If Post");
            errorReceived?.Invoke(badRequest);
            
        }
        else
        {
            Debug.Log("Else Post: " + requestReceived + " / " + request.downloadHandler.text);
            requestReceived?.Invoke(request.downloadHandler.text);
            Debug.Log("End Else Post");
        }
        
        request.Dispose();
    }

    private static bool HasError(UnityWebRequest request, out BadRequestData badRequestData)
    {
        if (request.responseCode is 200 or 201)
        {
            badRequestData = null;
            return false;
        }

        badRequestData = JsonUtility.FromJson<BadRequestData>(request.downloadHandler.text);

        try
        {
            badRequestData = JsonUtility.FromJson<BadRequestData>(request.downloadHandler.text);
            return true;
        }
        catch (Exception)
        {
            badRequestData = new BadRequestData
            {
                error = new Error
                {
                    code = (int)request.responseCode,
                    message = request.error
                }
            };

            return true;
        }
    }
}
