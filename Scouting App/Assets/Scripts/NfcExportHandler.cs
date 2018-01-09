using ScoutingApp.GameData;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class NfcExportHandler : MonoBehaviour
{
	public string TagID;
	public Text ProgressText;

	private AndroidJavaObject _Activity;
	private AndroidJavaObject _NfcWriter;

	// Use this for initialization
	void Start()
	{
		if (!Options.Inst.IsNFCEnabled)
		{
			ProgressText.text = "There was an error. We have no idea how you got to this page.";
		}
		else
		{
			_Activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			_NfcWriter = new AndroidJavaObject("us.shsrobotics.scoutingapp.androidnfchelper.NfcWriter");
			_NfcWriter.Call("register", _Activity);
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Options.Inst.IsNFCEnabled)
		{
			if (_NfcWriter.Call<bool>("broadcasting"))
			{
				ProgressText.text = "Sending...";
			}
			else
			{
				const int HASH_LEN = 16;
				const int LEN_LEN = 4;
				const int HEADER_LEN = HASH_LEN + LEN_LEN;

				MemoryStream stream = new MemoryStream();
				stream.Write(new byte[HEADER_LEN], 0, HEADER_LEN);
				DataStorage.Instance.SerializeData(stream);

				// Compute hash to verify data after NDEF transfer
				stream.Position = HEADER_LEN;
				MD5 md5 = MD5.Create();
				byte[] hash = md5.ComputeHash(stream);

				stream.Position = 0;
				stream.Write(hash, 0, HASH_LEN);

				int len = (int)(stream.Length - HEADER_LEN);
				byte[] lenBytes = new byte[LEN_LEN];
				lenBytes[0] = (byte)((len >> 24) & 0xFF);
				lenBytes[1] = (byte)((len >> 16) & 0xFF);
				lenBytes[2] = (byte)((len >> 8) & 0xFF);
				lenBytes[3] = (byte)(len & 0xFF);
				stream.Write(lenBytes, 0, LEN_LEN);

				Debug.Log(Convert.ToBase64String(stream.ToArray()));
				_NfcWriter.Call("setMessage", stream.ToArray());
			}
		}
	}
}
