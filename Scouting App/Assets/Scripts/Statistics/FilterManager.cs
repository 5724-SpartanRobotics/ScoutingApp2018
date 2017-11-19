using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FilterManager : MonoBehaviour
{

	public Button sortNameBtn;
	public Button sortNumBtn;

	private bool nameActive = false;
	private bool numActive = false;

	public Color unselected;
	public Color selected;

	private TableHandler tableScript;

	private void Start()
	{
		//sortNameBtn.onClick.AddListener(() => OnToggle(sortNameBtn.gameObject));
		//sortNameBtn.onClick.AddListener(() => OnToggle(sortNameBtn.gameObject));
		
		tableScript = GameObject.Find("TeamListContent").GetComponent<TableHandler>();
	}

	public void OnToggle(GameObject btnColor)
	{
		if (btnColor == sortNameBtn.gameObject)
		{
			if (nameActive == false)
			{
				btnColor.GetComponent<Image>().color = selected;
				nameActive = true;

				Debug.Log("SELECTED");

				tableScript.SortByName();
			}
			else if (nameActive == true)
			{
				btnColor.GetComponent<Image>().color = unselected;
				nameActive = false;

				Debug.Log("UNSELECTED");

				//ETHAN UNSELECT FILTER CODE HERE

			}
		}
		else if (btnColor == sortNumBtn.gameObject)
		{
			if (numActive == false)
			{
				btnColor.GetComponent<Image>().color = selected;
				numActive = true;

				Debug.Log("SELECTED");

				tableScript.SortByNumber();

			}
			else if (numActive == true)
			{
				btnColor.GetComponent<Image>().color = unselected;
				numActive = false;

				Debug.Log("UNSELECTED");

				//ETHAN UNSELECT FILTER CODE HERE

			}
		}
	}

}
