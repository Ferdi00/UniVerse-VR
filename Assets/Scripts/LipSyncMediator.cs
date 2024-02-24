using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LipSyncMediator : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private AudioSource audioSource;

    private OutputAudioRecorder recorder;
    public LipSyncMediator(AudioSource a)
    {
        audioSource = a;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(MakeRequest);
    }

    public void MakeRequest()
    {
        MakeAudioRequest();
    }

    private void MakeAudioRequest()
    {
        StartCoroutine(DownloadAudio());
    }

    IEnumerator DownloadAudio()    {         
        String fullPath = Application.temporaryCachePath + "/audio.mp3";
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.MPEG))         {         
            yield return www.SendWebRequest();             if (www.result == UnityWebRequest.Result.Success)             {       
                // Ottieni l'audio clip dal download
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);                 // Fai qualcosa con l'audio clip (es. assegnalo a un componente AudioSource)
                                 
                audioSource.clip = audioClip;                
                audioSource.Play();             }
            else
            {
                Debug.LogError("Errore nel download dell'audio: " + www.error);
            }         }     }
}