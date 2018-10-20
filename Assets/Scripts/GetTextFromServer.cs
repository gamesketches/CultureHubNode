﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GetTextFromServer : MonoBehaviour {

	private static string url = "http://localhost";
	private static string port = "3000";
	Text displayText;
	public Text fullText;
	bool running;
	EmotionColorPicker colorPicker;
	
	// Use this for initialization
	void Start () {
		displayText = GetComponent<Text>();
		colorPicker = GetComponent<EmotionColorPicker>();
		running = false;

		//StartNodeServer();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			if(running) {
				StartCoroutine(ChangeServerStatus("stop"));
				running = false;
			}
			else {
				StartCoroutine(ChangeServerStatus("start"));
				running = true;
				StartCoroutine(SendRequests());
			}
		}	
	}

	void StartNodeServer(){
		System.Diagnostics.ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo();
		string path = Application.dataPath.Substring(0,Application.dataPath.Length - 7);
		Debug.Log(Application.dataPath.Substring(0,Application.dataPath.Length - 7));
		proc.FileName = "/bin/sh";
		proc.UseShellExecute = true;
		proc.CreateNoWindow = false;
		proc.Arguments = path + "node server.js";
		System.Diagnostics.Process.Start(proc);
	}

	IEnumerator ChangeServerStatus(string action) {
		WWWForm form = new WWWForm();
		form.AddField("action", action);
		UnityWebRequest www = UnityWebRequest.Post(url + ":" + port, form);
		yield return www.Send();

		if(www.isNetworkError) {
			Debug.Log(www.error);
		}
		else {
			Debug.Log("Server " + action);
		}
	}

	IEnumerator SendRequests(){
		while(running) {
			UnityWebRequest www = UnityWebRequest.Get(url +":" + port);
			yield return www.Send();

			if(www.isNetworkError)
			{
				Debug.Log(www.error);
			}
			else
			{
				if(www.responseCode == 200) {
					Debug.Log("Form sent complete!");
					Debug.Log("response:" + www.downloadHandler.text);
					fullText.text += www.downloadHandler.text;
					string[] words = fullText.text.Split(" ".ToCharArray());
					displayText.text = words[words.Length - 1];
					colorPicker.UpdateBackgroundColor(words[words.Length - 1]);
				}
				else if(www.responseCode == 206) {
					Debug.Log("Received Guess");
					string guess = www.downloadHandler.text;
					string[] words = guess.Split(" ".ToCharArray());
					displayText.text = words[words.Length - 1];
				}
				else if(www.responseCode ==204) {
					//Debug.Log("no new content!");
				}
				else {
					Debug.Log("Error response code: " + www.responseCode.ToString());
				}
			}
		}
	}
}
