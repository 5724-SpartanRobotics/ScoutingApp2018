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
	const ushort VERSION = 0;
	public GameObject TextProgress;
	private Text _ProgressText;
	Texture2D[] _Textures;
	private int _PageIdx = 0;

	public void Start()
	{
		_ProgressText = TextProgress.GetComponent<Text>();
		DataStorage.Instance.Teams.Clear();
		System.Random rng = new System.Random();
		int numTeams = rng.Next(100) + 10;

		for (int i = 0; i < numTeams; i++)
			DataStorage.Instance.Teams.Add(new Team(rng));


		DataStorage dataStorage = DataStorage.Instance;
		MemoryStream uncompressed = new MemoryStream();
		MemoryStream streamToUse = new MemoryStream();

		using (BinaryWriter writer = new BinaryWriter(uncompressed))
		{
			MemoryStream compressed = new MemoryStream();
			byte[] uncompressedBytes;
			byte[] compressedBytes;

			writer.Write(VERSION);
			writer.Write(dataStorage.Teams.Count);

			foreach (Team team in dataStorage.Teams)
				team.Serialize(writer);

			uncompressedBytes = uncompressed.ToArray();

			compressedBytes = uncompressedBytes;
			if (uncompressedBytes.Length < compressedBytes.Length)
			{
				compressed.Position = 0;
				streamToUse.WriteByte(1);
				compressed.CopyTo(streamToUse);
			}
			else
			{
				uncompressed.Position = 0;
				streamToUse.WriteByte(0);
				uncompressed.CopyTo(streamToUse);
			}
		}

		streamToUse.Position = 0;
		_Textures = ImageUtils.EncodeToQRCodes(streamToUse);

		UpdateText();
	}

	private void UpdateText()
	{
		_ProgressText.text = $"Page {_PageIdx + 1}/{_Textures.Length}";
	}

	public void NextPage()
	{
		if (_PageIdx + 1 < _Textures.Length)
			_PageIdx++;
		UpdateText();
	}

	public void PreviousPage()
	{
		if (_PageIdx > 0)
			_PageIdx--;
		UpdateText();
	}

	public void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, Screen.height * 0.18F, Screen.width, Screen.width), _Textures[_PageIdx]);
	}
}


