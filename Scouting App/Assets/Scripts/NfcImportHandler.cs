using ScoutingApp.GameData;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NfcImportHandler : MonoBehaviour
{
	public string TagID;
	public Text ProgressText;
	public bool TagFound = false;

	private AndroidJavaObject _Activity;
	private AndroidJavaObject _Intent;
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
			if (!TagFound)
			{
				try
				{
					// Create new NFC Android object
					_Intent = _Activity.Call<AndroidJavaObject>("getIntent");
					_Action = _Intent.Call<String>("getAction");

					if (_Action == "android.nfc.action.NDEF_DISCOVERED")
					{
						Debug.Log("Found NDEF tag");

						// Parcelable[]
						AndroidJavaObject[] receivedArray = _Intent.Call<AndroidJavaObject[]>
							("getParcelableArrayExtra", new AndroidJavaClass("android.nfc.NfcAdapter")
							.GetStatic<string>("EXTRA_NDEF_MESSAGES"));

						if (receivedArray != null)
						{
							ProgressText.text = string.Empty;
							// casted to NdefMessage
							AndroidJavaObject receivedMsg = receivedArray[0];
							// NdefRecord[]
							AndroidJavaObject[] attachedRecords = receivedMsg.Call<AndroidJavaObject[]>("getRecords");

							foreach (AndroidJavaObject record in attachedRecords) // NdefRecord
							{
								byte[] data = record.Call<byte[]>("getPayload");
								if (Encoding.UTF8.GetString(data) == "us.shsrobotics.scoutingapp")
									continue;

								MD5 md5 = MD5.Create();
								byte[] hash = md5.ComputeHash(data, 0, data.Length - 16);
								bool flag = true;
								for (int i = 0; i < 16; i++)
								{
									if (hash[i] != data[data.Length - 16 + i])
									{
										flag = false;
										break;
									}
								}
								if (flag)
								{
									MemoryStream stream = new MemoryStream(data.Length - 16);
									stream.Write(data, 0, data.Length - 16);
									stream.Position = 0;
									DataStorage.Instance.DeserializeData(stream);
									ProgressText.text += "Import successful!";
									TagFound = true;
								}
								else
								{
									ProgressText.text = "Error. Please try again.";
								}
							}
						}
					}
					else
					{
						ProgressText.text = "No NFC tag found!";
						return;
					}
				}
				catch (Exception ex)
				{
					Debug.Log(ex.Message + "\n" + ex.StackTrace);
					ProgressText.text = ex.Message;
				}
			}
		}
	}

}
