using UnityEngine;
using UnityEngine.UI;
using static TableHandler;

public class SearchHandler : MonoBehaviour
{
	private TableHandler tableScript;
	public GameObject TableObj;
	public GameObject SearchText;
	private Text searchTextComp;
	private string prevText = string.Empty;

	void Start()
	{
		tableScript = TableObj.GetComponent<TableHandler>();
		searchTextComp = SearchText.GetComponent<Text>();
	}

	void Update()
	{
		string newText = searchTextComp.text.ToLowerInvariant();

		if (prevText != newText)
		{
			Search(newText);
			prevText = newText;
		}

	}

	private void Search(string text)
	{
		foreach (TableRow row in tableScript.TableObj.Rows)
			if (!row.Team.TeamName.ToLowerInvariant().Contains(text) && !row.Team.TeamNum.ToString().ToLowerInvariant().Contains(text))
				row.IsVisible = false;
			else
				row.IsVisible = true;

		tableScript.RedrawList();
	}

}
