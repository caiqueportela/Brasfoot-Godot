namespace Brasfoot;

public static class SeasonBuilder
{
	static readonly int[] TimesFormula = { 6, 8, 10, 11, 12, 12, 14, 16, 16, 16, 20 };
	static readonly int[] GruposFormula = { 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 4 };

	public static int TeamsInFormat(int formula)
	{
		if (formula < 0 || formula >= TimesFormula.Length) return 12;
		return TimesFormula[formula];
	}

	public static List<(int Division, string Name, List<ClubRecord> Clubs)> Preview(CatalogData banco, int pais)
	{
		var ordenados = banco.Clubs.Where(t => t.CountryId == pais).OrderByDescending(t => t.Level).ThenByDescending(t => t.Reputation).ThenBy(t => t.Name).ToList();
		var fatias = SliceLeagues(banco, pais, ordenados);
		return fatias.Select(f =>
		{
			string nome = string.IsNullOrWhiteSpace(f.cfg?.DivisionName) ? ("Division " + f.divisao) : f.cfg.DivisionName;
			return (f.divisao, nome, f.times);
		}).ToList();
	}

	public static CareerState CreateCareer(CatalogData banco, IReadOnlyList<string> humanos, IReadOnlyList<string> tecnicos, int ano, int semente, bool estadual, bool copa, bool continental)
	{
		var rng = new Random(semente);
		var est = new CareerState
		{
			CatalogData = banco,
			Year = ano,
			Today = ano + "-01-01",
			Seed = semente,
			PlayStateCup = estadual,
			PlayNationalCup = copa,
			PlayContinental = continental,
			HumanClubIds = humanos.ToList()
		};
		var porPais = banco.Clubs.GroupBy(t => t.CountryId).ToDictionary(g => g.Key, g => g.ToList());
		var divisaoDe = new Dictionary<string, int>();
		var ligasPorPais = new Dictionary<int, List<(int divisao, List<ClubRecord> times, LeagueRecord cfg)>>();

		foreach (var par in porPais)
		{
			var ordenados = par.Value.OrderByDescending(t => t.Level).ThenByDescending(t => t.Reputation).ThenBy(t => t.Name).ToList();
			var fatias = SliceLeagues(banco, par.Key, ordenados);
			if (fatias.Count == 0) continue;
			ligasPorPais[par.Key] = fatias;
			foreach (var f in fatias)
				foreach (var t in f.times)
					divisaoDe[t.Slug] = f.divisao;
		}

		foreach (var time in banco.Clubs)
		{
			int div = divisaoDe.TryGetValue(time.Slug, out var d) ? d : 0;
			var clube = CreateClub(est, time, div, rng);
			if (humanos.Contains(time.Slug))
			{
				clube.IsHuman = true;
				int idx = humanos.ToList().IndexOf(time.Slug);
				if (idx >= 0 && idx < tecnicos.Count && !string.IsNullOrWhiteSpace(tecnicos[idx]))
					clube.Coach = tecnicos[idx].Trim();
				clube.Confidence = 68;
			}
			est.ClubsInCompetition.Add(clube);
		}

		foreach (var par in ligasPorPais)
		{
			string nomePais = banco.CountryName(par.Key);
			foreach (var f in par.Value)
			{
				string nomeDiv = string.IsNullOrWhiteSpace(f.cfg?.DivisionName) ? ("Division " + f.divisao) : f.cfg.DivisionName;
				string nome = string.IsNullOrWhiteSpace(f.cfg?.Name) ? (nomePais + " Championship") : f.cfg.Name;
				bool dois = f.cfg?.HomeAndAway ?? true;
				if (f.times.Count >= 18 && (f.cfg == null || f.cfg.KnockoutTeamCount == 0)) dois = true;
				int grupos = f.cfg?.GroupCount ?? 0;
				int mata = f.cfg?.KnockoutTeamCount ?? 0;
				if (mata == 1020) mata = grupos > 1 ? 4 : 8;
				int reba = f.cfg?.DirectRelegation ?? f.cfg?.RelegatedCount ?? 2;
				if (reba <= 0) reba = 2;
				var comp = new Competition
				{
					Id = "nac-" + par.Key + "-" + f.divisao,
					Name = nome + " - " + nomeDiv,
					Kind = grupos > 1 ? "groups" : (mata >= 4 ? "league_knockout" : "league"),
					CountryId = par.Key,
					Division = f.divisao,
					RelegationSpots = Math.Min(reba, Math.Max(1, f.times.Count / 5)),
					Groups = grupos,
					ClubsInCompetition = f.times.Select(t => t.Slug).ToList()
				};
				est.Competitions.Add(comp);
			}
		}

		if (estadual)
			CreateStateCups(est, banco, rng);
		if (copa)
			CreateCups(est);
		if (continental)
			CreateContinental(est, banco);

		Schedule(est);
		MatchEngine.PayMonth(est, est.Today);
		return est;
	}

	static List<(int divisao, List<ClubRecord> times, LeagueRecord cfg)> SliceLeagues(CatalogData banco, int pais, List<ClubRecord> ordenados)
	{
		var saida = new List<(int, List<ClubRecord>, LeagueRecord)>();
		var resto = ordenados.ToList();
		for (int div = 1; div <= 4 && resto.Count >= 6; div++)
		{
			var cfg = banco.Leagues.FirstOrDefault(l => l.CountryId == pais && l.Division == div);
			int n;
			if (cfg != null && cfg.TeamCount > 0 && cfg.TeamCount <= resto.Count)
				n = cfg.TeamCount;
			else if (resto.Count >= 20) n = 20;
			else if (resto.Count >= 18) n = 18;
			else if (resto.Count >= 16) n = 16;
			else if (resto.Count >= 14) n = 14;
			else if (resto.Count >= 12) n = 12;
			else if (resto.Count >= 10) n = 10;
			else if (resto.Count >= 8) n = 8;
			else if (div == 1 && resto.Count >= 6) n = resto.Count;
			else break;
			var fatia = resto.Take(n).ToList();
			resto = resto.Skip(n).ToList();
			saida.Add((div, fatia, cfg));
			if (cfg == null && resto.Count < 8 && div >= 1 && fatia.Count < 12) break;
		}
		return saida;
	}

	static Club CreateClub(CareerState est, ClubRecord time, int divisao, Random rng)
	{
		var c = new Club
		{
			Slug = time.Slug,
			Name = time.Name,
			CountryId = time.CountryId,
			StateId = time.StateId,
			Level = time.Level,
			Reputation = Math.Max(1, time.Reputation),
			Stadium = string.IsNullOrWhiteSpace(time.Stadium) ? "Stadium" : time.Stadium,
			Capacity = time.Capacity > 0 ? time.Capacity : 8000,
			BackgroundColor = string.IsNullOrWhiteSpace(time.BackgroundColor) ? "#1d7a32" : time.BackgroundColor,
			TextColor = string.IsNullOrWhiteSpace(time.TextColor) ? "#ffffff" : time.TextColor,
			Division = divisao,
			Coach = string.IsNullOrWhiteSpace(time.Coach) ? "Coach" : time.Coach,
			CoachCountryId = time.CoachCountryId,
			Confidence = 50 + time.Reputation * 6
		};
		foreach (var j in time.Players)
			c.Squad.Add(CreatePlayer(est, time, j, false, divisao, rng));
		foreach (var j in time.Youth.Take(8))
			c.Squad.Add(CreatePlayer(est, time, j, true, divisao, rng));
		long folha = c.Squad.Sum(j => (long)j.Wage);
		long baseCaixa = divisao switch { 1 => 12_000_000, 2 => 6_000_000, 3 => 3_000_000, 4 => 1_500_000, _ => 800_000 };
		c.Funds = baseCaixa + time.Level * 180_000L + c.Reputation * 250_000L;
		if (c.Funds < folha * 4) c.Funds = folha * 4;
		return c;
	}

	static Player CreatePlayer(CareerState est, ClubRecord time, PlayerRecord j, bool junior, int divisao, Random rng)
	{
		int forca = Rules.BaseStrength(time, j, rng);
		if (junior) forca = Math.Max(6, forca - 10);
		var jog = new Player
		{
			Id = est.NextId++,
			Name = string.IsNullOrWhiteSpace(j.Name) ? "Jogador" : j.Name,
			CountryId = j.CountryId,
			Age = Math.Clamp(junior ? Math.Min(j.Age, 20) : j.Age, 16, 45),
			Position = Math.Clamp(j.Position, 0, 4),
			Side = j.Side == 1 ? 1 : 0,
			PrimaryTrait = Math.Clamp(j.PrimaryTrait, 0, 13),
			SecondaryTrait = Math.Clamp(j.SecondaryTrait, 0, 13),
			IsStar = j.IsStar || j.WorldClass,
			WorldClass = j.WorldClass,
			PreferredStarter = j.LineupStatus == 1 && !junior,
			Strength = forca,
			Fitness = 78 + rng.Next(23),
			Morale = 60 + rng.Next(25),
			ContractYears = 1 + rng.Next(4),
			Wage = Rules.WageFor(forca, divisao, j.Position, j.Age, j.IsStar || j.WorldClass)
		};
		jog.Price = Rules.MarketValue(forca, time.Level, jog.Age, jog.Position, jog.IsStar, jog.PreferredStarter);
		return jog;
	}

	static void CreateStateCups(CareerState est, CatalogData banco, Random rng)
	{
		var br = est.ClubsInCompetition.Where(c => c.CountryId == 29 && c.Division > 0).ToList();
		foreach (var uf in br.Select(c => c.StateId).Distinct().OrderBy(x => x))
		{
			if (uf < 0) continue;
			var cfg = banco.StateCups.Where(e => e.Id == uf).OrderBy(e => e.Division).FirstOrDefault();
			int n = cfg != null ? TeamsInFormat(cfg.Format) : 12;
			var times = br.Where(c => c.StateId == uf).OrderByDescending(c => c.Level).ThenByDescending(c => c.Reputation).Take(n).ToList();
			if (times.Count < 6) continue;
			int grupos = cfg != null && cfg.Format >= 0 && cfg.Format < GruposFormula.Length ? GruposFormula[cfg.Format] : 0;
			if (grupos > 0 && times.Count / grupos < 3) grupos = 0;
			var comp = new Competition
			{
				Id = "est-" + uf,
				Name = banco.StateName(uf) + " Championship",
				Kind = grupos > 1 ? "groups" : "league",
				CountryId = 29,
				Division = 0,
				RelegationSpots = 0,
				Groups = grupos,
				ClubsInCompetition = times.Select(t => t.Slug).ToList()
			};
			est.Competitions.Add(comp);
		}
	}

	static void CreateCups(CareerState est)
	{
		foreach (var pais in est.ClubsInCompetition.Select(c => c.CountryId).Distinct())
		{
			var times = est.ClubsInCompetition.Where(c => c.CountryId == pais && c.Division >= 1 && c.Division <= 3)
				.OrderBy(c => c.Division).ThenByDescending(c => c.Level).Select(c => c.Slug).ToList();
			if (times.Count < 8) continue;
			int pot2 = 1;
			while (pot2 * 2 <= times.Count && pot2 < 64) pot2 *= 2;
			times = times.Take(pot2).ToList();
			est.Competitions.Add(new Competition
			{
				Id = "copa-" + pais,
				Name = "Copa " + (est.CatalogData?.CountryName(pais) ?? ""),
				Kind = "knockout",
				CountryId = pais,
				Division = 0,
				RelegationSpots = 0,
				ClubsInCompetition = times
			});
		}
	}

	static void CreateContinental(CareerState est, CatalogData banco)
	{
		for (int conf = 0; conf <= 5; conf++)
		{
			var paises = banco.Countries.Where(p => p.Confederation == conf).Select(p => p.Id).ToHashSet();
			var times = new List<Club>();
			foreach (var pais in paises)
			{
				times.AddRange(est.ClubsInCompetition.Where(c => c.CountryId == pais && c.Division == 1)
					.OrderByDescending(c => c.Level).ThenByDescending(c => c.Reputation).Take(2));
			}
			times = times.OrderByDescending(c => c.Level).ThenByDescending(c => c.Reputation).Take(16).ToList();
			if (times.Count < 8) continue;
			int pot2 = 1;
			while (pot2 * 2 <= times.Count) pot2 *= 2;
			est.Competitions.Add(new Competition
			{
				Id = "cont-" + conf,
				Name = ConfederationName(conf),
				Kind = "knockout",
				CountryId = -1,
				Division = 0,
				ClubsInCompetition = times.Take(pot2).Select(t => t.Slug).ToList()
			});
		}
	}

	static string ConfederationName(int c) => c >= 0 && c < Rules.Confederations.Length ? Rules.Confederations[c] : "Continental";

	public static void Schedule(CareerState est)
	{
		var agenda = new FixtureCalendar();
		string inicioEst = est.Year + "-01-07";
		foreach (var comp in est.Competitions.Where(c => c.Id.StartsWith("est-")))
		{
			if (comp.Kind == "groups") ScheduleGroups(comp, agenda, inicioEst, dois: true);
			else ScheduleLeague(comp, agenda, inicioEst, dois: true);
		}
		string depoisEst = est.Year + "-01-07";
		var fimEst = est.Competitions.Where(c => c.Id.StartsWith("est-")).SelectMany(c => c.Matches).Select(p => p.MatchDate).DefaultIfEmpty(est.Year + "-03-01").Max();
		if (!string.IsNullOrEmpty(fimEst))
		{
			var d = DateTime.Parse(fimEst).AddDays(7);
			depoisEst = d.ToString("yyyy-MM-dd");
		}
		foreach (var comp in est.Competitions.Where(c => c.Id.StartsWith("nac-")))
		{
			string inicio = comp.CountryId == 29 ? depoisEst : est.Year + "-01-14";
			bool dois = comp.ClubsInCompetition.Count >= 16 || comp.Kind == "league";
			if (comp.Kind == "groups") ScheduleGroups(comp, agenda, inicio, dois);
			else if (comp.Kind == "league_knockout")
				ScheduleLeague(comp, agenda, inicio, dois: false);
			else ScheduleLeague(comp, agenda, inicio, dois);
		}
		foreach (var comp in est.Competitions.Where(c => c.Id.StartsWith("copa-")))
		{
			string inicio = comp.CountryId == 29 ? est.Year + "-02-15" : est.Year + "-02-01";
			ScheduleKnockout(comp, agenda, inicio, duasPernasApartir: 8);
		}
		foreach (var comp in est.Competitions.Where(c => c.Id.StartsWith("cont-")))
			ScheduleKnockout(comp, agenda, est.Year + "-04-04", duasPernasApartir: 16);
	}

	static void ScheduleLeague(Competition comp, FixtureCalendar agenda, string inicio, bool dois)
	{
		var refs = comp.ClubsInCompetition.ToList();
		var rodadas = RoundPairs(refs.Count);
		int num = 1;
		string cursor = inicio;
		void PlayRound(List<(int a, int b)> jogos, bool inverter)
		{
			foreach (var (a, b) in jogos)
			{
				string casa = inverter ? refs[b] : refs[a];
				string fora = inverter ? refs[a] : refs[b];
				string data = agenda.NextFixture(cursor, DayOfWeek.Saturday, casa, fora);
				comp.Matches.Add(CreateMatch(comp, casa, fora, data, num));
				agenda.Mark(data, casa, fora);
			}
			cursor = NextSaturday(cursor);
			num++;
		}
		foreach (var r in rodadas) PlayRound(r, false);
		if (dois) foreach (var r in rodadas) PlayRound(r, true);
	}

	static void ScheduleGroups(Competition comp, FixtureCalendar agenda, string inicio, bool dois)
	{
		int g = Math.Max(2, comp.Groups);
		var refs = comp.ClubsInCompetition.ToList();
		int por = Math.Max(3, (int)Math.Ceiling(refs.Count / (double)g));
		var grupos = new List<List<string>>();
		for (int i = 0; i < g; i++) grupos.Add(new List<string>());
		for (int i = 0; i < refs.Count; i++) grupos[i % g].Add(refs[i]);
		string cursor = inicio;
		int rodadaBase = 1;
		int maxRod = 0;
		for (int gi = 0; gi < grupos.Count; gi++)
		{
			var sub = new Competition { Id = comp.Id, Name = comp.Name, ClubsInCompetition = grupos[gi], Matches = new List<Match>() };
			var local = new FixtureCalendar();
			ScheduleLeague(sub, local, inicio, dois && grupos[gi].Count <= 8);
			foreach (var p in sub.Matches)
			{
				p.Stage = 0;
				p.Bracket = gi;
				comp.Matches.Add(p);
				agenda.Mark(p.MatchDate, p.HomeSlug, p.AwaySlug);
			}
			maxRod = Math.Max(maxRod, sub.Matches.Select(p => p.Round).DefaultIfEmpty(0).Max());
		}
			_ = rodadaBase;
			_ = cursor;
			_ = por;
		}

	static void ScheduleKnockout(Competition comp, FixtureCalendar agenda, string inicio, int duasPernasApartir)
	{
		ScheduleBracket(comp, agenda, inicio, comp.ClubsInCompetition.ToList(), true, duasPernasApartir);
	}

	internal static void ScheduleBracket(Competition comp, FixtureCalendar agenda, string inicio, List<string> times, bool duasNaFinal, int duasApartir = 0)
	{
		if (times.Count < 2) return;
		var atual = times.ToList();
		int fase = comp.Matches.Select(p => p.Stage).DefaultIfEmpty(0).Max() + 1;
		string cursor = inicio;
		while (atual.Count >= 2)
		{
			int jogos = atual.Count / 2;
			bool duas = duasNaFinal && (atual.Count <= Math.Max(2, duasApartir) || atual.Count == 2);
			for (int i = 0; i < jogos; i++)
			{
				string a = atual[i];
				string b = atual[atual.Count - 1 - i];
				string d1 = agenda.NextFixture(cursor, DayOfWeek.Wednesday, a, b);
				var ida = CreateMatch(comp, a, b, d1, fase);
				ida.Stage = fase;
				ida.Bracket = i;
				ida.Leg = 1;
				comp.Matches.Add(ida);
				agenda.Mark(d1, a, b);
				if (duas)
				{
					string d2 = agenda.NextFixture(DateTime.Parse(d1).AddDays(6).ToString("yyyy-MM-dd"), DayOfWeek.Wednesday, a, b);
					var volta = CreateMatch(comp, b, a, d2, fase);
					volta.Stage = fase;
					volta.Bracket = i;
					volta.Leg = 2;
					comp.Matches.Add(volta);
					agenda.Mark(d2, a, b);
				}
			}
			cursor = DateTime.Parse(cursor).AddDays(14).ToString("yyyy-MM-dd");
			var prox = new List<string>();
			for (int i = 0; i < jogos; i++) prox.Add("");
			atual = prox;
			fase++;
			if (atual.Count < 2) break;
			if (fase > 12) break;
			atual = new List<string>();
			for (int i = 0; i < jogos; i++) atual.Add("?");
			if (atual.Contains("?")) break;
		}
	}

	public static Match CreateMatch(Competition comp, string casa, string fora, string data, int rodada)
	{
		return new Match
		{
			Id = comp.Id + "-" + data + "-" + casa + "-" + fora,
			CompetitionId = comp.Id,
			HomeSlug = casa,
			AwaySlug = fora,
			MatchDate = data,
			Round = rodada
		};
	}

	static string NextSaturday(string iso)
	{
		var d = DateTime.Parse(iso).AddDays(7);
		while (d.DayOfWeek != DayOfWeek.Saturday) d = d.AddDays(1);
		return d.ToString("yyyy-MM-dd");
	}

	public static List<List<(int a, int b)>> RoundPairs(int n)
	{
		int m = n % 2 == 0 ? n : n + 1;
		var idx = Enumerable.Range(0, m).ToArray();
		var rodadas = new List<List<(int, int)>>();
		for (int r = 0; r < m - 1; r++)
		{
			var jogos = new List<(int, int)>();
			for (int i = 0; i < m / 2; i++)
			{
				int a = idx[i];
				int b = idx[m - 1 - i];
				if (a < n && b < n) jogos.Add(r % 2 == 0 ? (a, b) : (b, a));
			}
			rodadas.Add(jogos);
			int guarda = idx[1];
			for (int i = 1; i < m - 1; i++) idx[i] = idx[i + 1];
			idx[m - 1] = guarda;
		}
		return rodadas;
	}

	public static List<Player> PickLineup(Club c, Random rng)
	{
		var slots = Rules.SlotsFor(c.Formation);
		var usados = new HashSet<int>();
		var onze = new List<Player>();
		if (c.Lineup != null && c.Lineup.Count > 0)
		{
			foreach (var id in c.Lineup)
			{
				var j = c.FindPlayer(id);
				if (j != null && j.IsAvailable && usados.Add(j.Id)) onze.Add(j);
			}
		}
		for (int pos = 0; pos < 5; pos++)
		{
			int quer = slots[pos];
			int tem = onze.Count(j => j.Position == pos);
			var cand = c.Squad.Where(j => j.IsAvailable && j.Position == pos && !usados.Contains(j.Id))
				.OrderByDescending(j => j.PreferredStarter).ThenByDescending(j => j.Strength).ThenByDescending(j => j.Fitness).ToList();
			foreach (var j in cand)
			{
				if (tem >= quer) break;
				onze.Add(j);
				usados.Add(j.Id);
				tem++;
			}
		}
		if (onze.Count < 11)
		{
			foreach (var j in c.Squad.Where(j => j.IsAvailable && !usados.Contains(j.Id)).OrderByDescending(j => j.Strength))
			{
				onze.Add(j);
				usados.Add(j.Id);
				if (onze.Count >= 11) break;
			}
		}
		return onze.Take(11).ToList();
	}
}

sealed class FixtureCalendar
{
	readonly Dictionary<string, HashSet<string>> dias = new();

	public string NextFixture(string inicio, DayOfWeek dia, string a, string b)
	{
		var d = DateTime.Parse(inicio);
		int guarda = 0;
		while (d.DayOfWeek != dia && guarda++ < 10) d = d.AddDays(1);
		guarda = 0;
		while (guarda++ < 80)
		{
			string iso = d.ToString("yyyy-MM-dd");
			if (IsFree(iso, a) && IsFree(iso, b)) return iso;
			d = d.AddDays(7);
		}
		return d.ToString("yyyy-MM-dd");
	}

	public void Mark(string iso, string a, string b)
	{
		MarkOne(iso, a);
		MarkOne(iso, b);
	}

	bool IsFree(string iso, string c)
	{
		return !dias.TryGetValue(c, out var set) || !set.Contains(iso);
	}

	void MarkOne(string iso, string c)
	{
		if (!dias.TryGetValue(c, out var set))
		{
			set = new HashSet<string>();
			dias[c] = set;
		}
		set.Add(iso);
	}
}

