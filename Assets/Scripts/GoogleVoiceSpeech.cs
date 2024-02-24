//	Copyright (c) 2016 steele of lowkeysoft.com
//        http://lowkeysoft.com
//
//	This software is provided 'as-is', without any express or implied warranty. In
//	no event will the authors be held liable for any damages arising from the use
//	of this software.
//
//	Permission is granted to anyone to use this software for any purpose,
//	including commercial applications, and to alter it and redistribute it freely,
//	subject to the following restrictions:
//
//	1. The origin of this software must not be misrepresented; you must not claim
//	that you wrote the original software. If you use this software in a product,
//	an acknowledgment in the product documentation would be appreciated but is not
//	required.
//
//	2. Altered source versions must be plainly marked as such, and must not be
//	misrepresented as being the original software.
//
//	3. This notice may not be removed or altered from any source distribution.
//
//  =============================================================================
//
// Acquired from https://github.com/steelejay/LowkeySpeech
//
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Threading.Tasks;


[RequireComponent(typeof(AudioSource))]

public class GoogleVoiceSpeech : MonoBehaviour
{
	public TextMeshProUGUI testo;
	public TMP_InputField inputText;
	[SerializeField] private Button button;
	[SerializeField] private Button sendButton;
	private TextMeshProUGUI buttonText;

	// Array de strings
	private string[] strings = { "Interpretazione audio .", "Interpretazione audio ..", "Interpretazione audio ..." };

	// Índice actual en el array
	private int currentIndex = 0;

	// Tiempo transcurrido desde la última actualización
	private float elapsedTime = 0f;

	// Intervalo de tiempo para cambiar de string
	private float interval = 1f; // Cambia cada segundo

	private bool esperando = false;

	private RootObject roote = null;
	private string filePath;

	struct ClipData
	{
		public int samples;
	}

	private int minFreq;
	private int maxFreq;
	private bool micConnected = false;
	private AudioSource goAudioSource;

	public string apiKey;

	// Inizializzazione
	void Start()
	{
		// Verifica se c'è un microfono connesso
		if (Microphone.devices.Length <= 0)
			Debug.LogWarning("Microphone not connected!");
		else
		{
			micConnected = true;

			//Get the default microphone recording capabilities
			Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

			//According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...
			if (minFreq == 0 && maxFreq == 0)
			{
				//...meaning 44100 Hz can be used as the recording sampling rate
				maxFreq = 44100;
			}

			//Get the attached AudioSource component
			goAudioSource = this.GetComponent<AudioSource>();
		}

		buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
		string buttonTextValue = buttonText.text;
		button.onClick.AddListener(Pressed);
	}
	private async void Pressed()
	{
		if (micConnected)
		{
			sendButton.interactable = false;
			if (!Microphone.IsRecording(null))
			{
				buttonText.text = "i";
				inputText.text = "Registrazione in corso. Tocca di nuovo per interrompere.";
				goAudioSource.clip = Microphone.Start(null, true, 7, maxFreq);
				//GUI.Label(new Rect(Screen.width/2-100, Screen.height/2+25, 200, 50), "Sto ascoltando..."); //Feedback visivo per l'ascolto
			}
			else
			{
				esperando = true;
				button.interactable = false;
				buttonText.text = "p";
				float filenameRand = UnityEngine.Random.Range(0.0f, 10.0f);
				string filename = "testing" + filenameRand;
				if (!filename.ToLower().EndsWith(".wav"))
				{
					filename = Path.ChangeExtension(filename, ".wav");
				}
				filePath = Path.Combine(Application.persistentDataPath, "testing", filename);
				Microphone.End(null); // Stop recording
				SavWav.Save(filePath, goAudioSource.clip);
				await Task.Run(() =>
				{
					// Llamar al método que tarda mucho
					roote = Auxiliar(filePath);
				});

			}
		}
		else
		{
			GUI.contentColor = Color.red;
			GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Microphone not connected!");
		}
	}
	private RootObject Auxiliar(string filePath)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(filePath));

		//Google API Key
		string apiURL = "https://speech.googleapis.com/v1/speech:recognize?&key=";

		string Response = HttpUploadFile(apiURL, filePath, "file", "audio/wav; rate=44100");
		return JsonConvert.DeserializeObject<RootObject>(Response);

	}

	public string HttpUploadFile(string url, string file, string paramName, string contentType)
	{
		ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

		Byte[] bytes = File.ReadAllBytes(file);
		String file64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);

		try
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				string json = "{ \"config\": { \"languageCode\" : \"it-IT\" }, \"audio\" : { \"content\" : \"" + file64 + "\"}}";
				streamWriter.Write(json);
				streamWriter.Flush();
				streamWriter.Close();
			}

			var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				var result = streamReader.ReadToEnd();
				return result;
			}
		}
		catch (WebException ex)
		{
			var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
		}
		return "empty";
	}

	private void Update()
	{
		if (roote != null)
		{
			if (roote.results != null && roote.results.Count > 0 &&
				roote.results[0].alternatives != null && roote.results[0].alternatives.Count > 0 &&
				roote.results[0].alternatives[0].transcript != null)
			{
				inputText.text = roote.results[0].alternatives[0].transcript;
				testo.text = "Ho ascoltato...ora invia";
			}
			else
				testo.text = "Non ho sentito.";

			roote = null;
			File.Delete(filePath); //Cancella il Wav temporaneo

			buttonText.text = "r";
			sendButton.interactable = true;
			button.interactable = true;
			esperando = false;
		}
		else if (esperando)
		{
			// Incrementar el tiempo transcurrido
			elapsedTime += Time.deltaTime;

			// Verificar si ha pasado el intervalo de tiempo
			if (elapsedTime >= interval)
			{
				// Reiniciar el tiempo transcurrido
				elapsedTime = 0f;

				// Avanzar al siguiente string circularmente
				currentIndex = (currentIndex + 1) % strings.Length;

				// Imprimir el string actual en la consola (puedes hacer lo que necesites con el string aquí)
				inputText.text = strings[currentIndex];
			}
		}
	}
	public class Alternative
	{
		public string transcript { get; set; }
		public double confidence { get; set; }
	}
	public class Result
	{
		public List<Alternative> alternatives { get; set; }
		public string resultEndTime { get; set; }
		public string languageCode { get; set; }
	}
	public class RootObject
	{
		public List<Result> results { get; set; }
		public string totalBilledTime { get; set; }
		public string requestId { get; set; }
	}
}

