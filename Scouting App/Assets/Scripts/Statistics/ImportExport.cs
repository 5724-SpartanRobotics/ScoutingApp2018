using ScoutingApp;
using System;
using System.IO;
using UnityEngine;
using static TableHandler;

public class ImportExport : MonoBehaviour
{
	const ushort VERSION = 0;
	public GameObject TableObj;
	private TableHandler tableScript;

	public void Start()
	{
		tableScript = TableObj.GetComponent<TableHandler>();
	}

	public void ImportClick()
	{
		Console.WriteLine("IMPORT NOT YET IMPLEMENTED");
	}

	public void ExportClick()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(VERSION);
		writer.Write(tableScript.table.rows.Count);

		foreach (TableRow row in tableScript.table.rows)
			row.Team.Serialize(writer);

		byte[] image = ImageUtils.EncodeToImage(stream.ToArray());

		string picDir = Path.Combine(Application.persistentDataPath, "ScoutingAppSaves");
		if (!Directory.Exists(picDir))
			Directory.CreateDirectory(picDir);

		string fileName = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss.ff tt") + "_scout.png";
		string filePath = Path.Combine(picDir, fileName);

		File.WriteAllBytes(filePath, image);

		// Doesn't work yet
		//if (Application.platform == RuntimePlatform.Android)
		//	File.Move(filePath, "/mnt/sdcard/Pictures/" + fileName);
	}
}
