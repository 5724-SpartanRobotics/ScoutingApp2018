using ScoutingApp.GameData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViewTeam : MonoBehaviour
{
	public RouteManager RouteManager;

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

	public Text FinalistText;

	public Text ExcludeButtonText;

	void Start()
	{
		int teamNum = PlayerPrefs.GetInt("currentTeam");

		if (!DataStorage.Instance.Teams.Contains(teamNum))
			return;

		_Team = DataStorage.Instance.Teams[teamNum];

		if (!string.IsNullOrEmpty(_Team.TeamName))
			TeamName.text = _Team.TeamName;
		TeamNumber.text = "Team " + _Team.TeamNum.ToString();

		if (_Team.AvgMatches.Count != 0)
		{
			AvgStatTemplate.gameObject.SetActive(true);
			TextValueItem autoMoveAvg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem autoItem1Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem autoItem2Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem autoItem3Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem item1Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem item2Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem item3Avg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem defenseAvg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem parkAvg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			TextValueItem endgameAvg = Instantiate(AvgStatTemplate, AvgStatTemplate.transform.parent);
			AvgStatTemplate.gameObject.SetActive(false);

			autoMoveAvg.KeyText.text = "% time crosses baseline in auto: ";
			autoMoveAvg.ValueText.text = ToPercent(_Team.MovedInAutoAvg);
			autoItem1Avg.KeyText.text = "Auto scale score average: ";
			autoItem1Avg.ValueText.text = ToRoundStr(_Team.AutoItem1Avg);
			autoItem2Avg.KeyText.text = "Auto alliance switch score average: ";
			autoItem2Avg.ValueText.text = ToRoundStr(_Team.AutoItem2Avg);
			autoItem3Avg.KeyText.text = "Auto vault score average: ";
			autoItem3Avg.ValueText.text = ToRoundStr(_Team.AutoItem3Avg);

			item1Avg.KeyText.text = "Average scale cubes scored: ";
			item1Avg.ValueText.text = ToRoundStr(_Team.Item1Avg);
			item2Avg.KeyText.text = "Average alliance switch cubes scored: ";
			item2Avg.ValueText.text = ToRoundStr(_Team.Item2Avg);
			item3Avg.KeyText.text = "Average vault cubes scored: ";
			item3Avg.ValueText.text = ToRoundStr(_Team.Item3Avg);
			parkAvg.KeyText.text = "Average park percentage: ";
			parkAvg.ValueText.text = ToPercent(_Team.ParkAvg);
			endgameAvg.KeyText.text = "Average rope climbing ability: ";
			endgameAvg.ValueText.text = ToRoundStr(_Team.EndgameAvg);
			defenseAvg.KeyText.text = "Average defense ability: ";
			defenseAvg.ValueText.text = ToRoundStr(_Team.DefenseAvg);

			Comments.text = _Team.Comments;
		}

		
		if (_Team.IsFinalist)
		{
			FinalistText.text = "Unselect As Finalist";
		}
		else
		{
			FinalistText.text = "Select As Finalist";
		}

		UpdateMatchPicker();

		UpdateBrokenCheck();
		MatchSelected(0);
	}

	private void UpdateMatchPicker()
	{
		MatchPicker.ClearOptions();
		List<string> options = new List<string>(_Team.Matches.Count);
		foreach (Match m in _Team.Matches)
		{
			string s = m.MatchNum.ToString();
			if (m.Excluded)
				s += " (Excluded)";
			options.Add(s);
		}
		MatchPicker.AddOptions(options);
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
			else
			{
				Debug.Log("Override cancel!");
			}
			ConfirmOverridePanel.SetActive(false);
			OverrideButton.SetActive(true);
		}
	}

	public void ExcludeCurrentMatch()
	{
		Debug.Log("Exclude/Include requested for team " + _Team.TeamNum.ToString() + " match "
			+ _Team.Matches[MatchPicker.value].MatchNum);

		_Team.Matches[MatchPicker.value].Excluded = !_Team.Matches[MatchPicker.value].Excluded;
		Debug.Log("Exclude/Include Confirmed!");

		DataStorage.Instance.SaveData();

		// Reload the scene to update the statistics
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void SelectTeamAsFinalist()
	{
		_Team.IsFinalist = !_Team.IsFinalist;

		if (_Team.IsFinalist)
		{
			FinalistText.text = "Unselect As Finalist";
		}
		else
		{
			FinalistText.text = "Select As Finalist";
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
		TextValueItem autoItem3 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem item1 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem item2 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem item3 = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem defense = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem parked = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		TextValueItem endgame = Instantiate(MatchStatTemplate, MatchStatTemplate.transform.parent);
		MatchStatTemplate.gameObject.SetActive(false);

		autoMove.KeyText.text = "Crossed baseline in auto: ";
		autoMove.ValueText.text = match.MovedInAuto.ToString();
		autoItem1.KeyText.text = "Auto scale powercubes scored: ";
		autoItem1.ValueText.text = match.AutoScoreItem1.ToString();
		autoItem2.KeyText.text = "Auto Switch powercubes scored: ";
		autoItem2.ValueText.text = match.AutoScoreItem2.ToString();
		autoItem3.KeyText.text = "Auto vault powercubes scored: ";
		autoItem3.ValueText.text = match.AutoScoreItem3.ToString();

		item1.KeyText.text = "Scale powercubes scored: ";
		item1.ValueText.text = match.ScoreItem1.ToString();
		item2.KeyText.text = "Switch powercubes scored: ";
		item2.ValueText.text = match.ScoreItem2.ToString();
		item3.KeyText.text = "Vault powercubes scored: ";
		item3.ValueText.text = match.ScoreItem2.ToString();

		defense.KeyText.text = "Defense rating: ";
		defense.ValueText.text = match.DefenseAbility.ToString();
		parked.KeyText.text = "Parked robot: ";
		parked.ValueText.text = match.Parked.ToString();
		endgame.KeyText.text = "Climb rating: ";
		endgame.ValueText.text = match.EndgameAbility.ToString();

		if (!match.Excluded)
			ExcludeButtonText.text = "Exclude From Stats";
		else
			ExcludeButtonText.text = "Include In Stats";

		MatchComments.text = _Team.Matches[index].Comments;
	}

	public void DeleteTeam()
	{
		DataStorage.Instance.Teams.Remove(_Team);
		DataStorage.Instance.SaveData();
		RouteManager.NavigateBack();
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
