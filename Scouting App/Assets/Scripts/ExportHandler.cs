using ScoutingApp.GameData;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ScoutingApp;
//if UNITY_STANDALONE || UNITY_WEBGL
//using System.IO.Compression;
//endif

public class ExportHandler : MonoBehaviour
{
	public Text ProgressText;
	public RawImage QRImage;
	Texture2D[] _Textures;
	private int _PageIdx = 0;

	public void Start()
	{
		DataStorage.Instance.Teams.Clear();
		System.Random rng = new System.Random();
		int numTeams = rng.Next(10) + 3;

		for (int i = 0; i < numTeams; i++)
			DataStorage.Instance.Teams.Add(new Team(rng));

		MemoryStream memStream = new MemoryStream();
		DataStorage.Instance.SerializeData(memStream);
		memStream.Position = 0;

		_Textures = ImageUtils.EncodeToQRCodes(memStream, 25); // TODO make qrVersion configurable

		UpdateStuff();
	}

	private void UpdateStuff()
	{
		ProgressText.text = $"Page {_PageIdx + 1}/{_Textures.Length}";
		QRImage.texture = _Textures[_PageIdx];
	}

	public void NextPage()
	{
		if (_PageIdx + 1 < _Textures.Length)
			_PageIdx++;
		UpdateStuff();
	}

	public void PreviousPage()
	{
		if (_PageIdx > 0)
			_PageIdx--;
		UpdateStuff();
	}

	public void OnGUI()
	{
		//GUI.DrawTexture(new Rect(0, (int)(Screen.height * 0.18F), Screen.width, Screen.width), _Textures[_PageIdx], ScaleMode.StretchToFill, false);
	}
}
