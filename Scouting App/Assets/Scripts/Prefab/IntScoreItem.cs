using UnityEngine;
using UnityEngine.UI;

public class IntScoreItem : MonoBehaviour
{
	public Text ValueText;

	public int Maximum = 1000;
	public int Minimum = -1000;

	[SerializeField]
	public IntEvent OnValueChange;

	private int _Value;

	public int Value
	{
		get
		{
			return _Value;
		}
		set
		{
			_Value = value;
			ValueUpdated();
		}
	}

	private void ValueUpdated()
	{
		ValueText.text = _Value.ToString();
		OnValueChange.Invoke(_Value);
	}

	public void Increment()
	{
		_Value++;
		if (_Value > Maximum)
			_Value = Maximum;

		ValueUpdated();
	}

	public void Decrement()
	{
		_Value--;
		if (_Value < Minimum)
			_Value = Minimum;

		ValueUpdated();
	}
}
