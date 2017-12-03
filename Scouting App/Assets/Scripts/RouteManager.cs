using UnityEngine;
using UnityEngine.SceneManagement;

public class RouteManager : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void LoadStats()
	{
		SceneManager.LoadScene("stats");
	}

	public void LoadMain()
	{
		SceneManager.LoadScene("main");
	}

	public void LoadExport()
	{
		SceneManager.LoadScene("export");
	}

	public void LoadImport()
	{
		Debug.Log("IMPORT NOT YET IMPLEMENTED");
	}
}
