using UnityEngine;

public class FilterManager : MonoBehaviour
{
	public TableHandler TableScript;

	public void OnOptionSelected(int option)
	{
		if (option == 0)
			TableScript.SortByScore();
		else if (option == 1)
			TableScript.SortByClimb();
		else if (option == 2)
			TableScript.SortByBoxes();
		else
			TableScript.SortByNumber();
	}
}
