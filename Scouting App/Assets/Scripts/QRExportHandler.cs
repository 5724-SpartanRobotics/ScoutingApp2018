using ScoutingApp.GameData;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ScoutingApp;

public class QRExportHandler : MonoBehaviour
{
	public Text ProgressText;
	public RawImage QRImage;
	Texture2D[] _Textures;
	private int _PageIdx = 0;

	public void Start()
	{
		MemoryStream memStream = new MemoryStream();
		DataStorage.Instance.SerializeData(memStream);
		memStream.Position = 0;

		_Textures = ImageUtils.EncodeToQRCodes(memStream, Options.Inst.QRVersion, Options.Inst.QRErrorCorrection);

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

}
