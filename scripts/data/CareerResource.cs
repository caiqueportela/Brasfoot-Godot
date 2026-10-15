using Godot;

namespace Brasfoot;

[GlobalClass]
public partial class CareerResource : Resource
{
	[Export] public string Format { get; set; } = "";
	[Export] public int Version { get; set; }
	[Export] public int Year { get; set; }
	[Export] public string Today { get; set; } = "";
	[Export] public int Seed { get; set; }
	[Export] public int NextId { get; set; }
	[Export] public int Options { get; set; }
	[Export] public int Speed { get; set; } = 2;
	[Export] public string LastPaidMonth { get; set; } = "";
	[Export] public string[] Humans { get; set; } = System.Array.Empty<string>();
	[Export] public string[] History { get; set; } = System.Array.Empty<string>();

	[Export] public string[] ClubSlug { get; set; } = System.Array.Empty<string>();
	[Export] public string[] ClubName { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubCountry { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubState { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubLevel { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubReputation { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubStadium { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubCapacity { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubColor1 { get; set; } = System.Array.Empty<string>();
	[Export] public string[] ClubColor2 { get; set; } = System.Array.Empty<string>();
	[Export] public long[] ClubFunds { get; set; } = System.Array.Empty<long>();
	[Export] public int[] ClubConfidence { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubDivision { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubFormation { get; set; } = System.Array.Empty<string>();
	[Export] public string[] ClubTactic { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubFlags { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubCoach { get; set; } = System.Array.Empty<string>();
	[Export] public int[] ClubCoachCountry { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubWins { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubDraws { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubLosses { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubPlayerStart { get; set; } = System.Array.Empty<int>();
	[Export] public int[] ClubPlayerCount { get; set; } = System.Array.Empty<int>();
	[Export] public string[] ClubSchedule { get; set; } = System.Array.Empty<string>();

	[Export] public string[] PlayerName { get; set; } = System.Array.Empty<string>();
	[Export] public int[] PlayerId { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerCountry { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerAge { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerPosition { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerSide { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerTrait1 { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerTrait2 { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerFlags { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerStrength { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerFitness { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerMorale { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerWage { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerValue { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerGoals { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerMatches { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerYellows { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerYellowRun { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerReds { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerSuspension { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerInjury { get; set; } = System.Array.Empty<int>();
	[Export] public int[] PlayerContract { get; set; } = System.Array.Empty<int>();
	[Export] public float[] PlayerRating { get; set; } = System.Array.Empty<float>();
	[Export] public int[] PlayerRatingCount { get; set; } = System.Array.Empty<int>();

	[Export] public string[] CompetitionId { get; set; } = System.Array.Empty<string>();
	[Export] public string[] CompetitionName { get; set; } = System.Array.Empty<string>();
	[Export] public string[] CompetitionKind { get; set; } = System.Array.Empty<string>();
	[Export] public int[] CompetitionCountry { get; set; } = System.Array.Empty<int>();
	[Export] public int[] CompetitionDivision { get; set; } = System.Array.Empty<int>();
	[Export] public int[] CompetitionRelegated { get; set; } = System.Array.Empty<int>();
	[Export] public int[] CompetitionGroups { get; set; } = System.Array.Empty<int>();
	[Export] public int[] CompetitionQualifiers { get; set; } = System.Array.Empty<int>();
	[Export] public string[] CompetitionClubs { get; set; } = System.Array.Empty<string>();
	[Export] public int[] CompetitionFlags { get; set; } = System.Array.Empty<int>();
	[Export] public string[] CompetitionChampion { get; set; } = System.Array.Empty<string>();

	[Export] public int[] MatchCompetition { get; set; } = System.Array.Empty<int>();
	[Export] public string[] MatchId { get; set; } = System.Array.Empty<string>();
	[Export] public string[] MatchHome { get; set; } = System.Array.Empty<string>();
	[Export] public string[] MatchAway { get; set; } = System.Array.Empty<string>();
	[Export] public string[] MatchDate { get; set; } = System.Array.Empty<string>();
	[Export] public int[] MatchRound { get; set; } = System.Array.Empty<int>();
	[Export] public int[] MatchPhase { get; set; } = System.Array.Empty<int>();
	[Export] public int[] MatchBracket { get; set; } = System.Array.Empty<int>();
	[Export] public int[] MatchLeg { get; set; } = System.Array.Empty<int>();
	[Export] public int[] MatchFlags { get; set; } = System.Array.Empty<int>();
	[Export] public int[] MatchHomeGoals { get; set; } = System.Array.Empty<int>();
	[Export] public int[] MatchAwayGoals { get; set; } = System.Array.Empty<int>();
	[Export] public int[] MatchPossession { get; set; } = System.Array.Empty<int>();

	public static CareerResource FromState(CareerState est)
	{
		var r = new CareerResource
		{
			Format = "career",
			Version = Paths.FormatVersion,
			Year = est.Year,
			Today = est.Today ?? "",
			Seed = est.Seed,
			NextId = est.NextId,
			Options = (est.PlayStateCup ? 1 : 0) | (est.PlayNationalCup ? 2 : 0) | (est.PlayContinental ? 4 : 0) | (est.SoundEnabled ? 8 : 0),
			Speed = est.Speed,
			LastPaidMonth = est.LastPaidMonth ?? "",
			Humans = est.HumanClubIds.ToArray(),
			History = est.History.ToArray()
		};
		int nc = est.ClubsInCompetition.Count;
		int nj = est.ClubsInCompetition.Sum(c => c.Squad.Count);
		r.ClubSlug = new string[nc];
		r.ClubName = new string[nc];
		r.ClubCountry = new int[nc];
		r.ClubState = new int[nc];
		r.ClubLevel = new int[nc];
		r.ClubReputation = new int[nc];
		r.ClubStadium = new string[nc];
		r.ClubCapacity = new int[nc];
		r.ClubColor1 = new string[nc];
		r.ClubColor2 = new string[nc];
		r.ClubFunds = new long[nc];
		r.ClubConfidence = new int[nc];
		r.ClubDivision = new int[nc];
		r.ClubFormation = new string[nc];
		r.ClubTactic = new string[nc];
		r.ClubFlags = new int[nc];
		r.ClubCoach = new string[nc];
		r.ClubCoachCountry = new int[nc];
		r.ClubWins = new int[nc];
		r.ClubDraws = new int[nc];
		r.ClubLosses = new int[nc];
		r.ClubPlayerStart = new int[nc];
		r.ClubPlayerCount = new int[nc];
		r.ClubSchedule = new string[nc];
		r.PlayerName = new string[nj];
		r.PlayerId = new int[nj];
		r.PlayerCountry = new int[nj];
		r.PlayerAge = new int[nj];
		r.PlayerPosition = new int[nj];
		r.PlayerSide = new int[nj];
		r.PlayerTrait1 = new int[nj];
		r.PlayerTrait2 = new int[nj];
		r.PlayerFlags = new int[nj];
		r.PlayerStrength = new int[nj];
		r.PlayerFitness = new int[nj];
		r.PlayerMorale = new int[nj];
		r.PlayerWage = new int[nj];
		r.PlayerValue = new int[nj];
		r.PlayerGoals = new int[nj];
		r.PlayerMatches = new int[nj];
		r.PlayerYellows = new int[nj];
		r.PlayerYellowRun = new int[nj];
		r.PlayerReds = new int[nj];
		r.PlayerSuspension = new int[nj];
		r.PlayerInjury = new int[nj];
		r.PlayerContract = new int[nj];
		r.PlayerRating = new float[nj];
		r.PlayerRatingCount = new int[nj];
		int cursor = 0;
		for (int i = 0; i < nc; i++)
		{
			var c = est.ClubsInCompetition[i];
			r.ClubSlug[i] = c.Slug ?? "";
			r.ClubName[i] = c.Name ?? "";
			r.ClubCountry[i] = c.CountryId;
			r.ClubState[i] = c.StateId;
			r.ClubLevel[i] = c.Level;
			r.ClubReputation[i] = c.Reputation;
			r.ClubStadium[i] = c.Stadium ?? "";
			r.ClubCapacity[i] = c.Capacity;
			r.ClubColor1[i] = c.BackgroundColor ?? "";
			r.ClubColor2[i] = c.TextColor ?? "";
			r.ClubFunds[i] = c.Funds;
			r.ClubConfidence[i] = c.Confidence;
			r.ClubDivision[i] = c.Division;
			r.ClubFormation[i] = c.Formation ?? "4-4-2";
			r.ClubTactic[i] = c.Tactic ?? "balanced";
			r.ClubFlags[i] = (c.IsHuman ? 1 : 0) | (c.HasTrained ? 2 : 0);
			r.ClubCoach[i] = c.Coach ?? "";
			r.ClubCoachCountry[i] = c.CoachCountryId;
			r.ClubWins[i] = c.Wins;
			r.ClubDraws[i] = c.Draws;
			r.ClubLosses[i] = c.Losses;
			r.ClubPlayerStart[i] = cursor;
			r.ClubPlayerCount[i] = c.Squad.Count;
			r.ClubSchedule[i] = c.Lineup == null || c.Lineup.Count == 0 ? "" : string.Join(",", c.Lineup);
			foreach (var j in c.Squad)
			{
				r.PlayerName[cursor] = j.Name ?? "";
				r.PlayerId[cursor] = j.Id;
				r.PlayerCountry[cursor] = j.CountryId;
				r.PlayerAge[cursor] = j.Age;
				r.PlayerPosition[cursor] = j.Position;
				r.PlayerSide[cursor] = j.Side;
				r.PlayerTrait1[cursor] = j.PrimaryTrait;
				r.PlayerTrait2[cursor] = j.SecondaryTrait;
				r.PlayerFlags[cursor] = (j.IsStar ? 1 : 0) | (j.WorldClass ? 2 : 0) | (j.PreferredStarter ? 4 : 0) | (j.ListedForSale ? 8 : 0);
				r.PlayerStrength[cursor] = j.Strength;
				r.PlayerFitness[cursor] = j.Fitness;
				r.PlayerMorale[cursor] = j.Morale;
				r.PlayerWage[cursor] = j.Wage;
				r.PlayerValue[cursor] = j.Price;
				r.PlayerGoals[cursor] = j.Goals;
				r.PlayerMatches[cursor] = j.MatchesPlayed;
				r.PlayerYellows[cursor] = j.YellowCards;
				r.PlayerYellowRun[cursor] = j.YellowStreak;
				r.PlayerReds[cursor] = j.RedCards;
				r.PlayerSuspension[cursor] = j.BanMatches;
				r.PlayerInjury[cursor] = j.InjuryMatches;
				r.PlayerContract[cursor] = j.ContractYears;
				r.PlayerRating[cursor] = (float)j.RatingSum;
				r.PlayerRatingCount[cursor] = j.RatingCount;
				cursor++;
			}
		}

		int nk = est.Competitions.Count;
		int nm = est.Competitions.Sum(k => k.Matches.Count);
		r.CompetitionId = new string[nk];
		r.CompetitionName = new string[nk];
		r.CompetitionKind = new string[nk];
		r.CompetitionCountry = new int[nk];
		r.CompetitionDivision = new int[nk];
		r.CompetitionRelegated = new int[nk];
		r.CompetitionGroups = new int[nk];
		r.CompetitionQualifiers = new int[nk];
		r.CompetitionClubs = new string[nk];
		r.CompetitionFlags = new int[nk];
		r.CompetitionChampion = new string[nk];
		r.MatchCompetition = new int[nm];
		r.MatchId = new string[nm];
		r.MatchHome = new string[nm];
		r.MatchAway = new string[nm];
		r.MatchDate = new string[nm];
		r.MatchRound = new int[nm];
		r.MatchPhase = new int[nm];
		r.MatchBracket = new int[nm];
		r.MatchLeg = new int[nm];
		r.MatchFlags = new int[nm];
		r.MatchHomeGoals = new int[nm];
		r.MatchAwayGoals = new int[nm];
		r.MatchPossession = new int[nm];
		int mj = 0;
		for (int i = 0; i < nk; i++)
		{
			var k = est.Competitions[i];
			r.CompetitionId[i] = k.Id ?? "";
			r.CompetitionName[i] = k.Name ?? "";
			r.CompetitionKind[i] = k.Kind ?? "league";
			r.CompetitionCountry[i] = k.CountryId;
			r.CompetitionDivision[i] = k.Division;
			r.CompetitionRelegated[i] = k.RelegationSpots;
			r.CompetitionGroups[i] = k.Groups;
			r.CompetitionQualifiers[i] = k.QualifiersPerGroup;
			r.CompetitionClubs[i] = string.Join("\u001f", k.ClubsInCompetition);
			r.CompetitionFlags[i] = k.Finished ? 1 : 0;
			r.CompetitionChampion[i] = k.Champion ?? "";
			foreach (var p in k.Matches)
			{
				r.MatchCompetition[mj] = i;
				r.MatchId[mj] = p.Id ?? "";
				r.MatchHome[mj] = p.HomeSlug ?? "";
				r.MatchAway[mj] = p.AwaySlug ?? "";
				r.MatchDate[mj] = p.MatchDate ?? "";
				r.MatchRound[mj] = p.Round;
				r.MatchPhase[mj] = p.Stage;
				r.MatchBracket[mj] = p.Bracket;
				r.MatchLeg[mj] = p.Leg;
				r.MatchFlags[mj] = p.Played ? 1 : 0;
				r.MatchHomeGoals[mj] = p.HomeGoals;
				r.MatchAwayGoals[mj] = p.AwayGoals;
				r.MatchPossession[mj] = p.HomePossession;
				mj++;
			}
		}
		return r;
	}

	public CareerState ToState(CatalogData banco)
	{
		if (Format != "career" || Version < 2) throw new InvalidDataException(Format ?? "");
		var est = new CareerState
		{
			CatalogData = banco,
			Version = Version,
			Year = Year,
			Today = Today,
			Seed = Seed,
			NextId = NextId,
			PlayStateCup = (Options & 1) != 0,
			PlayNationalCup = (Options & 2) != 0,
			PlayContinental = (Options & 4) != 0,
			SoundEnabled = (Options & 8) != 0,
			Speed = Speed <= 0 ? 2 : Speed,
			LastPaidMonth = LastPaidMonth ?? "",
			HumanClubIds = Humans?.ToList() ?? new List<string>(),
			History = History?.ToList() ?? new List<string>()
		};
		var jogadores = new Player[PlayerName.Length];
		for (int i = 0; i < PlayerName.Length; i++)
		{
			jogadores[i] = new Player
			{
				Name = PlayerName[i],
				Id = PlayerId[i],
				CountryId = PlayerCountry[i],
				Age = PlayerAge[i],
				Position = PlayerPosition[i],
				Side = PlayerSide[i],
				PrimaryTrait = PlayerTrait1[i],
				SecondaryTrait = PlayerTrait2[i],
				IsStar = (PlayerFlags[i] & 1) != 0,
				WorldClass = (PlayerFlags[i] & 2) != 0,
				PreferredStarter = (PlayerFlags[i] & 4) != 0,
				ListedForSale = (PlayerFlags[i] & 8) != 0,
				Strength = PlayerStrength[i],
				Fitness = PlayerFitness[i],
				Morale = PlayerMorale[i],
				Wage = PlayerWage[i],
				Price = PlayerValue[i],
				Goals = PlayerGoals[i],
				MatchesPlayed = PlayerMatches[i],
				YellowCards = PlayerYellows[i],
				YellowStreak = PlayerYellowRun[i],
				RedCards = PlayerReds[i],
				BanMatches = PlayerSuspension[i],
				InjuryMatches = PlayerInjury[i],
				ContractYears = PlayerContract[i],
				RatingSum = PlayerRating[i],
				RatingCount = PlayerRatingCount[i]
			};
		}
		for (int i = 0; i < ClubSlug.Length; i++)
		{
			var c = new Club
			{
				Slug = ClubSlug[i],
				Name = ClubName[i],
				CountryId = ClubCountry[i],
				StateId = ClubState[i],
				Level = ClubLevel[i],
				Reputation = ClubReputation[i],
				Stadium = ClubStadium[i],
				Capacity = ClubCapacity[i],
				BackgroundColor = ClubColor1[i],
				TextColor = ClubColor2[i],
				Funds = ClubFunds[i],
				Confidence = ClubConfidence[i],
				Division = ClubDivision[i],
				Formation = ClubFormation[i],
				Tactic = ClubTactic[i],
				IsHuman = (ClubFlags[i] & 1) != 0,
				HasTrained = (ClubFlags[i] & 2) != 0,
				Coach = ClubCoach[i],
				CoachCountryId = ClubCoachCountry[i],
				Wins = ClubWins[i],
				Draws = ClubDraws[i],
				Losses = ClubLosses[i]
			};
			for (int k = 0; k < ClubPlayerCount[i]; k++) c.Squad.Add(jogadores[ClubPlayerStart[i] + k]);
			if (!string.IsNullOrEmpty(ClubSchedule[i]))
				c.Lineup = ClubSchedule[i].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
			est.ClubsInCompetition.Add(c);
		}
		var comps = new Competition[CompetitionId.Length];
		for (int i = 0; i < CompetitionId.Length; i++)
		{
			comps[i] = new Competition
			{
				Id = CompetitionId[i],
				Name = CompetitionName[i],
				Kind = CompetitionKind[i],
				CountryId = CompetitionCountry[i],
				Division = CompetitionDivision[i],
				RelegationSpots = CompetitionRelegated[i],
				Groups = CompetitionGroups[i],
				QualifiersPerGroup = CompetitionQualifiers[i],
				ClubsInCompetition = string.IsNullOrEmpty(CompetitionClubs[i]) ? new List<string>() : CompetitionClubs[i].Split('\u001f').ToList(),
				Finished = (CompetitionFlags[i] & 1) != 0,
				Champion = CompetitionChampion[i] ?? ""
			};
			est.Competitions.Add(comps[i]);
		}
		for (int i = 0; i < MatchId.Length; i++)
		{
			comps[MatchCompetition[i]].Matches.Add(new Match
			{
				Id = MatchId[i],
				CompetitionId = comps[MatchCompetition[i]].Id,
				HomeSlug = MatchHome[i],
				AwaySlug = MatchAway[i],
				MatchDate = MatchDate[i],
				Round = MatchRound[i],
				Stage = MatchPhase[i],
				Bracket = MatchBracket[i],
				Leg = MatchLeg[i],
				Played = (MatchFlags[i] & 1) != 0,
				HomeGoals = MatchHomeGoals[i],
				AwayGoals = MatchAwayGoals[i],
				HomePossession = MatchPossession[i]
			});
		}
		return est;
	}
}
