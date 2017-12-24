using System;
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
			ProgressText.text = "There was an error. We have no idea how you got to this page.";
		else
			_Activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
	}

	// Update is called once per frame
	void Update()
	{
		if (Options.Inst.IsNFCEnabled)
		{
			if (!TagFound)
			{
				try
				{
					// Create new NFC Android object
					_Intent = _Activity.Call<AndroidJavaObject>("getIntent");
					_Action = _Intent.Call<String>("getAction");
					if (_Action == "android.nfc.action.NDEF_DISCOVERED")
					{
						Debug.Log("Tag of type NDEF");
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
								ProgressText.text += Convert.ToBase64String(data);
							}
						}
					}
					else if (_Action == "android.nfc.action.TECH_DISCOVERED")
					{
						Debug.Log("TAG DISCOVERED");
						// Get ID of tag
						AndroidJavaObject mNdefMessage = _Intent.Call<AndroidJavaObject>("getParcelableExtra", "android.nfc.extra.TAG");
						if (mNdefMessage != null)
						{
							byte[] payLoad = mNdefMessage.Call<byte[]>("getId");
							string text = Convert.ToBase64String(payLoad);
							ProgressText.text = text;
							TagID = text;
						}
						else
						{
							ProgressText.text = "No ID found !";
						}
						TagFound = true;
						return;
					}
					else if (_Action == "android.nfc.action.TAG_DISCOVERED")
					{
						Debug.Log("This type of tag is not supported !");
					}
					else
					{
						ProgressText.text = "No tag...";
						return;
					}
				}
				catch (Exception ex)
				{
					string text = ex.Message;
					ProgressText.text = text;
				}
			}
		}
	}
}
