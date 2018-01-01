using ScoutingApp.GameData;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ViewTeam : MonoBehaviour
{
	public Text TeamName;
	public Text TeamNumber;
	public TextValueItem AvgTemplate;
	public Text Comments;

	void Start()
	{
		int teamNum = PlayerPrefs.GetInt("currentTeam");

		if (!DataStorage.Instance.Teams.Contains(teamNum))
			return;

		Team team = DataStorage.Instance.Teams[teamNum];

		if (!string.IsNullOrEmpty(team.TeamName))
			TeamName.text = team.TeamName;
		TeamNumber.text = "Team " + team.TeamNum.ToString();
		AvgTemplate.gameObject.SetActive(true);
		TextValueItem autoMoveAvg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem autoItem1Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem autoItem2Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem item1Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem item2Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		TextValueItem endgameAvg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		AvgTemplate.gameObject.SetActive(false);

		autoMoveAvg.KeyText.text = "% of the time moves in auto: ";
		autoMoveAvg.ValueText.text = ToPercent(team.MovedInAutoAvg);
		autoItem1Avg.KeyText.text = "Average balls scored in auto: ";
		autoItem1Avg.ValueText.text = ToRoundStr(team.AutoItem1Avg);
		autoItem2Avg.KeyText.text = "% times auto gear scored: ";
		autoItem2Avg.ValueText.text = ToPercent(team.AutoItem2Avg);

		item1Avg.KeyText.text = "Average balls scored: ";
		item1Avg.ValueText.text = ToRoundStr(team.Item1Avg);
		item2Avg.KeyText.text = "Average gears scored: ";
		item2Avg.ValueText.text = ToRoundStr(team.Item2Avg);
		endgameAvg.KeyText.text = "% times climbed rope: ";
		endgameAvg.ValueText.text = ToPercent(team.EndgameAvg);

		Comments.text = team.Comments;
		team.Matches.ForEach(x => Comments.text += "\n\nMatch " + x.MatchNum + ":\n" + x.Comments);
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
