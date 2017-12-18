using ScoutingApp;
using ScoutingApp.GameData;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImportHandler : MonoBehaviour
{
	public RawImage RenderImage;
	public Text StatusText;
	private WebCamTexture _WCT;
	private Texture2D _ScanImg;
	public RouteManager RouteManager;

	// Use this for initialization
	void Start()
	{
		Application.RequestUserAuthorization(UserAuthorization.WebCam).completed += (obj) =>
		{
			_WCT = new WebCamTexture(480, 640)
			{
				// Set camera filter modes for a smoother looking image
				filterMode = FilterMode.Trilinear
			};

			RenderImage.texture = _WCT;
			_WCT.Play();
		};
	}

	// Called once per frame
	void Update()
	{
		// I got this off of the Unity forms. I realize it's buggy but it's as far as I've gotten so far.
		// Source: https://answers.unity.com/questions/773464/webcamtexture-correct-resolution-and-ratio.html#answer-1148424

		if (_WCT.width < 100)
		{
			Debug.Log("Still waiting another frame for correct info...");
			return;
		}
		if (_ScanImg == null)
			_ScanImg = new Texture2D(_WCT.width, _WCT.height);

		// change as user rotates iPhone or Android:

		int cwNeeded = _WCT.videoRotationAngle;
		// Unity helpfully returns the _clockwise_ twist needed
		// guess nobody at Unity noticed their product works in counterclockwise:
		int ccwNeeded = -cwNeeded;


		// IF the image needs to be mirrored, it seems that it
		// ALSO needs to be spun. Strange: but true.
		if (_WCT.videoVerticallyMirrored) ccwNeeded += 180;

		// you'll be using a UI RawImage, so simply spin the RectTransform
		RenderImage.transform.localEulerAngles = new Vector3(0f, 0f, ccwNeeded);

		float videoRatio = (float)_WCT.height / _WCT.width;





		/*if (ccwNeeded != 0 &&
					 ccwNeeded % 90 == 0)
		{
			transform.parent.localScale = new Vector3(videoRatio, videoRatio, 1);
		}
		else
		{
			transform.parent.localScale = new Vector3(1, 1, 1);
		}*/



		// you'll be using an AspectRatioFitter on the Image, so simply set it
		RenderImage.GetComponent<AspectRatioFitter>().aspectRatio = videoRatio;

		// alert, the ONLY way to mirror a RAW image, is, the uvRect.
		// changing the scale is completely broken.
		if (_WCT.videoVerticallyMirrored)
			RenderImage.uvRect = new Rect(1, 0, -1, 1);  // means flip on vertical axis
		else
			RenderImage.uvRect = new Rect(0, 0, 1, 1);  // means no flip

		// devText.text =
		//  videoRotationAngle+"/"+ratio+"/"+wct.videoVerticallyMirrored;

		ScanCode();
	}

	MemoryStream _DataStream = new MemoryStream();
	int _LastCodeScanned = -1;
	int _TotalCodes = 0;
	int _NumCodesScanned = 0;

	private void ResetScanning()
	{
		_DataStream.SetLength(0);
		_LastCodeScanned = -1;
		_TotalCodes = 0;
		_NumCodesScanned = 0;
		StatusText.text = "";
	}

	public void ScanCode()
	{
		if (_ScanImg == null)
			return;

		_ScanImg.SetPixels(_WCT.GetPixels());
		_ScanImg.Apply();


		int qrIdx;
		int numCodes;
		byte[] data = ImageUtils.DecodeQRCode(_ScanImg, out qrIdx, out numCodes);

		if (data != null)
		{
			if (_TotalCodes == 0)
			{
				_TotalCodes = numCodes;
			}
			else if (_TotalCodes != numCodes)
			{
				ResetScanning();
				StatusText.color = Color.red;
				StatusText.text = "Error: code not from the same sequence.";
			}

			// Just keep waiting for the next code if they haven't pressed the next button yet.
			if (qrIdx == _LastCodeScanned)
				return;

			if (qrIdx != _LastCodeScanned + 1)
			{
				StatusText.color = Color.red;
				// +2 because +1 to make it a code number, not an index, and 1 for current instead of last.
				StatusText.text = $"Error: wrong code number. Looking for code number {_LastCodeScanned + 2}, not {qrIdx + 1}";
			}
			else
			{
				_NumCodesScanned++;
				_LastCodeScanned = qrIdx;
				StatusText.color = Color.blue;
				StatusText.text = $"Codes scanned: {_NumCodesScanned}/{_TotalCodes}";
				_DataStream.Write(data, 0, data.Length);

				if (_NumCodesScanned == _TotalCodes)
				{
					StatusText.color = Color.green;
					StatusText.text = $"Import complete!";
					_DataStream.Position = 0;
					try
					{
						DataStorage.Instance.DeserializeData(_DataStream);
					}
					catch (Exception e)
					{
						StatusText.color = Color.red;
						StatusText.text = e.Message + e.StackTrace;
					}
					RouteManager.NavigateBack();
				}
			}
		}
	}
}
