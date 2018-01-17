using UnityEngine;

public class MultiChoiceOption : MonoBehaviour
{
	public MultiChoiceScoreItem ParentMultiChoice;
	public int OptionId;

	public void ProcessClick()
	{
		ParentMultiChoice.SelectOption(OptionId);
	}
}
