using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScoutingApp.GameData
{
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
		public void DeserializeData(Stream stream, bool closeAndSave = true)
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
						team.Deserialize(reader);
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

	public class Team : BaseSerializableData
	{
		public string TeamName { get; set; }
		public ushort TeamNum { get; set; }
		private List<Match> _Matches;
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
		private double _MovedInAutoAvg = DEFAULT;
		private double _Item1Avg = DEFAULT;
		private double _Item2Avg = DEFAULT;
		private double _EndgameAvg = DEFAULT;

		private DateTime _OverrideBroken = DateTime.MinValue;

		public double AutoItem1Avg
		{
			get
			{
				if (_EndgameAvg == DEFAULT && _Matches.Count > 0)
					_AutoItem1Avg = _Matches.Average(match => match.AutoBallScore);
				return _AutoItem1Avg;
			}
		}

		public double AutoItem2Avg
		{
			get
			{
				if (_AutoItem2Avg == DEFAULT && _Matches.Count > 0)
					_AutoItem2Avg = _Matches.Average(match => match.AutoGearScore ? 1 : 0);
				return _AutoItem2Avg;
			}
		}

		public double MovedInAutoAvg
		{
			get
			{
				if (_MovedInAutoAvg == DEFAULT && _Matches.Count > 0)
					_MovedInAutoAvg = _Matches.Average(match => match.MovedInAuto ? 1 : 0);
				return _MovedInAutoAvg;
			}
		}

		public double Item1Avg
		{
			get
			{
				if (_Item1Avg == DEFAULT && _Matches.Count > 0)
					_Item1Avg = _Matches.Average(match => match.BallScore);
				return _Item1Avg;
			}
		}

		public double Item2Avg
		{
			get
			{
				if (_Item2Avg == DEFAULT && _Matches.Count > 0)
					_Item2Avg = _Matches.Average(match => match.GearScore);
				return _Item2Avg;
			}
		}

		public double EndgameAvg
		{
			get
			{
				if (_EndgameAvg == DEFAULT && _Matches.Count > 0)
					_EndgameAvg = _Matches.Average(match => match.ClimbedRope ? 1 : 0);
				return _EndgameAvg;
			}
		}

		public bool NotBroken
		{
			get
			{
				if (_Matches.Count > 0)
				{
					Match match = _Matches.Last();
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
			SerializerHelper.WriteString(writer, TeamName);
			SerializerHelper.WriteString(writer, Comments);
			writer.Write((ushort)_Matches.Count);
			foreach (Match match in _Matches)
				match.Serialize(writer);
		}

		public override void Deserialize(BinaryReader reader)
		{
			TeamNum = reader.ReadUInt16();
			_OverrideBroken = DateTime.FromBinary(reader.ReadInt64());
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
	}

	public class Match : BaseSerializableData, IComparable<Match>
	{
		public MatchPosition MatchPos { get; set; }
		public ushort MatchNum { get; set; }
		public int AutoBallScore { get; set; }
		public int BallScore { get; set; }
		public byte GearScore { get; set; }
		public string Comments { get; set; }

		public bool MovedInAuto { get; set; }
		public bool AutoGearScore { get; set; }
		public bool ClimbedRope { get; set; }
		public bool WorksPostMatch { get; set; }
		public DateTime Timestamp { get; private set; }

		// Test method that creates a random Match with random information
		public Match(System.Random rand) : this()
		{
			MatchNum = (ushort)(rand.Next(72) + 1);
			MovedInAuto = rand.NextDouble() < 0.85D;
			WorksPostMatch = rand.NextDouble() < 0.90D;
			AutoBallScore = rand.NextDouble() < 0.25D ? rand.Next(30) : 0;
			AutoGearScore = rand.NextDouble() < 0.3D;
			BallScore = rand.NextDouble() < 0.25D ? rand.Next(30) : 0;
			GearScore = (byte)rand.Next(9);
			ClimbedRope = rand.NextDouble() > (1 / 3D);
			MatchPos = (MatchPosition)rand.Next(6);

			const string PRINTABLE_ASCII = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

			int commentsLen = rand.Next(500);
			for (int i = 0; i < commentsLen; i++)
				Comments += PRINTABLE_ASCII[rand.Next(PRINTABLE_ASCII.Length)];

			Timestamp = DateTime.Now;
		}

		public Match()
		{
			Comments = string.Empty;
		}

		public override void Serialize(BinaryWriter writer)
		{
			writer.Write(Timestamp.ToBinary());
			writer.Write(MatchNum);
			writer.Write(AutoBallScore);
			writer.Write(BallScore);
			writer.Write(GearScore);

			byte[] bools = SerializerHelper.PackBools(MovedInAuto, AutoGearScore, ClimbedRope, WorksPostMatch);
			writer.Write(bools[0]);

			writer.Write((byte)MatchPos);
			SerializerHelper.WriteString(writer, Comments);
		}

		public override void Deserialize(BinaryReader reader)
		{
			Timestamp = DateTime.FromBinary(reader.ReadInt64());
			MatchNum = reader.ReadUInt16();
			AutoBallScore = reader.ReadInt32();
			BallScore = reader.ReadInt32();
			GearScore = reader.ReadByte();

			byte[] boolBytes = new byte[] { reader.ReadByte() };
			bool[] bools = SerializerHelper.UnpackBools(boolBytes, 4);
			MovedInAuto = bools[0];
			AutoGearScore = bools[1];
			ClimbedRope = bools[2];
			WorksPostMatch = bools[3];

			MatchPos = (MatchPosition)reader.ReadByte();
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
		BLUE1 = 0,
		BLUE2 = 1,
		BLUE3 = 2,
		RED1 = 3,
		RED2 = 4,
		RED3 = 5
	}
}
