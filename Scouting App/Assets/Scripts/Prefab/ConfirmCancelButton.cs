using UnityEngine;
using UnityEngine.Events;

public class ConfirmCancelButton : MonoBehaviour
{
	public GameObject ConfirmCancelPanel;
	public GameObject MainButton;
	public UnityEvent OnConfirm;
	public UnityEvent OnCancel;

	public void Click(bool cancel)
	{
		if (MainButton.activeInHierarchy)
		{
			MainButton.SetActive(false);
			ConfirmCancelPanel.SetActive(true);
		}
		else
		{
			if (!cancel)
			{
				OnConfirm.Invoke();
			}
			else
			{
				OnCancel.Invoke();
			}
			ConfirmCancelPanel.SetActive(false);
			MainButton.SetActive(true);
		}
	}
}
