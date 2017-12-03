using System;
using System.Collections.Generic;
using System.IO;

namespace ScoutingApp.GameData
{
	public class DataStorage
	{
		public static DataStorage Instance { get; } = new DataStorage();
		public List<Team> Teams { get; } = new List<Team>();

		public DataStorage()
		{
			if (Instance != null)
				throw new ApplicationException("Cannot create more than one instance of DataStorage!");
		}
	}

	public class Team : BaseSerializableData
	{
		public string TeamName { get; set; }
		public ushort TeamNum { get; set; }
		public List<Match> Matches { get; set; }
		public string Comments { get; set; }

		// Test method that creates a random Team with random information and match data
		public Team(Random rand) : this()
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
				Matches.Add(new Match(rand));
		}

		public Team()
		{
			TeamName = string.Empty;
			Comments = string.Empty;
			Matches = new List<Match>();
		}

		public override void Serialize(BinaryWriter writer)
		{
			writer.Write(TeamNum);
			SerializerHelper.WriteString(writer, TeamName);
			SerializerHelper.WriteString(writer, Comments);
			writer.Write((ushort)Matches.Count);
			foreach (Match match in Matches)
				match.Serialize(writer);
		}

		public override void Deserialize(BinaryReader reader)
		{
			TeamNum = reader.ReadUInt16();
			TeamName = SerializerHelper.ReadString(reader);
			Comments = SerializerHelper.ReadString(reader);
			int len = reader.ReadUInt16();
			Matches = new List<Match>(len);
			for (int i = 0; i < len; i++)
			{
				Match match = new Match();
				match.Deserialize(reader);
				Matches.Add(match);
			}
		}

	}

	public class Match : BaseSerializableData
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
		public bool WorksAtEnd { get; set; }

		// Test method that creates a random Match with random information
		public Match(Random rand) : this()
		{
			MatchNum = (ushort)(rand.Next(72) + 1);
			MovedInAuto = rand.NextDouble() < 0.85D;
			WorksAtEnd = rand.NextDouble() < 0.90D;
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
		}

		public Match()
		{
			Comments = string.Empty;
		}

		public override void Serialize(BinaryWriter writer)
		{
			writer.Write(MatchNum);
			writer.Write(AutoBallScore);
			writer.Write(BallScore);
			writer.Write(GearScore);

			byte[] bools = SerializerHelper.PackBools(MovedInAuto, AutoGearScore, ClimbedRope, WorksAtEnd);
			writer.Write(bools[0]);

			writer.Write((byte)MatchPos);
			SerializerHelper.WriteString(writer, Comments);
		}

		public override void Deserialize(BinaryReader reader)
		{
			MatchNum = reader.ReadUInt16();
			AutoBallScore = reader.ReadInt32();
			BallScore = reader.ReadInt32();
			GearScore = reader.ReadByte();

			byte[] boolBytes = new byte[] { reader.ReadByte() };
			bool[] bools = SerializerHelper.UnpackBools(boolBytes, 4);
			MovedInAuto = bools[0];
			AutoGearScore = bools[1];
			ClimbedRope = bools[2];
			WorksAtEnd = bools[3];

			MatchPos = (MatchPosition)reader.ReadByte();
			Comments = SerializerHelper.ReadString(reader);
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
