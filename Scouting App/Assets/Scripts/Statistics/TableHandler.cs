using ScoutingApp.GameData;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TableHandler : MonoBehaviour
{
	public Table TableObj;
	public GameObject TeamItemPrefab;
	public GameObject ContentPanel;
	public Sprite XSprite;
	public Sprite CheckSprite;
	private bool _OnlyFinalists = false;

	// Use this for initialization
	void Start()
	{
		TableObj = new Table();
		RedrawList();
	}

	public void ToggleShowOnlyFinalists()
	{
		_OnlyFinalists = !_OnlyFinalists;
		RedrawList();
	}

	public void RedrawList()
	{
		foreach (Transform child in ContentPanel.transform)
			Destroy(child.gameObject);

		foreach (TableRow row in TableObj.Rows)
		{
			if (!row.IsVisible || (_OnlyFinalists && !row.Team.IsFinalist))
				continue;
			
			GameObject newTeam = Instantiate(TeamItemPrefab) as GameObject;

			TeamListItem item = newTeam.GetComponent<TeamListItem>();
			item.TeamNum.text = row.Team.TeamNum.ToString();
			item.OverallRating.text = ToRoundStr(row.Team.GenerateRating(false));
			item.ClimbRating.text = ToRoundStr(row.Team.GetEndgameRating(false));
			item.BoxRating.text = ToRoundStr(row.Team.GetBoxRating(false));

			if (row.Team.NotBroken)
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

	public void SortByScore()
	{
		TableObj.SortByScore();
		RedrawList();
	}

	public void SortByClimb()
	{
		TableObj.SortByClimb();
		RedrawList();
	}

	public void SortByBoxes()
	{
		TableObj.SortByBoxes();
		RedrawList();
	}

	public void SortByNumber()
	{
		TableObj.SortByNumber();
		RedrawList();
	}

	private static string ToRoundStr(double d)
	{
		return (Math.Round(d * 10) / 10).ToString();
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

		public void SortByScore()
		{
			Rows.Sort((row2, row1) => row1.Team.CompareTo(row2.Team));
		}

		public void SortByClimb()
		{
			Rows.Sort((row2, row1) => row1.Team.GetEndgameRating().CompareTo(row2.Team.GetEndgameRating()));
		}

		public void SortByBoxes()
		{
			Rows.Sort((row2, row1) => row1.Team.GetBoxRating().CompareTo(row2.Team.GetBoxRating()));
		}

		public void SortByNumber()
		{
			Rows.Sort((row1, row2) =>
			{
				if (row1.Team.NotBroken == row2.Team.NotBroken)
					return row1.Team.TeamNum.CompareTo(row2.Team.TeamNum);
				else
					return row1.Team.NotBroken ? -1 : 1;
			});
		}
	}

	public class TableRow
	{
		public Team Team;
		public bool IsVisible { get; set; } = true;

		public TableRow(Team team)
		{
			Team = team;
		}
	}
}
