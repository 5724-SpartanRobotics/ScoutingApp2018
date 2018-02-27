using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScoutingApp.GameData
{// TODO add robot position to statistics page
	public class DataStorage
	{
		private static DataStorage _Inst;
		public static DataStorage Instance
		{
			get
			{
				if (_Inst == null)
				{
					_Inst = new DataStorage();
					_Inst.LoadData();
				}
				return _Inst;
			}
		}

		public TeamList Teams { get; } = new TeamList();
		const ushort DATA_VERSION = 0;
		private string _SaveLoc = Path.Combine(Application.persistentDataPath, "save.dat");

		public void LoadData()
		{
			try
			{
				if (File.Exists(_SaveLoc))
					using (FileStream fs = File.OpenRead(_SaveLoc))
						DeserializeData(fs, false);
			}
			catch (Exception e)
			{
				Debug.Log("Error, save.dat missing or corrupted, replacing:");
				Debug.LogException(e);
				SaveData();
			}
		}

		public void SaveData()
		{
			using (FileStream fs = File.Open(_SaveLoc, FileMode.Create))
				SerializeData(fs);
		}

		/// <summary>
		/// Deserializes the data in the Stream and adds it to this instance.
		/// If closeAndSave is true, closes the stream and saves the data to the save file,
		/// otherwise it leaves the stream open and does not save to the save file.
		/// </summary>
		/// <param name="stream">The stream to deserialize the data from.</param>
		/// <param name="closeAndSave">Whether or not to close the stream and save the data.</param>
		public void DeserializeData(Stream stream, bool closeAndSave = true, bool value1 = false)
		{
			int isCompressed = stream.ReadByte();
			if (isCompressed == 0)
			{
				using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, !closeAndSave))
				{
					ushort version = reader.ReadUInt16();
					if (version != DATA_VERSION)
						throw new FormatException($"Version number {version} is not a supported data version!");
					int count = reader.ReadInt32();

					for (int i = 0; i < count; i++)
					{
						Team team = new Team();
						team.Deserialize(reader, value1);
						Teams.Add(team);
					}
				}
				if (closeAndSave)
					SaveData();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Serializes the data in this instance to the stream.
		/// </summary>
		/// <param name="stream">The stream to write the data to.</param>
		/// <param name="close">Whether or not to close the stream after serializing to it.</param>
		public void SerializeData(Stream stream, bool close = false)
		{
			MemoryStream uncompressed = new MemoryStream();

			using (BinaryWriter writer = new BinaryWriter(uncompressed, Encoding.UTF8))
			{
				MemoryStream compressed = new MemoryStream();
				byte[] uncompressedBytes;
				byte[] compressedBytes;

				writer.Write(DATA_VERSION);
				writer.Write(Teams.Count);

				foreach (Team team in Teams)
					team.Serialize(writer);

				uncompressedBytes = uncompressed.ToArray();

				compressedBytes = uncompressedBytes;
				if (uncompressedBytes.Length < compressedBytes.Length)
				{
					compressed.Position = 0;
					stream.WriteByte(1);
					compressed.CopyTo(stream);
				}
				else
				{
					uncompressed.Position = 0;
					stream.WriteByte(0);
					uncompressed.CopyTo(stream);
				}
				if (close)
					stream.Dispose();
			}
		}
	}

	public class TeamList : ICollection<Team>
	{
		SortedList<int, Team> _Teams;

		public TeamList()
		{
			_Teams = new SortedList<int, Team>();
		}

		public int Count => _Teams.Count;

		public bool IsReadOnly => false;

		public void Add(Team item)
		{
			if (Contains(item))
				_Teams[item.TeamNum].MergeTeam(item);
			else
				_Teams.Add(item.TeamNum, item);
		}

		public Team this[int teamNum]
		{
			get
			{
				return _Teams[teamNum];
			}
		}

		public void Clear()
		{
			_Teams.Clear();
		}

		public bool Contains(Team team)
		{
			return Contains(team.TeamNum);
		}

		public bool Contains(int num)
		{
			return _Teams.ContainsKey(num);
		}

		public void CopyTo(Team[] array, int arrayIndex)
		{
			_Teams.Values.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Team> GetEnumerator()
		{
			return _Teams.Values.GetEnumerator();
		}

		public bool Remove(Team item)
		{
			return _Teams.Remove(item.TeamNum);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _Teams.Values.GetEnumerator();
		}
	}

	public class Team : BaseSerializableData, IComparable<Team>
	{
		public string TeamName { get; set; }
		public ushort TeamNum { get; set; }

		public bool IsFinalist { get; set; }

		private List<Match> _Matches;

		public List<Match> AvgMatches
		{
			get
			{
				return _Matches.Where(x => !x.Excluded).ToList();
			}
		}

		public ImmutableList<Match> Matches
		{
			get
			{
				return new ImmutableList<Match>(_Matches);
			}
		}

		public string Comments { get; set; }

		// Test method that creates a random Team with random information and match data
		public Team(System.Random rand) : this()
		{
			const string PRINTABLE_ASCII = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
			int nameLen = rand.Next(5) + 5;
			for (int i = 0; i < nameLen; i++)
				TeamName += PRINTABLE_ASCII[rand.Next(PRINTABLE_ASCII.Length)];

			int commentsLen = rand.Next(500);
			for (int i = 0; i < commentsLen; i++)
				Comments += PRINTABLE_ASCII[rand.Next(PRINTABLE_ASCII.Length)];

			TeamNum = (ushort)rand.Next(10000);

			int numMatches = rand.Next(10) + 2;

			for (int i = 0; i < numMatches; i++)
				_Matches.Add(new Match(rand));
			_Matches.Sort();
		}

		const int DEFAULT = -1;
		private double _AutoItem1Avg = DEFAULT;
		private double _AutoItem2Avg = DEFAULT;
		private double _AutoItem3Avg = DEFAULT;
		private double _MovedInAutoAvg = DEFAULT;
		private double _Item1Avg = DEFAULT;
		private double _Item2Avg = DEFAULT;
		private double _Item3Avg = DEFAULT;
		private double _ParkAvg = DEFAULT;
		private double _EndgameAvg = DEFAULT;
		private double _DefenseAvg = DEFAULT;

		private DateTime _OverrideBroken = DateTime.MinValue;

		public double AutoItem1Avg
		{
			get
			{
				if (_EndgameAvg == DEFAULT && AvgMatches.Count > 0)
					_AutoItem1Avg = AvgMatches.Average(match => match.AutoScoreItem1);
				return _AutoItem1Avg;
			}
		}

		public double AutoItem2Avg
		{
			get
			{
				if (_AutoItem2Avg == DEFAULT && AvgMatches.Count > 0)
					_AutoItem2Avg = AvgMatches.Average(match => match.AutoScoreItem2);
				return _AutoItem2Avg;
			}
		}

		public double AutoItem3Avg
		{
			get
			{
				if (_AutoItem3Avg == DEFAULT && AvgMatches.Count > 0)
					_AutoItem3Avg = AvgMatches.Average(match => match.AutoScoreItem3);
				return _AutoItem3Avg;
			}
		}

		public double MovedInAutoAvg
		{
			get
			{
				if (_MovedInAutoAvg == DEFAULT && AvgMatches.Count > 0)
					_MovedInAutoAvg = AvgMatches.Average(match => match.MovedInAuto ? 1 : 0);
				return _MovedInAutoAvg;
			}
		}

		public double Item1Avg
		{
			get
			{
				if (_Item1Avg == DEFAULT && AvgMatches.Count > 0)
					_Item1Avg = AvgMatches.Average(match => match.ScoreItem1);
				return _Item1Avg;
			}
		}

		public double Item2Avg
		{
			get
			{
				if (_Item2Avg == DEFAULT && AvgMatches.Count > 0)
					_Item2Avg = AvgMatches.Average(match => match.ScoreItem2);
				return _Item2Avg;
			}
		}

		public double Item3Avg
		{
			get
			{
				if (_Item3Avg == DEFAULT && AvgMatches.Count > 0)
					_Item3Avg = AvgMatches.Average(match => match.ScoreItem3);
				return _Item3Avg;
			}
		}

		public double ParkAvg
		{
			get
			{
				if (_ParkAvg == DEFAULT && AvgMatches.Count > 0)
					_ParkAvg = AvgMatches.Average(match => match.Parked ? 1 : 0);
				return _ParkAvg;
			}
		}

		public double EndgameAvg
		{
			get
			{
				if (_EndgameAvg == DEFAULT && AvgMatches.Count > 0)
					_EndgameAvg = AvgMatches.Average(match => match.EndgameAbility);
				return _EndgameAvg;
			}
		}

		public double DefenseAvg
		{
			get
			{
				if (_DefenseAvg == DEFAULT && AvgMatches.Count > 0)
					_DefenseAvg = AvgMatches.Average(match => match.DefenseAbility);
				return _DefenseAvg;
			}
		}

		public bool NotBroken
		{
			get
			{
				if (AvgMatches.Count > 0)
				{
					Match match = AvgMatches.Last();
					if (match.Timestamp > _OverrideBroken)
						return match.WorksPostMatch;
					else
						return !match.WorksPostMatch;
				}
				return false;
			}
		}

		public Team()
		{
			TeamName = string.Empty;
			Comments = string.Empty;
			_Matches = new List<Match>();
		}

		public override void Serialize(BinaryWriter writer)
		{
			writer.Write(TeamNum);
			writer.Write(_OverrideBroken.ToBinary());

			writer.Write(IsFinalist);

			SerializerHelper.WriteString(writer, TeamName);
			SerializerHelper.WriteString(writer, Comments);
			writer.Write((ushort)_Matches.Count);
			foreach (Match match in _Matches)
				match.Serialize(writer);
		}

		public override void Deserialize(BinaryReader reader, bool value1 = false)
		{
			TeamNum = reader.ReadUInt16();
			_OverrideBroken = DateTime.FromBinary(reader.ReadInt64());

			IsFinalist = reader.ReadBoolean();
			if (value1)
				IsFinalist = false;

			TeamName = SerializerHelper.ReadString(reader);
			Comments = SerializerHelper.ReadString(reader);
			int len = reader.ReadUInt16();
			_Matches = new List<Match>(len);
			for (int i = 0; i < len; i++)
			{
				Match match = new Match();
				match.Deserialize(reader);
				_Matches.Add(match);
			}
			_Matches.Sort();
		}

		public void MergeTeam(Team item)
		{
			if (TeamNum != item.TeamNum)
				throw new ArgumentException($"Team {item.TeamNum} != team {TeamNum}");

			if (!string.IsNullOrWhiteSpace(item.Comments))
				Comments += "\n\n" + item.Comments;

			if (item._OverrideBroken > _OverrideBroken)
				_OverrideBroken = item._OverrideBroken;

			foreach (Match match in item._Matches)
			{
				bool flag = true;
				foreach (Match thisMatch in _Matches)
				{
					if (match.CompareTo(thisMatch) == 0)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					AddMatch(match);
				}
			}
		}

		public void AddMatch(Match match)
		{
			_Matches.Add(match);
			_Matches.Sort();

			_AutoItem1Avg = DEFAULT;
			_AutoItem2Avg = DEFAULT;
			_AutoItem3Avg = DEFAULT;
			_MovedInAutoAvg = DEFAULT;
			_Item1Avg = DEFAULT;
			_Item2Avg = DEFAULT;
			_Item3Avg = DEFAULT;
			_ParkAvg = DEFAULT;
			_EndgameAvg = DEFAULT;
			_DefenseAvg = DEFAULT;
		}

		public void OverrideBroken()
		{
			if (Matches.Count == 0)
				return;
			if (_OverrideBroken < Matches.Last().Timestamp)
				_OverrideBroken = DateTime.Now;
			else
				_OverrideBroken = DateTime.MinValue;
			DataStorage.Instance.SaveData();
		}

		/// <summary>
		/// The score is a value that is calculated based on importance of each
		/// score item, which allows us to compare Teams.
		/// </summary>
		/// <returns></returns>
		public double GenerateRating(bool accountBroken = true)
		{
			return GetEndgameRating(false) +
				GetBoxRating(false) +
				(accountBroken && !NotBroken ? -1000000000 : 0);
		}

		public double GetEndgameRating(bool accountBroken = true)
		{
			return EndgameAvg * 30 +
				ParkAvg * 5 +
				(accountBroken && !NotBroken ? -1000000000 : 0);
		}

		public double GetBoxRating(bool accountBroken = true)
		{
			return AutoItem1Avg * 5 +
				AutoItem2Avg * 4 +
				AutoItem3Avg * 3 +
				Item1Avg * 3 + // Scale
				Item2Avg * 1 + // Alliance switch
				Item3Avg * 2 +
				(accountBroken && !NotBroken ? -1000000000 : 0);
		}

		public int CompareTo(Team other)
		{
			return GenerateRating().CompareTo(other.GenerateRating());
		}
	}

	public class Match : BaseSerializableData, IComparable<Match>
	{
		public MatchPosition MatchPos { get; set; }
		public RobotPosition RobotPos { get; set; }
		public ushort MatchNum { get; set; }
		public int AutoScoreItem1 { get; set; }
		public int AutoScoreItem2 { get; set; }
		public int AutoScoreItem3 { get; set; }
		public int ScoreItem1 { get; set; }
		public int ScoreItem2 { get; set; }
		public int ScoreItem3 { get; set; }
		public string Comments { get; set; }

		public bool MovedInAuto { get; set; }
		public bool Parked { get; set; }
		public byte EndgameAbility { get; set; }
		public byte DefenseAbility { get; set; }
		public bool WorksPostMatch { get; set; }
		public DateTime Timestamp { get; private set; }

		// Whether to exclude this match from averages
		public bool Excluded { get; set; }

		// Test method that creates a random Match with random information
		public Match(System.Random rand) : this()
		{
			MatchNum = (ushort)(rand.Next(72) + 1);
			MovedInAuto = rand.NextDouble() < 0.85D;
			WorksPostMatch = rand.NextDouble() < 0.90D;
			AutoScoreItem1 = rand.NextDouble() < 0.25D ? rand.Next(30) : 0;
			AutoScoreItem2 = (int)(rand.NextDouble() * 2);
			AutoScoreItem3 = (int)(rand.NextDouble() * 2);
			ScoreItem1 = rand.NextDouble() < 0.25D ? rand.Next(30) : 0;
			ScoreItem2 = (byte)rand.Next(9);
			ScoreItem3 = (byte)rand.Next(9);
			EndgameAbility = (byte)(rand.NextDouble() * 3);
			DefenseAbility = (byte)(rand.NextDouble() * 3);
			MatchPos = (MatchPosition)rand.Next(6);
			Excluded = false;

			const string PRINTABLE_ASCII = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

			int commentsLen = rand.Next(500);
			for (int i = 0; i < commentsLen; i++)
				Comments += PRINTABLE_ASCII[rand.Next(PRINTABLE_ASCII.Length)];

			Timestamp = DateTime.Now;
		}

		public Match()
		{
			Comments = string.Empty;
			Timestamp = DateTime.Now;
		}

		public override void Serialize(BinaryWriter writer)
		{
			writer.Write(Timestamp.ToBinary());
			writer.Write(MatchNum);
			writer.Write(AutoScoreItem1);
			writer.Write(AutoScoreItem2);
			writer.Write(AutoScoreItem3);
			writer.Write(ScoreItem1);
			writer.Write(ScoreItem2);
			writer.Write(ScoreItem3);
			writer.Write(EndgameAbility);
			writer.Write(DefenseAbility);

			byte[] bools = SerializerHelper.PackBools(MovedInAuto, Parked, WorksPostMatch, Excluded);
			writer.Write(bools[0]);

			writer.Write((byte)MatchPos);
			writer.Write((byte)RobotPos);
			SerializerHelper.WriteString(writer, Comments);
		}

		public override void Deserialize(BinaryReader reader, bool value1 = false)
		{
			Timestamp = DateTime.FromBinary(reader.ReadInt64());
			MatchNum = reader.ReadUInt16();
			AutoScoreItem1 = reader.ReadInt32();
			AutoScoreItem2 = reader.ReadInt32();
			AutoScoreItem3 = reader.ReadInt32();
			ScoreItem1 = reader.ReadInt32();
			ScoreItem2 = reader.ReadInt32();
			ScoreItem3 = reader.ReadInt32();
			EndgameAbility = reader.ReadByte();
			DefenseAbility = reader.ReadByte();

			byte[] boolBytes = new byte[] { reader.ReadByte() };
			bool[] bools = SerializerHelper.UnpackBools(boolBytes, 4);
			MovedInAuto = bools[0];
			Parked = bools[1];
			WorksPostMatch = bools[2];
			Excluded = bools[3];

			MatchPos = (MatchPosition)reader.ReadByte();
			RobotPos = (RobotPosition)reader.ReadByte();
			Comments = SerializerHelper.ReadString(reader);
		}

		public int CompareTo(Match other)
		{
			int comp = MatchNum.CompareTo(other.MatchNum);
			return comp != 0 ? comp : Timestamp.CompareTo(other.Timestamp);
		}
	}

	public enum MatchPosition : byte
	{
		RED1 = 0,
		RED2 = 1,
		RED3 = 2,
		BLUE1 = 3,
		BLUE2 = 4,
		BLUE3 = 5
	}

	public enum RobotPosition : byte
	{
		LEFT = 0,
		CENTER = 1,
		RIGHT = 2,
	}
}
