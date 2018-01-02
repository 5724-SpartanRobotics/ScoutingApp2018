using ScoutingApp.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ViewTeam : MonoBehaviour
{
	private Team _Team;
	public Text TeamName;
	public Text TeamNumber;
	public TextValueItem AvgTemplate;
	public Text Comments;

	public Dropdown MatchPicker;
	public Text MatchComments;

	void Start()
	{
		int teamNum = PlayerPrefs.GetInt("currentTeam");

		if (!DataStorage.Instance.Teams.Contains(teamNum))
			return;

		_Team = DataStorage.Instance.Teams[teamNum];

		if (!string.IsNullOrEmpty(_Team.TeamName))
			TeamName.text = _Team.TeamName;
		TeamNumber.text = "Team " + _Team.TeamNum.ToString();
		AvgTemplate.gameObject.SetActive(true);
		TextValueItem autoMoveAvg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem autoItem1Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem autoItem2Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem item1Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem item2Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem endgameAvg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		AvgTemplate.gameObject.SetActive(false);

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
			options.Add(m.MatchNum.ToString());
		MatchPicker.AddOptions(options);

		MatchSelected(0);
	}

	public void MatchSelected(int index)
	{
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
