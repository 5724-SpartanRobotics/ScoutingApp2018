using ScoutingApp;
using ScoutingApp.GameData;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StandScouting : MonoBehaviour
{
	// TODO: add 'Obsolete' button to remove a match from averages in view_team
	public InputField TeamNumber;
	public InputField TeamName;
	public InputField MatchNumber;
	public Dropdown Position;
	public Toggle RobotMoved;
	public InputField AutoScoreItem1;
	public InputField AutoScoreItem2;
	public InputField TeleopScoreItem1;
	public InputField TeleopScoreItem2;
	public Toggle Endgame;
	public Toggle WorksAtEnd;
	// To be safe this input field has a max limit of 32767 characters because
	// the SerializerHelper uses a ushort as the byte limit and can therefore
	// only hold 65535 (ushort.MaxValue) bytes. (Leaves room for multi-byte
	// characters).
	public InputField Comments;
	public Text StatusText;

	public GameObject CommentsInput;
	private RectTransform _CommentsTransform;

	private string _StateFile;


	void Start()
	{
		_StateFile = Path.Combine(Application.temporaryCachePath, "stand_scouting_state.dat");
		_CommentsTransform = CommentsInput.GetComponent<RectTransform>();
		LoadState();
	}

	void Update()
	{
		// This makes the comments box expand as you input text
		_CommentsTransform.sizeDelta = new Vector2(_CommentsTransform.rect.width, Comments.preferredHeight + 30);

	}

	private bool _SaveOff;

	private void LoadState()
	{
		try
		{
			_SaveOff = true;
			if (File.Exists(_StateFile))
			{
				using (BinaryReader reader = new BinaryReader(File.OpenRead(_StateFile)))
				{
					TeamNumber.text = SerializerHelper.ReadString(reader);
					TeamName.text = SerializerHelper.ReadString(reader);
					MatchNumber.text = SerializerHelper.ReadString(reader);
					Position.value = reader.ReadInt32();
					RobotMoved.isOn = reader.ReadBoolean();
					AutoScoreItem1.text = SerializerHelper.ReadString(reader);
					AutoScoreItem2.text = SerializerHelper.ReadString(reader);
					TeleopScoreItem1.text = SerializerHelper.ReadString(reader);
					TeleopScoreItem2.text = SerializerHelper.ReadString(reader);
					Endgame.isOn = reader.ReadBoolean();
					WorksAtEnd.isOn = reader.ReadBoolean();
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError("There was an error while attempting to load the saved stand scouting state.");
			Debug.LogException(e);
			ClearState();
		}
		finally
		{
			_SaveOff = false;
		}
	}

	public void SaveState()
	{
		if (!_SaveOff)
		{
			try
			{
				using (BinaryWriter writer = new BinaryWriter(File.Open(_StateFile, FileMode.Create)))
				{
					SerializerHelper.WriteString(writer, TeamNumber.text);
					SerializerHelper.WriteString(writer, TeamName.text);
					SerializerHelper.WriteString(writer, MatchNumber.text);
					writer.Write(Position.value);
					writer.Write(RobotMoved.isOn);
					SerializerHelper.WriteString(writer, AutoScoreItem1.text);
					SerializerHelper.WriteString(writer, AutoScoreItem2.text);
					SerializerHelper.WriteString(writer, TeleopScoreItem1.text);
					SerializerHelper.WriteString(writer, TeleopScoreItem2.text);
					writer.Write(Endgame.isOn);
					writer.Write(WorksAtEnd.isOn);
				}
			}
			catch (Exception e)
			{
				Debug.LogError("There was an error while attempting to save the stand scouting state.");
				Debug.LogException(e);
			}
		}
	}

	private void ClearState()
	{
		Debug.Log("Clearing saved stand scouting state...");
		if (File.Exists(_StateFile))
			File.Delete(_StateFile);
	}

	private void ResetPage()
	{
		TeamNumber.text = string.Empty;
		TeamName.text = string.Empty;
		MatchNumber.text = string.Empty;
		Position.value = 0;
		RobotMoved.isOn = false;
		AutoScoreItem1.text = string.Empty;
		AutoScoreItem2.text = string.Empty;
		TeleopScoreItem1.text = string.Empty;
		TeleopScoreItem2.text = string.Empty;
		Endgame.isOn = false;
		WorksAtEnd.isOn = true;
		Comments.text = string.Empty;
	}

	private bool CheckEmpty(InputField text)
	{
		return string.IsNullOrWhiteSpace(text.text);
	}

	public void Save()
	{
		// Don't have to check for comments because comments can technically be empty
		if (CheckEmpty(TeamName) || CheckEmpty(TeamNumber) || CheckEmpty(MatchNumber) || CheckEmpty(AutoScoreItem1) ||
			CheckEmpty(AutoScoreItem2) || CheckEmpty(TeleopScoreItem1) || CheckEmpty(TeleopScoreItem2))
		{
			StatusText.color = Color.red;
			StatusText.text = $"Error, you must specify all fields before saving!";
			StatusText.gameObject.SetActive(true);
			return;
		}

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
			WorksPostMatch = WorksAtEnd.isOn,
			Comments = Comments.text
		};

		team.AddMatch(match);
		DataStorage.Instance.Teams.Add(team);
		DataStorage.Instance.SaveData();

		StatusText.color = Color.green;
		StatusText.text = $"Team '{TeamName.text}' match {MatchNumber.text} saved!";
		StatusText.gameObject.SetActive(true);

		ClearState();
		ResetPage();
	}
}
