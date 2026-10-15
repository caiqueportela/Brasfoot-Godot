using Godot;

namespace Brasfoot;

[GlobalClass]
public partial class CatalogResource : Resource
{
	[Export] public string Format { get; set; } = "";
	[Export] public int Version { get; set; }

	[Export] public int[] CountryId { get; set; } = System.Array.Empty<int>();
	[Export] public string[] CountryCode { get; set; } = System.Array.Empty<string>();
	[Export] public string[] CountryName { get; set; } = System.Array.Empty<string>();
	[Export] public int[] CountryConfederation { get; set; } = System.Array.Empty<int>();
	[Export] public int[] CountryLevel { get; set; } = System.Array.Empty<int>();

	[Export] public int[] StateId { get; set; } = System.Array.Empty<int>();
	[Export] public string[] Abbreviation { get; set; } = System.Array.Empty<string>();
	[Export] public string[] StateName { get; set; } = System.Array.Empty<string>();

	[Export] public int[] LeagueCountry { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueDivision { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueFormat { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueTieBreak { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueTeamCount { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueFlags { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueGroups { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueRelegated { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueDirectRelegation { get; set; } = System.Array.Empty<int>();
	[Export] public int[] LeagueKnockout { get; set; } = System.Array.Empty<int>();
	[Export] public string[] LeagueName { get; set; } = System.Array.Empty<string>();
	[Export] public string[] LeagueDivisionName { get; set; } = System.Array.Empty<string>();

	[Export] public int[] StateCupId { get; set; } = System.Array.Empty<int>();
	[Export] public int[] StateCupDivision { get; set; } = System.Array.Empty<int>();
	[Export] public int[] StateCupFormat { get; set; } = System.Array.Empty<int>();
	[Export] public int[] StateCupRelegated { get; set; } = System.Array.Empty<int>();
	[Export] public int[] StateCupTieBreak { get; set; } = System.Array.Empty<int>();
	[Export] public int[] StateCupFinalCount { get; set; } = System.Array.Empty<int>();
	[Export] public int[] StateCupFinals { get; set; } = System.Array.Empty<int>();

	[Export] public string[] ClubSlug { get; set; } = System.Array.Empty<string>();
	[Export] public string[] ClubName { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubCountry { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubState { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubLevel { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubReputation { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubStadium { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubCapacity { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubCoach { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubCoachCountry { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubColor1 { get; set; } = System.Array.Empty<string>();
	[Export] public string[] ClubColor2 { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubPlayerStart { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubPlayerCount { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubYouthStart { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubYouthCount { get; set; } = System.Array.Empty<int>();

	[Export] public string[] PlayerName { get; set; } = System.Array.Empty<string>();
	[Export] public int[] PlayerCountry { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerAge { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerPosition { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerSide { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerStatus { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerTrait1 { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerTrait2 { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerFlags { get; set; } = System.Array.Empty<int>();

	[Export] public string[] NameCode { get; set; } = System.Array.Empty<string>();
	[Export] public int[] GivenStart { get; set; } = System.Array.Empty<int>();
	[Export] public int[] GivenCount { get; set; } = System.Array.Empty<int>();
	[Export] public string[] GivenName { get; set; } = System.Array.Empty<string>();
	[Export] public int[] SurnameStart { get; set; } = System.Array.Empty<int>();
	[Export] public int[] SurnameCount { get; set; } = System.Array.Empty<int>();
	[Export] public string[] Surname { get; set; } = System.Array.Empty<string>();
	[Export] public string[] NameTable { get; set; } = System.Array.Empty<string>();

	public static CatalogResource FromData(CatalogData banco)
	{
		var c = new CatalogResource { Format = "catalog", Version = Paths.FormatVersion };
		int np = banco.Countries.Count;
		c.CountryId = new int[np];
		c.CountryCode = new string[np];
		c.CountryName = new string[np];
		c.CountryConfederation = new int[np];
		c.CountryLevel = new int[np];
		for (int i = 0; i < np; i++)
		{
			var p = banco.Countries[i];
			c.CountryId[i] = p.Id;
			c.CountryCode[i] = p.Code ?? "";
			c.CountryName[i] = p.Name ?? "";
			c.CountryConfederation[i] = p.Confederation;
			c.CountryLevel[i] = p.Level;
		}

		int ne = banco.States.Count;
		c.StateId = new int[ne];
		c.Abbreviation = new string[ne];
		c.StateName = new string[ne];
		for (int i = 0; i < ne; i++)
		{
			c.StateId[i] = banco.States[i].Id;
			c.Abbreviation[i] = banco.States[i].Abbreviation ?? "";
			c.StateName[i] = banco.States[i].Name ?? "";
		}

		int nl = banco.Leagues.Count;
		c.LeagueCountry = new int[nl];
		c.LeagueDivision = new int[nl];
		c.LeagueFormat = new int[nl];
		c.LeagueTieBreak = new int[nl];
		c.LeagueTeamCount = new int[nl];
		c.LeagueFlags = new int[nl];
		c.LeagueGroups = new int[nl];
		c.LeagueRelegated = new int[nl];
		c.LeagueDirectRelegation = new int[nl];
		c.LeagueKnockout = new int[nl];
		c.LeagueName = new string[nl];
		c.LeagueDivisionName = new string[nl];
		for (int i = 0; i < nl; i++)
		{
			var l = banco.Leagues[i];
			c.LeagueCountry[i] = l.CountryId;
			c.LeagueDivision[i] = l.Division;
			c.LeagueFormat[i] = l.Format;
			c.LeagueTieBreak[i] = l.TieBreak;
			c.LeagueTeamCount[i] = l.TeamCount;
			c.LeagueFlags[i] = (l.HomeAndAway ? 1 : 0) | (l.MatchesInsideGroup ? 2 : 0) | (l.RankByOverallTable ? 4 : 0);
			c.LeagueGroups[i] = l.GroupCount;
			c.LeagueRelegated[i] = l.RelegatedCount;
			c.LeagueDirectRelegation[i] = l.DirectRelegation;
			c.LeagueKnockout[i] = l.KnockoutTeamCount;
			c.LeagueName[i] = l.Name ?? "";
			c.LeagueDivisionName[i] = l.DivisionName ?? "";
		}

		int ns = banco.StateCups.Count;
		c.StateCupId = new int[ns];
		c.StateCupDivision = new int[ns];
		c.StateCupFormat = new int[ns];
		c.StateCupRelegated = new int[ns];
		c.StateCupTieBreak = new int[ns];
		c.StateCupFinalCount = new int[ns];
		var finais = new List<int>();
		for (int i = 0; i < ns; i++)
		{
			var e = banco.StateCups[i];
			c.StateCupId[i] = e.Id;
			c.StateCupDivision[i] = e.Division;
			c.StateCupFormat[i] = e.Format;
			c.StateCupRelegated[i] = e.RelegatedCount;
			c.StateCupTieBreak[i] = e.TieBreak;
			var f = e.FinalRounds ?? new List<int>();
			c.StateCupFinalCount[i] = f.Count;
			finais.AddRange(f);
		}
		c.StateCupFinals = finais.ToArray();

		int nt = banco.Clubs.Count;
		int nJog = 0;
		foreach (var t in banco.Clubs) nJog += t.Players.Count + t.Youth.Count;
		c.ClubSlug = new string[nt];
		c.ClubName = new string[nt];
		c.ClubCountry = new int[nt];
		c.ClubState = new int[nt];
		c.ClubLevel = new int[nt];
		c.ClubReputation = new int[nt];
		c.ClubStadium = new string[nt];
		c.ClubCapacity = new int[nt];
		c.ClubCoach = new string[nt];
		c.ClubCoachCountry = new int[nt];
		c.ClubColor1 = new string[nt];
		c.ClubColor2 = new string[nt];
		c.ClubPlayerStart = new int[nt];
		c.ClubPlayerCount = new int[nt];
		c.ClubYouthStart = new int[nt];
		c.ClubYouthCount = new int[nt];
		c.PlayerName = new string[nJog];
		c.PlayerCountry = new int[nJog];
		c.PlayerAge = new int[nJog];
		c.PlayerPosition = new int[nJog];
		c.PlayerSide = new int[nJog];
		c.PlayerStatus = new int[nJog];
		c.PlayerTrait1 = new int[nJog];
		c.PlayerTrait2 = new int[nJog];
		c.PlayerFlags = new int[nJog];
		int cursor = 0;
		for (int i = 0; i < nt; i++)
		{
			var t = banco.Clubs[i];
			c.ClubSlug[i] = t.Slug ?? "";
			c.ClubName[i] = t.Name ?? "";
			c.ClubCountry[i] = t.CountryId;
			c.ClubState[i] = t.StateId;
			c.ClubLevel[i] = t.Level;
			c.ClubReputation[i] = t.Reputation;
			c.ClubStadium[i] = t.Stadium ?? "";
			c.ClubCapacity[i] = t.Capacity;
			c.ClubCoach[i] = t.Coach ?? "";
			c.ClubCoachCountry[i] = t.CoachCountryId;
			c.ClubColor1[i] = string.IsNullOrWhiteSpace(t.BackgroundColor) ? "#ffffff" : t.BackgroundColor;
			c.ClubColor2[i] = string.IsNullOrWhiteSpace(t.TextColor) ? "#000000" : t.TextColor;
			c.ClubPlayerStart[i] = cursor;
			c.ClubPlayerCount[i] = t.Players.Count;
			foreach (var j in t.Players) c.WritePlayer(cursor++, j);
			c.ClubYouthStart[i] = cursor;
			c.ClubYouthCount[i] = t.Youth.Count;
			foreach (var j in t.Youth) c.WritePlayer(cursor++, j);
		}
		c.WriteNames(banco);
		return c;
	}

	void WriteNames(CatalogData banco)
	{
		var pools = banco.Names ?? new List<NamePool>();
		NameCode = new string[pools.Count];
		GivenStart = new int[pools.Count];
		GivenCount = new int[pools.Count];
		SurnameStart = new int[pools.Count];
		SurnameCount = new int[pools.Count];
		int given = 0;
		int surnames = 0;
		for (int i = 0; i < pools.Count; i++)
		{
			given += pools[i].Given?.Count ?? 0;
			surnames += pools[i].Surnames?.Count ?? 0;
		}
		GivenName = new string[given];
		Surname = new string[surnames];
		int g = 0;
		int s = 0;
		for (int i = 0; i < pools.Count; i++)
		{
			NameCode[i] = pools[i].Code ?? "";
			GivenStart[i] = g;
			GivenCount[i] = pools[i].Given?.Count ?? 0;
			for (int k = 0; k < GivenCount[i]; k++) GivenName[g++] = pools[i].Given[k] ?? "";
			SurnameStart[i] = s;
			SurnameCount[i] = pools[i].Surnames?.Count ?? 0;
			for (int k = 0; k < SurnameCount[i]; k++) Surname[s++] = pools[i].Surnames[k] ?? "";
		}
		NameTable = (banco.NameTable ?? new List<string>()).ToArray();
	}

	void WritePlayer(int i, PlayerRecord j)
	{
		PlayerName[i] = j.Name ?? "";
		PlayerCountry[i] = j.CountryId;
		PlayerAge[i] = j.Age;
		PlayerPosition[i] = j.Position;
		PlayerSide[i] = j.Side;
		PlayerStatus[i] = j.LineupStatus;
		PlayerTrait1[i] = j.PrimaryTrait;
		PlayerTrait2[i] = j.SecondaryTrait;
		PlayerFlags[i] = (j.IsStar ? 1 : 0) | (j.WorldClass ? 2 : 0);
	}

	public CatalogData ToData()
	{
		if (Format != "catalog" || Version < 2) throw new InvalidDataException(Format ?? "");
		var b = new CatalogData();
		for (int i = 0; i < CountryId.Length; i++)
			b.Countries.Add(new CountryRecord { Id = CountryId[i], Code = CountryCode[i], Name = CountryName[i], Confederation = CountryConfederation[i], Level = CountryLevel[i] });
		for (int i = 0; i < StateId.Length; i++)
			b.States.Add(new StateRecord { Id = StateId[i], Abbreviation = Abbreviation[i], Name = StateName[i] });
		for (int i = 0; i < LeagueCountry.Length; i++)
			b.Leagues.Add(new LeagueRecord
			{
				CountryId = LeagueCountry[i],
				Division = LeagueDivision[i],
				Format = LeagueFormat[i],
				TieBreak = LeagueTieBreak[i],
				TeamCount = LeagueTeamCount[i],
				HomeAndAway = (LeagueFlags[i] & 1) != 0,
				MatchesInsideGroup = (LeagueFlags[i] & 2) != 0,
				RankByOverallTable = (LeagueFlags[i] & 4) != 0,
				GroupCount = LeagueGroups[i],
				RelegatedCount = LeagueRelegated[i],
				DirectRelegation = LeagueDirectRelegation[i],
				KnockoutTeamCount = LeagueKnockout[i],
				Name = LeagueName[i],
				DivisionName = LeagueDivisionName[i]
			});
		int cursorF = 0;
		for (int i = 0; i < StateCupId.Length; i++)
		{
			var fin = new List<int>();
			int n = StateCupFinalCount[i];
			for (int k = 0; k < n && cursorF < StateCupFinals.Length; k++) fin.Add(StateCupFinals[cursorF++]);
			b.StateCups.Add(new StateCupRecord
			{
				Id = StateCupId[i],
				Division = StateCupDivision[i],
				Format = StateCupFormat[i],
				RelegatedCount = StateCupRelegated[i],
				TieBreak = StateCupTieBreak[i],
				FinalRounds = fin
			});
		}
		for (int i = 0; i < ClubSlug.Length; i++)
		{
			var t = new ClubRecord
			{
				Slug = ClubSlug[i],
				Name = ClubName[i],
				CountryId = ClubCountry[i],
				StateId = ClubState[i],
				Level = ClubLevel[i],
				Reputation = ClubReputation[i],
				Stadium = ClubStadium[i],
				Capacity = ClubCapacity[i],
				Coach = ClubCoach[i],
				CoachCountryId = ClubCoachCountry[i],
				BackgroundColor = ClubColor1[i],
				TextColor = ClubColor2[i]
			};
			for (int k = 0; k < ClubPlayerCount[i]; k++) t.Players.Add(LerJogador(ClubPlayerStart[i] + k));
			for (int k = 0; k < ClubYouthCount[i]; k++) t.Youth.Add(LerJogador(ClubYouthStart[i] + k));
			b.Clubs.Add(t);
		}
		if (NameCode != null)
		{
			for (int i = 0; i < NameCode.Length; i++)
			{
				var pool = new NamePool { Code = NameCode[i] ?? "" };
				int gs = i < GivenStart.Length ? GivenStart[i] : 0;
				int gc = i < GivenCount.Length ? GivenCount[i] : 0;
				for (int k = 0; k < gc && gs + k < GivenName.Length; k++) pool.Given.Add(GivenName[gs + k] ?? "");
				int ss = i < SurnameStart.Length ? SurnameStart[i] : 0;
				int sc = i < SurnameCount.Length ? SurnameCount[i] : 0;
				for (int k = 0; k < sc && ss + k < Surname.Length; k++) pool.Surnames.Add(Surname[ss + k] ?? "");
				b.Names.Add(pool);
			}
		}
		if (NameTable != null) b.NameTable.AddRange(NameTable);
		return b;
	}

	PlayerRecord LerJogador(int i) => new()
	{
		Name = PlayerName[i],
		CountryId = PlayerCountry[i],
		Age = PlayerAge[i],
		Position = PlayerPosition[i],
		Side = PlayerSide[i],
		LineupStatus = PlayerStatus[i],
		PrimaryTrait = PlayerTrait1[i],
		SecondaryTrait = PlayerTrait2[i],
		IsStar = (PlayerFlags[i] & 1) != 0,
		WorldClass = (PlayerFlags[i] & 2) != 0
	};
}
