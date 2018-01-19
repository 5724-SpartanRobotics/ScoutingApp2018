using ScoutingApp.GameData;
using System.Collections.Generic;
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
			Rows.Sort();
			return; // TODO remove the code below after we finalize how we will sort
			Rows.Sort((row1, row2) =>
			{
				if (row1.Team.NotBroken == row2.Team.NotBroken)
					return row1.Team.TeamName.CompareTo(row2.Team.TeamName);
				else
					return row1.Team.NotBroken ? -1 : 1;
			});
			if (reverse)
				Rows.Reverse();
		}

		public void SortByNum(bool reverse = false)
		{
			Rows.Sort();
			return; // TODO remove the code below after we finalize how we will sort
			Rows.Sort((row1, row2) =>
			{
				if (row1.Team.NotBroken == row2.Team.NotBroken)
					return row1.Team.TeamNum.CompareTo(row2.Team.TeamNum);
				else
					return row1.Team.NotBroken ? -1 : 1;
			});
			if (reverse)
				Rows.Reverse();
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
