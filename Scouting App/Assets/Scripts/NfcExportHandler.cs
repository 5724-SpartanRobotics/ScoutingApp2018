using ScoutingApp.GameData;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class NfcExportHandler : MonoBehaviour
{
	public string TagID;
	public Text ProgressText;
	public bool TagFound = false;

	private AndroidJavaObject _Activity;
	private AndroidJavaObject _Intent;
	private AndroidJavaObject _NfcWriter;
	private string _Action;

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

			MemoryStream stream = new MemoryStream();
			DataStorage.Instance.SerializeData(stream);

			_NfcWriter.Call("setMessage", stream.ToArray());
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
				MemoryStream stream = new MemoryStream();
				DataStorage.Instance.SerializeData(stream);

				// Compute hash to verify data integrity after beam
				MD5 md5 = MD5.Create();
				stream.Position = 0;
				byte[] hash = md5.ComputeHash(stream);

				stream.Position = stream.Length;
				stream.Write(hash, 0, hash.Length);

				_NfcWriter.Call("setMessage", stream.ToArray());
			}
		}
	}
}
