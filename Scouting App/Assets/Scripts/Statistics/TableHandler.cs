using ScoutingApp.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableHandler : MonoBehaviour
{
	public Table TableObj;
	public GameObject TeamItemPrefab;
	public GameObject ContentPanel;
	public Sprite XSprite;
	public Sprite CheckSprite;

	// Use this for initialization
	void Start()
	{
		System.Random rand = new System.Random();
		int numTeams = rand.Next(100) + 10;

		for (int i = 0; i < numTeams; i++)
			DataStorage.Instance.Teams.Add(new Team(rand));

		TableObj = new Table();
		RedrawList();
	}

	public void RedrawList()
	{
		foreach (Transform child in ContentPanel.transform)
			Destroy(child.gameObject);

		foreach (TableRow row in TableObj.Rows)
		{
			if (!row.IsVisible)
				continue;

			GameObject newTeam = Instantiate(TeamItemPrefab) as GameObject;
			TeamListItem item = newTeam.GetComponent<TeamListItem>();
			item.TeamName.text = row.Team.TeamName;
			item.TeamNum.text = row.Team.TeamNum.ToString();
			if (row.NotBroken)
			{
				item.TeamStatus.sprite = CheckSprite;
				item.TeamStatus.color = Color.green;
			}
			else
			{
				item.TeamStatus.sprite = XSprite;
				item.TeamStatus.color = Color.red;
			}

			item.transform.SetParent(ContentPanel.transform, false);
			item.transform.localScale = Vector3.one;
		}
	}

	public void SortByName()
	{
		TableObj.SortByName();
		RedrawList();
	}

	public void SortByNumber()
	{
		TableObj.SortByNum();
		RedrawList();
	}

	public class Table
	{
		public List<TableRow> Rows;

		public Table()
		{
			Rows = new List<TableRow>();

			foreach (Team team in DataStorage.Instance.Teams)
				Rows.Add(new TableRow(team));
		}

		public void SortByName(bool reverse = false)
		{
			Rows.Sort((row1, row2) =>
			{
				if (row1.NotBroken == row2.NotBroken)
					return row1.Team.TeamName.CompareTo(row2.Team.TeamName);
				else
					return row1.NotBroken ? -1 : 1;
			});
			if (reverse)
				Rows.Reverse();
		}

		public void SortByNum(bool reverse = false)
		{
			Rows.Sort((row1, row2) =>
			{
				if (row1.NotBroken == row2.NotBroken)
					return row1.Team.TeamNum.CompareTo(row2.Team.TeamNum);
				else
					return row1.NotBroken ? -1 : 1;
			});
			if (reverse)
				Rows.Reverse();
		}

		public void DebugPrint()
		{
			string s = string.Empty;
			foreach (TableRow row in Rows)
				s += row.Columns.Aggregate((current, next) => current + "\t" + next) + "\n";
			Debug.Log(s);
		}
	}

	public class TableRow
	{
		public Team Team;

		private string ToPercent(double d)
		{
			return (Math.Round(d * 1000) / 10) + "%";
		}

		private string ToRoundStr(double d)
		{
			return (Math.Round(d * 10) / 10).ToString();
		}

		public string[] Columns
		{
			get
			{
				return new string[] { Team.TeamName, Team.TeamNum.ToString(), ToRoundStr(BallAvg),
					ToRoundStr(AutoBallAvg), ToPercent(AutoGearAvg), ToRoundStr(BallAvg),
					ToRoundStr(GearAvg), ToRoundStr(ClimbAvg), NotBroken ? "\u2714" : "X" };
				// I'm just using characters right now for the check / X mark because it is easy.
				// We can change it to use images later.
			}
		}

		public double AutoBallAvg { get; private set; }
		public double AutoGearAvg { get; private set; }
		public double BallAvg { get; private set; }
		public double GearAvg { get; private set; }
		public double ClimbAvg { get; private set; }
		public bool NotBroken { get; private set; }
		public bool IsVisible { get; set; } = true;

		public TableRow(Team team)
		{
			Team = team;

			AutoBallAvg = team.Matches.Average(match => match.AutoBallScore);
			AutoGearAvg = team.Matches.Average(match => match.AutoGearScore ? 1 : 0);
			BallAvg = team.Matches.Average(match => match.BallScore);
			GearAvg = team.Matches.Average(match => match.GearScore);

			ClimbAvg = team.Matches.Average(match => match.ClimbedRope ? 1 : 0);

			NotBroken = team.Matches.OrderByDescending(match => match.MatchNum).First().WorksAtEnd;
		}
	}
}
