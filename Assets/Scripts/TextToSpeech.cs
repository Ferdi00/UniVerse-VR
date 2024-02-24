using System;
using TMPro;
using UnityEngine;

public class TextToSpeech : MonoBehaviour
{
    private string apiKey = "AIzaSyCA9_vnteeJwLkvzN1b9TdTHPSKncCcjuQ";

    private Action<string> _actionRequestReceived;
    private Action<BadRequestData> _errorReceived;
    private Action<AudioClip> _audioClipReceived;

    private RequestService _requestService;
    private static AudioConverter _audioConverter;

    public void GetSpeechAudioFromGoogle(string textToConvert, VoiceScriptableObject voice,
        Action<AudioClip> audioClipReceived, Action<BadRequestData> errorReceived)
    {
        if (_actionRequestReceived == null)
        {
            _actionRequestReceived = requestData => RequestReceived(requestData, audioClipReceived);
        }
        else
        {
            _actionRequestReceived += (requestData => RequestReceived(requestData,audioClipReceived));
        }

        if (_requestService == null)
            _requestService = new RequestService();
        if (_audioConverter == null)
            _audioConverter = new AudioConverter();
        
        var dataToSend = new DataToSend
        {
            input =
                new Input()
                {
                    text = textToConvert
                },
            voice =
                new Voice()
                {
                    languageCode = voice.languageCode,
                    name = voice.name
                },
            audioConfig =
                new AudioConfig()
                {
                    audioEncoding = "MP3",
                    pitch = voice.pitch,
                    speakingRate = voice.speed
                }
        };
        RequestService.SendDataToGoogle("https://texttospeech.googleapis.com/v1/text:synthesize", dataToSend,
            apiKey, _actionRequestReceived, errorReceived);
    }

    private static void RequestReceived(string requestData, Action<AudioClip> audioClipReceived)
    {
        var audioData = JsonUtility.FromJson<AudioData>(requestData);
        AudioConverter.SaveTextToMp3(audioData);
        _audioConverter.LoadClipFromMp3(audioClipReceived);
    }
}
