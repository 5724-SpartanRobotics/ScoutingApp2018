using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiChoiceScoreItem : MonoBehaviour
{
	public List<GameObject> Options;
	public Color SelectedColor;
	public Color DeselectedColor;
	public int Value { get; private set; }

	[SerializeField]
	public IntEvent OnOptionSelected;

	private void Start()
	{
		SelectOption(Value);
	}

	public void SelectOption(int option)
	{
		Value = option;
		foreach (GameObject opt in Options)
			opt.GetComponent<Image>().color = DeselectedColor;

		Options[option].GetComponent<Image>().color = SelectedColor;

		OnOptionSelected.Invoke(option);
	}
}
