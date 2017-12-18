using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RouteManager : MonoBehaviour
{
	public const string MAIN_SCENE = "main",
		STATS_SCENE = "stats",
		QR_EXPORT_SCENE = "qr_export",
		QR_IMPORT_SCENE = "qr_import";

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
		SceneManager.LoadScene(QR_EXPORT_SCENE);
	}

	public void LoadImport()
	{
		SceneManager.LoadScene(QR_IMPORT_SCENE);
	}

	static readonly Dictionary<string, string> _PrevSceneMap = new Dictionary<string, string>()
	{
		{ MAIN_SCENE, null },
		{ STATS_SCENE, MAIN_SCENE },
		{ QR_EXPORT_SCENE, STATS_SCENE },
		{ QR_IMPORT_SCENE, STATS_SCENE }
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
#if UNITY_ANDROID
		if (Application.platform == RuntimePlatform.Android)
		{
			// Escape is back button on Android
			if (Input.GetKey(KeyCode.Escape))
				NavigateBack();
		}
#endif
	}
}
