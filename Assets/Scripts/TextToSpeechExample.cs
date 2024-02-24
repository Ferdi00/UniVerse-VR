using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextToSpeechExample : MonoBehaviour
{
    
    [SerializeField] private Button button;
    
    [SerializeField] private VoiceScriptableObject voice;
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TextMeshProUGUI inputField;
    
    // This is a modification, it was serialized
    private TextToSpeech textToSpeech = new TextToSpeech();
    
    private Action<AudioClip> _audioClipReceived;
    private Action<BadRequestData> _errorReceived;

    public TextToSpeechExample(VoiceScriptableObject v, AudioSource a)
    {
        voice = v;
        audioSource = a;
    }

    void Start()
    {
        button.onClick.AddListener(PressBtn);
        
    }
    
    // It's supposed that the conversion starts with a click (callable)
    public void PressBtn()
    {
        _errorReceived += ErrorReceived;
        _audioClipReceived += AudioClipReceived;
        textToSpeech.GetSpeechAudioFromGoogle(inputField.text + "?", voice, _audioClipReceived, _errorReceived);
        
    }
    
    public void Convert(String text)
    {
        _errorReceived += ErrorReceived;
        _audioClipReceived += AudioClipReceived;
        textToSpeech.GetSpeechAudioFromGoogle(text, voice, _audioClipReceived, _errorReceived);
        
    }

    private void ErrorReceived(BadRequestData badRequestData)
    {
        Debug.Log($"Error {badRequestData.error.code} : {badRequestData.error.message}");
    }

    private void AudioClipReceived(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}