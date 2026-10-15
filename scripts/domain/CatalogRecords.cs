namespace Brasfoot;

public sealed class CatalogData
{
	public List<CountryRecord> Countries { get; set; } = new();
	public List<StateRecord> States { get; set; } = new();
	public List<LeagueRecord> Leagues { get; set; } = new();
	public List<StateCupRecord> StateCups { get; set; } = new();
	public List<ClubRecord> Clubs { get; set; } = new();
	public List<NamePool> Names { get; set; } = new();
	public List<string> NameTable { get; set; } = new();

	public CountryRecord FindCountry(int id) => Countries.FirstOrDefault(p => p.Id == id);
	public string CountryCode(int id) => FindCountry(id)?.Code ?? "";
	public string CountryName(int id) => FindCountry(id)?.Name ?? ("Country " + id);
	public string StateCode(int id)
	{
		if (id < 0 || id >= States.Count) return "";
		return States[id].Abbreviation ?? "";
	}
	public string StateName(int id)
	{
		if (id < 0 || id >= States.Count) return "";
		return States[id].Name;
	}

	public string ComposeName(int countryId, Random rng)
	{
		if (countryId == 221) countryId = 85;
		else if (countryId == 222) countryId = 72;
		else if (countryId == 223) countryId = 85;
		string code = CountryCode(countryId);
		var pool = Names.FirstOrDefault(n => n.Code == code);
		if (pool != null && pool.Given.Count > 0)
		{
			int index = pool.Given.Count >= 1000 && rng.Next(2) == 0 ? rng.Next(500) : rng.Next(pool.Given.Count);
			if (index == 0 && pool.Given.Count > 1) index = 1;
			string name = pool.Given[index] ?? "";
			int words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
			if (words == 1 && pool.Surnames.Count > 2)
			{
				int surname = rng.Next(pool.Surnames.Count);
				if (surname == 0) surname = 1;
				if (pool.Surnames[surname] != name) name = name + " " + pool.Surnames[surname];
			}
			else if (words == 2 && name.Length <= 12 && rng.Next(2) == 0 && pool.Surnames.Count > 2)
			{
				int surname = rng.Next(pool.Surnames.Count);
				if (surname == 0) surname = 1;
				string extra = pool.Surnames[surname] ?? "";
				if (extra != name && extra.Length <= 6) name = name + " " + extra;
			}
			if (!string.IsNullOrWhiteSpace(name)) return name;
		}
		string fallback = FallbackName(countryId, rng);
		return string.IsNullOrWhiteSpace(fallback) ? "Player" : fallback;
	}

	string FallbackName(int countryId, Random rng)
	{
		if (NameTable.Count == 0) return "";
		int conf = FindCountry(countryId)?.Confederation ?? -1;
		int start;
		int count;
		if (conf == 0)
		{
			if (countryId is 97 or 62 or 100 or 101 or 145) { start = 8710; count = 1980; }
			else if (countryId is 3 or 15) { start = 10870; count = 1155; }
			else if (countryId == 104) { start = 12025; count = 1700; }
			else if (countryId == 72) { start = 13726; count = 901; }
			else if (countryId == 85) { start = 14627; count = 1830; }
			else if (countryId == 154) { start = 16458; count = 3528; }
			else if (countryId is 142 or 70 or 179 or 54) { start = 19986; count = 1340; }
			else if (countryId is 162 or 193 or 12 or 25) { start = 21327; count = 1429; }
			else if (countryId is 159 or 152 or 88 or 160 or 63 or 64) { start = 22756; count = 1257; }
			else if (countryId is 52 or 27) { start = 24013; count = 1774; }
			else if (countryId is 78 or 44) { start = 25787; count = 784; }
			else if (countryId == 21) { start = 13726; count = 2732; }
			else if (countryId == 65) { start = 5939; count = 2771; }
			else { start = 19986; count = 5801; }
		}
		else if (conf == 1)
		{
			if (countryId == 29) { start = 1; count = 5938; }
			else { start = 5939; count = 2771; }
		}
		else if (conf == 2)
		{
			if (countryId is 57 or 10 or 129 or 116 or 178 or 190) { start = 31211; count = 2554; }
			else { start = 26571; count = 2398; }
		}
		else if (conf == 3)
		{
			if (countryId is 48 or 49) { start = 29578; count = 560; }
			else if (countryId == 107) { start = 30138; count = 1072; }
			else if (countryId == 43) { start = 28970; count = 607; }
			else if (countryId == 14) { start = 8710; count = 1980; }
			else if (countryId is 9 or 18 or 59 or 89 or 99 or 98 or 108 or 111 or 115 or 117 or 144 or 146 or 103 or 39) { start = 31211; count = 2554; }
			else { start = 28970; count = 4795; }
		}
		else if (conf == 4)
		{
			if (countryId is 68 or 38 or 106) { start = 8710; count = 1980; }
			else { start = 5939; count = 2771; }
		}
		else if (conf == 5) { start = 8710; count = 1980; }
		else { start = 1; count = 20000; }
		if (count <= 0) return "";
		int index = rng.Next(count) + start;
		if (index < 0 || index >= NameTable.Count) return "";
		return NameTable[index];
	}
}

public sealed class NamePool
{
	public string Code { get; set; } = "";
	public List<string> Given { get; set; } = new();
	public List<string> Surnames { get; set; } = new();
}

public sealed class CountryRecord
{
	public int Id { get; set; }
	public string Code { get; set; } = "";
	public string Name { get; set; } = "";
	public int Confederation { get; set; }
	public int Level { get; set; }
}

public sealed class StateRecord
{
	public int Id { get; set; }
	public string Abbreviation { get; set; } = "";
	public string Name { get; set; } = "";
}

public sealed class LeagueRecord
{
	public int CountryId { get; set; }
	public int Division { get; set; }
	public int Format { get; set; }
	public int TieBreak { get; set; }
	public int TeamCount { get; set; }
	public bool HomeAndAway { get; set; }
	public int GroupCount { get; set; }
	public bool MatchesInsideGroup { get; set; }
	public int RelegatedCount { get; set; }
	public int DirectRelegation { get; set; }
	public int KnockoutTeamCount { get; set; }
	public bool RankByOverallTable { get; set; }
	public string Name { get; set; } = "";
	public string DivisionName { get; set; } = "";
}

public sealed class StateCupRecord
{
	public int Id { get; set; }
	public int Division { get; set; }
	public int Format { get; set; }
	public int RelegatedCount { get; set; }
	public int TieBreak { get; set; }
	public List<int> FinalRounds { get; set; } = new();
}

public sealed class ClubRecord
{
	public string Slug { get; set; } = "";
	public string Name { get; set; } = "";
	public int CountryId { get; set; }
	public int StateId { get; set; }
	public int Level { get; set; }
	public int Reputation { get; set; }
	public string Stadium { get; set; } = "";
	public int Capacity { get; set; }
	public string Coach { get; set; } = "";
	public int CoachCountryId { get; set; }
	public string BackgroundColor { get; set; } = "#ffffff";
	public string TextColor { get; set; } = "#000000";
	public List<PlayerRecord> Players { get; set; } = new();
	public List<PlayerRecord> Youth { get; set; } = new();
}

public sealed class PlayerRecord
{
	public string Name { get; set; } = "";
	public int CountryId { get; set; }
	public int Age { get; set; }
	public int Position { get; set; }
	public int Side { get; set; }
	public int LineupStatus { get; set; }
	public int PrimaryTrait { get; set; }
	public int SecondaryTrait { get; set; }
	public bool IsStar { get; set; }
	public bool WorldClass { get; set; }
}
