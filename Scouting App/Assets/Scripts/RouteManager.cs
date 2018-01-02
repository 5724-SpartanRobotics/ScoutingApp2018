using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RouteManager : MonoBehaviour
{
	public const string MAIN_SCENE = "main",
		STATS_SCENE = "stats",
		QR_EXPORT_SCENE = "qr_export",
		QR_IMPORT_SCENE = "qr_import",
		NFC_IMPORT_SCENE = "nfc_import",
		NFC_EXPORT_SCENE = "nfc_export",
		VIEW_TEAM_SCENE = "view_team",
		OPTIONS_SCENE = "options";

	public void LoadStats()
	{
		SceneManager.LoadScene(STATS_SCENE);
	}

	public void LoadMain()
	{
		SceneManager.LoadScene(MAIN_SCENE);
	}

	public void LoadExport()
	{
		if (Options.Inst.IsNFCEnabled)
			SceneManager.LoadScene(NFC_EXPORT_SCENE);
		else
			SceneManager.LoadScene(QR_EXPORT_SCENE);
	}

	public void LoadImport()
	{
		if (Options.Inst.IsNFCEnabled)
			SceneManager.LoadScene(NFC_IMPORT_SCENE);
		else
			SceneManager.LoadScene(QR_IMPORT_SCENE);
	}

	public void LoadOptions()
	{
		SceneManager.LoadScene(OPTIONS_SCENE);
	}

	static readonly Dictionary<string, string> _PrevSceneMap = new Dictionary<string, string>()
	{
		{ MAIN_SCENE, null },
		{ STATS_SCENE, MAIN_SCENE },
		{ OPTIONS_SCENE, MAIN_SCENE },
		{ QR_EXPORT_SCENE, STATS_SCENE },
		{ QR_IMPORT_SCENE, STATS_SCENE },
		{ NFC_EXPORT_SCENE, STATS_SCENE },
		{ NFC_IMPORT_SCENE, STATS_SCENE },
		{ VIEW_TEAM_SCENE, STATS_SCENE }
	};

	public void NavigateBack()
	{
		string newScene;
		if (_PrevSceneMap.TryGetValue(SceneManager.GetActiveScene().name, out newScene) && !string.IsNullOrEmpty(newScene))
			SceneManager.LoadScene(newScene);
		else
			Application.Quit();
	}

	void Update()
	{
		// Escape is back button on Android, but it is also sometimes
		// useful to have a back button on desktop in the editor.
		if (Input.GetKey(KeyCode.Escape))
			NavigateBack();
#if UNITY_ANDROID
		if (Application.platform == RuntimePlatform.Android)
		{

			bool openNFC = Options.Inst.Activity.Call<bool>("isNowNFCing");

			if (openNFC)
			{
				if (Options.Inst.IsNFCEnabled)
					LoadImport();
				else
					LoadOptions();
			}
		}
#endif
	}
}
