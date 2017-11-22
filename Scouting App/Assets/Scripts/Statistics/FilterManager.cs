using UnityEngine.UI;
using UnityEngine;

public class FilterManager : MonoBehaviour
{

	public Button SortNameBtn;
	public Button SortNumBtn;

	private bool nameActive = false;
	private bool numActive = false;

	public Color unselected;
	public Color selected;

	public GameObject TableObj;
	private TableHandler tableScript;

	private void Start()
	{
		tableScript = TableObj.GetComponent<TableHandler>();
		OnToggle(SortNameBtn.gameObject);
	}

	public void OnToggle(GameObject btn)
	{
		if (btn == SortNameBtn.gameObject)
		{
			if (nameActive == false)
			{
				btn.GetComponent<Image>().color = selected;
				nameActive = true;

				SortNumBtn.GetComponent<Image>().color = unselected;
				numActive = false;

				Debug.Log("NAME SELECTED");

				tableScript.SortByName();
			}
		}
		else if (btn == SortNumBtn.gameObject)
		{
			if (numActive == false)
			{
				btn.GetComponent<Image>().color = selected;
				numActive = true;

				SortNameBtn.GetComponent<Image>().color = unselected;
				nameActive = false;

				Debug.Log("NUMBER SELECTED");

				tableScript.SortByNumber();

			}
		}
	}

}
