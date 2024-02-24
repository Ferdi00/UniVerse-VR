using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;


public class AudioConverter : MonoBehaviour
{
    private const string Mp3FileName = "audio.mp3";
    public static string audioFilePath = Path.Combine(Application.persistentDataPath,Mp3FileName); // Asigna la ruta de tu archivo .mp3 en el Editor de Unity
    


    public static void SaveTextToMp3(AudioData audioData)
    {
        var bytes = Convert.FromBase64String(audioData.audioContent);
        Debug.Log("audioFilePath: " + audioFilePath);
        File.WriteAllBytes(audioFilePath, bytes);
        //Reproduce();
    }

    public void LoadClipFromMp3(Action<AudioClip> onClipLoaded)
    {
        _ = StartCoroutine(LoadClipFromMp3Cor(onClipLoaded));
    }

    private static IEnumerator LoadClipFromMp3Cor(Action<AudioClip> onClipLoaded)
    {
        //Debug.Log("Dentro");
        var downloadHandler =
            new DownloadHandlerAudioClip("file://" + audioFilePath,
                AudioType.MPEG);
        downloadHandler.compressed = false;
        //Debug.Log("Parte 1");
        using var webRequest = new UnityWebRequest("file://" + audioFilePath,
            "GET",
            downloadHandler, null);

        yield return webRequest.SendWebRequest();
        Debug.Log("Parte 2");
        if (webRequest.responseCode == 200)
        {
            onClipLoaded.Invoke(downloadHandler.audioClip);
        }
        Debug.Log("Parte 3");
        downloadHandler.Dispose();
    }
}

