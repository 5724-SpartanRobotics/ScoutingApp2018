using UnityEngine.UI;
using UnityEngine;

public class FilterManager : MonoBehaviour
{

	public Button SortNameBtn;
	public Button SortNumBtn;

	private bool _NameActive = false;
	private bool _NumActive = false;

	public Color Unselected;
	public Color Selected;

	public GameObject TableObj;
	private TableHandler _TableScript;

	private void Start()
	{
		_TableScript = TableObj.GetComponent<TableHandler>();
		OnToggle(SortNameBtn.gameObject);
	}

	public void OnToggle(GameObject btn)
	{
		if (btn == SortNameBtn.gameObject)
		{
			if (_NameActive == false)
			{
				btn.GetComponent<Image>().color = Selected;
				_NameActive = true;

				SortNumBtn.GetComponent<Image>().color = Unselected;
				_NumActive = false;

				_TableScript.SortByName();
			}
		}
		else if (btn == SortNumBtn.gameObject)
		{
			if (_NumActive == false)
			{
				btn.GetComponent<Image>().color = Selected;
				_NumActive = true;

				SortNameBtn.GetComponent<Image>().color = Unselected;
				_NameActive = false;

				_TableScript.SortByNumber();

			}
		}
	}

}
