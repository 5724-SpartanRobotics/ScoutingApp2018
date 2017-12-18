using UnityEngine;
using UnityEngine.UI;
using static TableHandler;

public class SearchHandler : MonoBehaviour
{
	public TableHandler TableScript;
	public Text SearchText;
	private string _PrevText = string.Empty;

	void Update()
	{
		string newText = SearchText.text.ToLowerInvariant();

		if (_PrevText != newText)
		{
			Search(newText);
			_PrevText = newText;
		}
	}

	private void Search(string text)
	{
		foreach (TableRow row in TableScript.TableObj.Rows)
			if (row.Team.TeamName.ToLowerInvariant().Contains(text) || row.Team.TeamNum.ToString().ToLowerInvariant().Contains(text))
				row.IsVisible = true;
			else
				row.IsVisible = false;

		TableScript.RedrawList();
	}

}
