using ScoutingApp.GameData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewTeam : MonoBehaviour
{
	private Team _Team;
	public TextValueItem AvgStatTemplate;
	public Text TeamName;
	public Text TeamNumber;
	public Text Comments;

	public Dropdown MatchPicker;
	public Text MatchComments;
	public TextValueItem MatchStatTemplate;

	public GameObject ConfirmOverridePanel;
	public GameObject OverrideButton;
	public Image BrokenCheckmark;
	public Sprite CheckSprite;
	public Sprite XSprite;

	void Start()
	{
		int teamNum = PlayerPrefs.GetInt("currentTeam");

		if (!DataStorage.Instance.Teams.Contains(teamNum))
			return;

		_Team = DataStorage.Instance.Teams[teamNum];

		if (!string.IsNullOrEmpty(_Team.TeamName))
			TeamName.text = _Team.TeamName;
		TeamNumber.text = "Team " + _Team.TeamNum.ToString();

		AvgStatTemplate.gameObject.SetActive(true);
		TextValueItem autoMoveAvg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
		TextValueItem autoItem1Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
		TextValueItem autoItem2Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
		TextValueItem item1Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
		TextValueItem item2Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
		TextValueItem endgameAvg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
		AvgStatTemplate.gameObject.SetActive(false);

		autoMoveAvg.KeyText.text = "% of the time moves in auto: ";
		autoMoveAvg.ValueText.text = ToPercent(_Team.MovedInAutoAvg);
		autoItem1Avg.KeyText.text = "Average balls scored in auto: ";
		autoItem1Avg.ValueText.text = ToRoundStr(_Team.AutoItem1Avg);
		autoItem2Avg.KeyText.text = "% times auto gear scored: ";
		autoItem2Avg.ValueText.text = ToPercent(_Team.AutoItem2Avg);

		item1Avg.KeyText.text = "Average balls scored: ";
		item1Avg.ValueText.text = ToRoundStr(_Team.Item1Avg);
		item2Avg.KeyText.text = "Average gears scored: ";
		item2Avg.ValueText.text = ToRoundStr(_Team.Item2Avg);
		endgameAvg.KeyText.text = "% times climbed rope: ";
		endgameAvg.ValueText.text = ToPercent(_Team.EndgameAvg);

		Comments.text = _Team.Comments;

		MatchPicker.ClearOptions();
		List<string> options = new List<string>(_Team.Matches.Count);
		foreach (Match m in _Team.Matches)
		{
			string s = m.MatchNum.ToString();
			if (m.Obsolete)
				s += " (Obsolete)";
			options.Add(s);
		}
		MatchPicker.AddOptions(options);

		UpdateBrokenCheck();
		MatchSelected(0);
	}

	public void ClickOverride(bool cancel)
	{
		Debug.Log("Functioning status override requested for team " + _Team.TeamNum.ToString());
		if (!ConfirmOverridePanel.activeInHierarchy)
		{
			OverrideButton.SetActive(false);
			ConfirmOverridePanel.SetActive(true);
		}
		else
		{
			if (!cancel)
			{
				Debug.Log("Override Confirmed!");
				_Team.OverrideBroken();
				UpdateBrokenCheck();
			}
			ConfirmOverridePanel.SetActive(false);
			OverrideButton.SetActive(true);
		}
	}

	private void UpdateBrokenCheck()
	{
		if (_Team.NotBroken)
		{
			BrokenCheckmark.sprite = CheckSprite;
			BrokenCheckmark.color = Color.green;
		}
		else
		{
			BrokenCheckmark.sprite = XSprite;
			BrokenCheckmark.color = Color.red;
		}
	}

	public void MatchSelected(int index)
	{
		foreach (Transform child in MatchStatTemplate.transform.parent)
			if (child.gameObject != MatchStatTemplate.gameObject)
				Destroy(child.gameObject);

		Match match = _Team.Matches[index];

		MatchStatTemplate.gameObject.SetActive(true);
		TextValueItem autoMove = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem autoItem1 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem autoItem2 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem item1 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem item2 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem endgame = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		MatchStatTemplate.gameObject.SetActive(false);

		autoMove.KeyText.text = "Moved in auto: ";
		autoMove.ValueText.text = match.MovedInAuto.ToString();
		autoItem1.KeyText.text = "Balls scored in auto: ";
		autoItem1.ValueText.text = match.AutoScoreItem1.ToString();
		autoItem2.KeyText.text = "Auto gear scored: ";
		autoItem2.ValueText.text = match.AutoScoreItem2.ToString();

		item1.KeyText.text = "Balls scored: ";
		item1.ValueText.text = match.ScoreItem1.ToString();
		item2.KeyText.text = "Gears scored: ";
		item2.ValueText.text = match.ScoreItem2.ToString();
		endgame.KeyText.text = "Climbed rope: ";
		endgame.ValueText.text = match.Endgame.ToString();

		MatchComments.text = _Team.Matches[index].Comments;
	}

	private static string ToPercent(double d)
	{
		return (Math.Round(d * 1000) / 10) + "%";
	}

	private static string ToRoundStr(double d)
	{
		return (Math.Round(d * 10) / 10).ToString();
	}
}
