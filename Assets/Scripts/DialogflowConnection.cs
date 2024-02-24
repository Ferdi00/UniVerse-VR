using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Android;
using System.IO;

public class DialogflowConnection : MonoBehaviour
{
    private const string apiUrl = "https://universe-lmr2.onrender.com/Chatbot";

    public TextMeshProUGUI risposta;
    public TMP_InputField question;
    public TextMeshProUGUI session;
    public Button button;
    public Button recordButton;
    public VoiceScriptableObject voice;
    public AudioSource audioSource;
    public Animator animator;

    public GameObject content;
    public ScrollRect scrollView;
    private string sessionId;

    private TextToSpeechExample textToSpeech;
    private TextMeshProUGUI buttonText;
    private LipSyncMediator lipsync;

    private string[] opciones = {
    "Sto trasformando onde vocali in parole scritte!",
    "Il magico processo di traduzione ha avuto inizio!",
    "Sto traducendo, intanto ammira la mia bellezza.",
    "Mentre il mio codice traduce le tue parole, ammira come mi muovo."
    };

    private string[] strings = { "Generando una risposta .", "Generando una risposta ..", "Generando una risposta ..." };
    private int currentIndex = 0;
    private float elapsedTime = 0f;
    private float interval = 1f;
    private bool esperando = false;

    private GameObject newTextObject;
    private TextMeshProUGUI newText;

    private void Start()
    {
        // Crear un nuevo objeto de texto
        newTextObject = new GameObject("TextObject");
        newTextObject.transform.SetParent(content.transform, false);

        // Añadir un componente de texto al nuevo objeto
        newText = newTextObject.AddComponent<TextMeshProUGUI>();

        textToSpeech = new TextToSpeechExample(voice, audioSource);
        lipsync = new LipSyncMediator(audioSource);
        button.onClick.AddListener(SendTestStringToDialogflow);
        recordButton.interactable = false;
        buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
        HandleResponse("Ciao, sono UniVerse, come posso aiutarti?");
        sessionId = GenerarStringAleatorio(32);
        session.text = "Session ID: " + sessionId;
    }

    private static string GenerarStringAleatorio(int longitud)
    {
        const string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder sb = new StringBuilder();
        System.Random rnd = new System.Random();
        for (int i = 0; i < longitud; i++)
        {
            int indiceCaracter = rnd.Next(0, caracteresPermitidos.Length);
            sb.Append(caracteresPermitidos[indiceCaracter]);
        }
        return sb.ToString();
    }

    private void Update()
    {

        bool isTalking = animator.GetBool("Talking");
        if (!audioSource.isPlaying && isTalking && buttonText.text == "r")
        {
            animator.SetBool("Talking", false);
            recordButton.interactable = true;
        }
        if (buttonText.text == "p")
        {
            buttonText.text = "f";
            HandleResponse(ElegirOpcionAleatoria(opciones));
        }
        if (esperando)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= interval)
            {
                elapsedTime = 0f;
                currentIndex = (currentIndex + 1) % strings.Length;
                risposta.text = strings[currentIndex];
                newText.text = strings[currentIndex];
            }
        }
    }

    private static T ElegirOpcionAleatoria<T>(T[] array)
    {
        System.Random rnd = new System.Random();
        int indiceAleatorio = rnd.Next(0, array.Length);
        return array[indiceAleatorio];
    }

    public void SendTestStringToDialogflow()
    {
        esperando = true;
        recordButton.interactable = false;
        string textMessage = question.text;
        StartCoroutine(SendToDialogflow(textMessage, HandleResponse));
        question.text = "";
    }

    public IEnumerator SendToDialogflow(string message, Action<string> onResponse)
    {
        newText.text = message;
        newText.fontStyle = FontStyles.Normal;
        nuevaLinea();
        var requestData = new Data { message = message, session = sessionId };
        string jsonData = JsonUtility.ToJson(requestData);
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Errore nella richiesta a Dialogflow: " + request.error);
            }
            else
            {
                var responseData = JsonUtility.FromJson<Data>(request.downloadHandler.text);
                onResponse?.Invoke(responseData.message);
                nuevaLinea();
            }
        }
    }

    private void nuevaLinea()
    {
        // Crear un nuevo objeto de texto
        newTextObject = new GameObject("TextObject");
        newTextObject.transform.SetParent(content.transform, false);

        // Añadir un componente de texto al nuevo objeto
        newText = newTextObject.AddComponent<TextMeshProUGUI>();
        formatoBasico();
    }

    private void formatoBasico()
    {
        // Establecer la fuente y el tamaño del texto
        newText.font = Resources.Load<TMP_FontAsset>("Fonts/LiberationSans SDF");
        newText.fontSize = 12;

        // Establecer el estilo del texto (negrita)
        newText.fontStyle = FontStyles.Bold;

        // Establecer el color del texto a negro
        newText.color = Color.black;

        // Establecer el estilo del texto (negrita)
        newText.fontStyle = FontStyles.Bold;
        // Establecer el ancho fijo del texto

        newText.rectTransform.sizeDelta = new Vector2(318f, newText.preferredHeight);
    }

    private void escribir(string text)
    {

        // Establecer el texto que se mostrará
        newText.text = text;

        formatoBasico();

        scrollView.normalizedPosition = new Vector2(0, -24);

    }

    private void HandleResponse(string response)
    {
        textToSpeech.Convert(response);
        esperando = false;
        risposta.text = response;
        escribir(response);
        StartCoroutine(DownloadAudio());
    }
    private IEnumerator DownloadAudio()
    {
        yield return new WaitForSeconds(1f);
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        string fullPath = Path.Combine(Application.persistentDataPath, "audio.mp3");
        string fileURL = "file://" + fullPath;

        // Carica il file MP3 utilizzando UnityWebRequest
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileURL, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            // Controlla se ci sono errori durante il caricamento
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Errore durante il caricamento del file MP3: " + www.error);
                yield break;
            }

            // Ottieni i dati audio dal file MP3
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

            // Assegna l'AudioClip all'AudioSource e riproduci
            audioSource.clip = audioClip;
            audioSource.volume = 1f;
            audioSource.spatialBlend = 1f; // Imposta l'audio come 3D
            audioSource.minDistance = 1f; // Distanza minima in cui l'audio è udibile
            audioSource.maxDistance = 10f;
            animator.SetBool("Talking", true);
            audioSource.Play();
        }
    }

    [Serializable]
    private class Data
    {
        public string message;
        public string session;
    }
}