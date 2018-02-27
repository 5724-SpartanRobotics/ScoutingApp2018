using System;
using UnityEngine;
using UnityEngine.UI;

public class CirclePercent : MonoBehaviour
{
	public Image CircleImage;
	public Text PercentText;
	private float _AnimState;
	public float Percent;
	private float _LastPercent;
	private bool _Animating = false;

	private void RedrawCircle(float value)
	{
		CircleImage.color = new Color(1 - value, value, 0F);
		CircleImage.fillAmount = value;
	}

	private void Update()
	{
		if (_LastPercent != Percent)
		{
			_LastPercent = Percent;
			PercentText.text = (Math.Round(Percent * 1000) / 10).ToString() + "%";
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

	public void AnimateToValue(float percent)
	{
		_Animating = true;
		_AnimState = Percent;
		Percent = percent;
	}
}
