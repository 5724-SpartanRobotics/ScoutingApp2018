using ScoutingApp.GameData;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StandScouting : MonoBehaviour
{
	public Text TeamNumber;
	public Text TeamName;
	public Text MatchNumber;
	public Dropdown Position;
	public Toggle RobotMoved;
	public Text AutoScoreItem1;
	public Text AutoScoreItem2;
	public Text TeleopScoreItem1;
	public Text TeleopScoreItem2;
	public Toggle Endgame;
	public Toggle WorksAtEnd;

	private bool CheckEmpty(Text text)
	{
		return string.IsNullOrWhiteSpace(text.text);
	}

	public void Save()
	{
		if (CheckEmpty(TeamName) || CheckEmpty(TeamNumber) || CheckEmpty(MatchNumber) || CheckEmpty(AutoScoreItem1) ||
			CheckEmpty(AutoScoreItem2) || CheckEmpty(TeleopScoreItem1) || CheckEmpty(TeleopScoreItem2))
			return; // TODO make an error message or something
		Team team = new Team
		{
			TeamName = TeamName.text,
			TeamNum = ushort.Parse(TeamNumber.text)
		};

		Match match = new Match
		{
			MatchNum = ushort.Parse(MatchNumber.text),
			MatchPos = (MatchPosition)Position.value,
			MovedInAuto = RobotMoved.isOn,
			AutoScoreItem1 = int.Parse(AutoScoreItem1.text),
			AutoScoreItem2 = int.Parse(AutoScoreItem2.text),
			ScoreItem1 = int.Parse(TeleopScoreItem1.text),
			ScoreItem2 = int.Parse(TeleopScoreItem2.text),
			Endgame = Endgame.isOn,
			WorksPostMatch = WorksAtEnd.isOn
		};

		team.AddMatch(match);
		DataStorage.Instance.Teams.Add(team);
		DataStorage.Instance.SaveData();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
