using ScoutingApp.GameData;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ViewTeam : MonoBehaviour
{
	public Text TeamName;
	public Text TeamNumber;
	public Text AvgTemplate;

	void Start()
	{
		int num = PlayerPrefs.GetInt("currentTeam");

		if (!DataStorage.Instance.Teams.Contains(num))
			return;

		Team team = DataStorage.Instance.Teams[num];

		if (!string.IsNullOrEmpty(team.TeamName))
			TeamName.text = team.TeamName;
		TeamNumber.text = "Team " + team.TeamNum.ToString();
		Text autoMoveAvg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		Text autoItem1Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		Text autoItem2Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		Text item1Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		Text item2Avg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);
		Text endgameAvg = Instantiate(AvgTemplate, AvgTemplate.transform.parent);

		autoMoveAvg.text = "Moves in auto " + ToPercent(team.MovedInAutoAvg) + " of the time";
		autoItem1Avg.text = "Average balls scored in auto: " + ToRoundStr(team.AutoItem1Avg);
		autoItem2Avg.text = "Scores a gear in auto " + ToPercent(team.AutoItem2Avg) + " of the time";

		item1Avg.text = "Average balls scored: " + ToRoundStr(team.Item1Avg);
		item2Avg.text = "Average gears scored: " + ToRoundStr(team.Item2Avg);
		endgameAvg.text = "Climbs the rope " + ToPercent(team.EndgameAvg) + " of the time";

		Destroy(AvgTemplate.gameObject);
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
