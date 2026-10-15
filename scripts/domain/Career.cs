namespace Brasfoot;

public sealed class Player
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public int CountryId { get; set; }
	public int Age { get; set; }
	public int Position { get; set; }
	public int Side { get; set; }
	public int PrimaryTrait { get; set; }
	public int SecondaryTrait { get; set; }
	public bool IsStar { get; set; }
	public bool WorldClass { get; set; }
	public bool PreferredStarter { get; set; }
	public int Strength { get; set; }
	public int Fitness { get; set; } = 100;
	public int Morale { get; set; } = 70;
	public int Wage { get; set; }
	public int Price { get; set; }
	public int Goals { get; set; }
	public int MatchesPlayed { get; set; }
	public int YellowCards { get; set; }
	public int YellowStreak { get; set; }
	public int RedCards { get; set; }
	public int BanMatches { get; set; }
	public int InjuryMatches { get; set; }
	public int ContractYears { get; set; } = 3;
	public bool ListedForSale { get; set; }
	public double RatingSum { get; set; }
	public int RatingCount { get; set; }

	public bool IsAvailable => BanMatches <= 0 && InjuryMatches <= 0;

	public string TraitSummary => Rules.TraitCode(PrimaryTrait) + "/" + Rules.TraitCode(SecondaryTrait);
}

public sealed class Club
{
	public string Slug { get; set; } = "";
	public string Name { get; set; } = "";
	public int CountryId { get; set; }
	public int StateId { get; set; }
	public int Level { get; set; }
	public int Reputation { get; set; }
	public string Stadium { get; set; } = "";
	public int Capacity { get; set; }
	public string BackgroundColor { get; set; } = "#ffffff";
	public string TextColor { get; set; } = "#111111";
	public long Funds { get; set; }
	public int Confidence { get; set; } = 60;
	public int Division { get; set; }
	public string Formation { get; set; } = "4-4-2";
	public string Tactic { get; set; } = "balanced";
	public bool IsHuman { get; set; }
	public string Coach { get; set; } = "";
	public int CoachCountryId { get; set; }
	public bool HasTrained { get; set; }
	public int Wins { get; set; }
	public int Draws { get; set; }
	public int Losses { get; set; }
	public List<Player> Squad { get; set; } = new();
	public List<int> Lineup { get; set; } = new();

	public int AverageStrength => Squad.Count == 0 ? 1 : (int)Squad.Average(j => j.Strength);

	public Player FindPlayer(int id) => Squad.FirstOrDefault(j => j.Id == id);
}

public sealed class MatchEvent
{
	public int Minute { get; set; }
	public string Kind { get; set; } = "";
	public string Description { get; set; } = "";
	public string ClubSlug { get; set; } = "";
	public int Player { get; set; }
	public int Extra { get; set; }
}

public sealed class Match
{
	public string Id { get; set; } = "";
	public string CompetitionId { get; set; } = "";
	public string HomeSlug { get; set; } = "";
	public string AwaySlug { get; set; } = "";
	public string MatchDate { get; set; } = "";
	public int Round { get; set; }
	public int Stage { get; set; }
	public int Bracket { get; set; }
	public int Leg { get; set; } = 1;
	public bool Played { get; set; }
	public int HomeGoals { get; set; }
	public int AwayGoals { get; set; }
	public int HomePossession { get; set; } = 50;
	public List<MatchEvent> Events { get; set; } = new();
}

public sealed class Competition
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public string Kind { get; set; } = "league";
	public int CountryId { get; set; } = -1;
	public int Division { get; set; }
	public int RelegationSpots { get; set; } = 2;
	public int Groups { get; set; }
	public int QualifiersPerGroup { get; set; } = 2;
	public List<string> ClubsInCompetition { get; set; } = new();
	public List<Match> Matches { get; set; } = new();
	public bool Finished { get; set; }
	public string Champion { get; set; } = "";
}

public sealed class CareerState
{
	public int Version { get; set; } = 1;
	public int Year { get; set; } = 2026;
	public string Today { get; set; } = "2026-01-01";
	public int Seed { get; set; } = 1;
	public int NextId { get; set; } = 1;
	public bool PlayStateCup { get; set; } = true;
	public bool PlayNationalCup { get; set; } = true;
	public bool PlayContinental { get; set; } = true;
	public int Speed { get; set; } = 2;
	public bool SoundEnabled { get; set; } = true;
	public string LastPaidMonth { get; set; } = "";
	public List<Club> ClubsInCompetition { get; set; } = new();
	public List<Competition> Competitions { get; set; } = new();
	public List<string> HumanClubIds { get; set; } = new();
	public List<string> History { get; set; } = new();

	public CatalogData CatalogData { get; set; }

	public Club FindClub(string r) => ClubsInCompetition.FirstOrDefault(c => c.Slug == r);
	public IEnumerable<Club> HumanClubs() => HumanClubIds.Select(FindClub).Where(c => c != null);
	public bool IsHumanClub(string r) => HumanClubIds.Contains(r);

	public void Save(string caminho) => CareerFile.Save(this, caminho);

	public static CareerState Open(string caminho, CatalogData banco) => CareerFile.Open(caminho, banco);
}

