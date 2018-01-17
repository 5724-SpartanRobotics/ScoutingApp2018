using ScoutingApp;
using ScoutingApp.GameData;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class StandScouting : MonoBehaviour
{
	public InputField TeamNumber;
	public InputField MatchNumber;
	public Dropdown Position;
	public Toggle RobotMoved;
	public IntScoreItem AutoScoreItem1;
	public IntScoreItem AutoScoreItem2;
	public IntScoreItem AutoScoreItem3;
	public IntScoreItem TeleopScoreItem1;
	public IntScoreItem TeleopScoreItem2;
	public IntScoreItem TeleopScoreItem3;
	public Toggle RobotParked;
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
					MatchNumber.text = SerializerHelper.ReadString(reader);
					Position.value = reader.ReadInt32();
					RobotMoved.isOn = reader.ReadBoolean();
					AutoScoreItem1.Value = reader.ReadInt32();
					AutoScoreItem2.Value = reader.ReadInt32();
					AutoScoreItem3.Value = reader.ReadInt32();
					TeleopScoreItem1.Value = reader.ReadInt32();
					TeleopScoreItem2.Value = reader.ReadInt32();
					TeleopScoreItem3.Value = reader.ReadInt32();
					RobotParked.isOn = reader.ReadBoolean();
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
				string tempFile = Path.Combine(Application.temporaryCachePath, "stand_scouting_state.temp");

				using (BinaryWriter writer = new BinaryWriter(File.Open(tempFile, FileMode.Create)))
				{
					SerializerHelper.WriteString(writer, TeamNumber.text);
					SerializerHelper.WriteString(writer, MatchNumber.text);
					writer.Write(Position.value);
					writer.Write(RobotMoved.isOn);
					writer.Write(AutoScoreItem1.Value);
					writer.Write(AutoScoreItem2.Value);
					writer.Write(AutoScoreItem3.Value);
					writer.Write(TeleopScoreItem1.Value);
					writer.Write(TeleopScoreItem2.Value);
					writer.Write(TeleopScoreItem3.Value);
					writer.Write(RobotParked.isOn);
					writer.Write(Endgame.isOn);
					writer.Write(WorksAtEnd.isOn);
				}

				if (File.Exists(_StateFile))
					File.Delete(_StateFile);

				File.Move(tempFile, _StateFile);
			}
			catch (Exception e)
			{
				Debug.LogError("There was an error while attempting to save the stand scouting state.");
				Debug.LogException(e);
			}
		}
	}

	public void ClearState()
	{
		Debug.Log("Clearing saved stand scouting state...");
		if (File.Exists(_StateFile))
			File.Delete(_StateFile);
		ResetPage();
	}

	private void ResetPage()
	{
		TeamNumber.text = string.Empty;
		MatchNumber.text = string.Empty;
		Position.value = 0;
		RobotMoved.isOn = false;
		AutoScoreItem1.Value = 0;
		AutoScoreItem2.Value = 0;
		AutoScoreItem3.Value = 0;
		TeleopScoreItem1.Value = 0;
		TeleopScoreItem2.Value = 0;
		TeleopScoreItem3.Value = 0;
		RobotParked.isOn = false;
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
		if (CheckEmpty(TeamNumber) || CheckEmpty(MatchNumber))
		{
			StatusText.color = Color.red;
			StatusText.text = $"Error, you must specify all fields before saving!";
			StatusText.gameObject.SetActive(true);
			return;
		}

		Team team = new Team
		{
			TeamNum = ushort.Parse(TeamNumber.text)
		};

		Match match = new Match
		{
			MatchNum = ushort.Parse(MatchNumber.text),
			MatchPos = (MatchPosition)Position.value,
			MovedInAuto = RobotMoved.isOn,
			AutoScoreItem1 = AutoScoreItem1.Value,
			AutoScoreItem2 = AutoScoreItem2.Value,
			AutoScoreItem3 = AutoScoreItem3.Value,
			ScoreItem1 = TeleopScoreItem1.Value,
			ScoreItem2 = TeleopScoreItem2.Value,
			ScoreItem3 = TeleopScoreItem3.Value,
			Parked = RobotParked.isOn,
			Endgame = Endgame.isOn,
			WorksPostMatch = WorksAtEnd.isOn,
			Comments = Comments.text
		};

		team.AddMatch(match);
		DataStorage.Instance.Teams.Add(team);
		DataStorage.Instance.SaveData();

		StatusText.color = Color.green;
		StatusText.text = $"Team '{TeamNumber.text}' match {MatchNumber.text} saved!";
		StatusText.gameObject.SetActive(true);

		ClearState();
	}
}
