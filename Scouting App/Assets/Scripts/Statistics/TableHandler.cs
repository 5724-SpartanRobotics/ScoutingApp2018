using ScoutingApp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableHandler : MonoBehaviour
{
	public Table table;
	public GameObject TeamItemPrefab;
	public GameObject ContentPanel;

	// Use this for initialization
	void Start()
	{
		System.Random rand = new System.Random();
		List<Team> teams = new List<Team>();
		int numTeams = rand.Next(100) + 10;

		for (int i = 0; i < numTeams; i++)
			teams.Add(new Team(rand));

		table = new Table(teams);
		RedrawList();
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void RedrawList()
	{
		foreach (Transform child in ContentPanel.transform)
			Destroy(child.gameObject);

		foreach (TableRow row in table.rows)
		{
			GameObject newTeam = Instantiate(TeamItemPrefab) as GameObject;
			TeamListItem item = newTeam.GetComponent<TeamListItem>();
			item.TeamName.text = row.Team.TeamName;
			item.TeamNum.text = row.Team.TeamNum.ToString();
			if (row.NotBroken)
			{
				item.TeamStatus.text = "\u2714";
				item.TeamStatus.color = Color.green;
			}
			else
			{
				item.TeamStatus.text = "X";
				item.TeamStatus.color = Color.red;
			}

			item.transform.parent = ContentPanel.transform;
			item.transform.localScale = Vector3.one;
		}
	}

	public void SortByName()
	{
		table.SortByName();
		RedrawList();
	}

	public void SortByNumber()
	{
		table.SortByNum();
		RedrawList();
	}

	public class Table
	{
		string[] tableHeadings = { "Team Name", "Team Number", "Climb Rope %", "Avg. Ball Score",
			"Avg. Auto Ball Score", "Avg. Gear Score", "Auto Gear Score %", "Last End Status"};
		public List<TableRow> rows;

		public Table(List<Team> teams)
		{
			rows = new List<TableRow>();

			foreach (Team team in teams)
				rows.Add(new TableRow(team));
		}

		public void SortByName(bool reverse = false)
		{
			rows.Sort((row1, row2) =>
			{
				if (row1.NotBroken == row2.NotBroken)
					return row1.Team.TeamName.CompareTo(row2.Team.TeamName);
				else
					return row1.NotBroken ? -1 : 1;
			});
			if (reverse)
				rows.Reverse();
		}

		public void SortByNum(bool reverse = false)
		{
			rows.Sort((row1, row2) =>
			{
				if (row1.NotBroken == row2.NotBroken)
					return row1.Team.TeamNum.CompareTo(row2.Team.TeamNum);
				else
					return row1.NotBroken ? -1 : 1;
			});
			if (reverse)
				rows.Reverse();
		}

		public void DebugPrint()
		{
			string s = string.Empty;
			foreach (TableRow row in rows)
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
