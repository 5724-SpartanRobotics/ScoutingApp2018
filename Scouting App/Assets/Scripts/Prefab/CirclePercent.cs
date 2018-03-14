using System;
using UnityEngine;
using UnityEngine.UI;

public class CirclePercent : MonoBehaviour
{
	public Image CircleImage;
	public Text PercentText;
	private float _AnimState;
	public float Percent;
	private bool _IsDirty = false;
	private bool _Animating = false;
	private string _NonPercentText = null;

	private void RedrawCircle(float value)
	{
		CircleImage.color = new Color(1 - value, value, 0F);
		CircleImage.fillAmount = value;
	}

	private void Update()
	{
		if (_IsDirty)
		{
			if (_NonPercentText == null)
				PercentText.text = (Math.Round(Percent * 1000) / 10).ToString() + "%";
			else
				PercentText.text = _NonPercentText;

			RedrawCircle(Percent);
		}

		if (_Animating)
		{
			if (Math.Round(_AnimState * 10000) / 10000 == Percent)
			{
				_Animating = false;
				// In case we go over too much or something I guess
				RedrawCircle(Percent);
			}
			else
			{
				float diff = Percent - _AnimState;
				float percentDiff = Math.Abs(diff / Percent);
				_AnimState += diff / (15 + ((percentDiff - 0.5F) * 20));
				RedrawCircle(_AnimState);
			}
		}
	}

	/// <summary>
	/// Animates to the given value and sets the text to <paramref name="text"/>,
	/// or <paramref name="percent"/> if <paramref name="text"/> is null.
	/// </summary>
	/// <param name="percent">A Value between 0 and 1 (inclusive) that represents a percentage.</param>
	/// <param name="text">If not null, the text to display in the center of the circle instead of the percent</param>
	public void AnimateToValue(float percent, string text = null)
	{
		_IsDirty = true;
		_Animating = true;
		_AnimState = Percent;
		Percent = percent;
		_NonPercentText = text;
	}
}
