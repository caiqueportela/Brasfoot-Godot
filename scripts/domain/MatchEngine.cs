namespace Brasfoot;

public static class MatchEngine
{
	public static void Resolve(CareerState est, Match p, bool detalhe)
	{
		if (p.Played) return;
		if (p.HomeSlug == "?" || p.AwaySlug == "?" || p.HomeSlug == "" || p.AwaySlug == "")
		{
			p.Played = true;
			return;
		}
		var casa = est.FindClub(p.HomeSlug);
		var fora = est.FindClub(p.AwaySlug);
		if (casa == null || fora == null)
		{
			p.Played = true;
			return;
		}
		var rng = new Random(Hash(est.Seed, p.Id));
		var onzeC = SeasonBuilder.PickLineup(casa, rng);
		var onzeF = SeasonBuilder.PickLineup(fora, rng);
		double fc = TeamStrength(casa, onzeC, true);
		double ff = TeamStrength(fora, onzeF, false);
		if (casa.Tactic == "all_out_attack") fc *= 1.08;
		if (fora.Tactic == "all_out_attack") ff *= 1.08;
		if (casa.Tactic == "counter") fc *= 0.96;
		if (fora.Tactic == "counter") ff *= 0.96;
		double xgC = 0.22 + 1.7 * (fc / Math.Max(1, fc + ff));
		double xgF = 0.18 + 1.55 * (ff / Math.Max(1, fc + ff));
		xgC *= 0.85 + casa.Confidence / 250.0;
		xgF *= 0.85 + fora.Confidence / 250.0;
		int gc = Poisson(xgC, rng);
		int gf = Poisson(xgF, rng);
		if (gc > 7) gc = 7;
		if (gf > 7) gf = 7;
		p.HomeGoals = gc;
		p.AwayGoals = gf;
		p.HomePossession = (int)Math.Clamp(50 + (fc - ff) * 0.8, 32, 72);
		p.Events.Clear();
		var minutosGolC = GoalMinutes(gc, rng);
		var minutosGolF = GoalMinutes(gf, rng);
		foreach (var m in minutosGolC) AddGoal(p, m, casa, onzeC, rng, true);
		foreach (var m in minutosGolF) AddGoal(p, m, fora, onzeF, rng, false);
		IssueCards(p, casa, onzeC, rng);
		IssueCards(p, fora, onzeF, rng);
		if (rng.NextDouble() < 0.08 && onzeC.Count > 0) ApplyInjury(p, casa, onzeC[rng.Next(onzeC.Count)], rng);
		if (rng.NextDouble() < 0.08 && onzeF.Count > 0) ApplyInjury(p, fora, onzeF[rng.Next(onzeF.Count)], rng);
		p.Events.Add(new MatchEvent { Minute = 45, Kind = "halftime" });
		p.Events.Add(new MatchEvent { Minute = 90, Kind = "full_time" });
		p.Events = p.Events.OrderBy(e => e.Minute).ThenBy(e => e.Kind == "full_time" ? 2 : e.Kind == "halftime" ? 1 : 0).ToList();
		ApplyResult(casa, fora, p, onzeC, onzeF, rng);
		p.Played = true;
		_ = detalhe;
	}

	static void ApplyResult(Club casa, Club fora, Match p, List<Player> onzeC, List<Player> onzeF, Random rng)
	{
		void Time(Club c, List<Player> onze, int gp, int gc, bool mando)
		{
			foreach (var j in onze)
			{
				j.MatchesPlayed++;
				j.Fitness = Math.Clamp(j.Fitness - rng.Next(8, 18), 35, 100);
				int nota = 6;
				if (gp > gc) nota += 1;
				if (gc == 0 && j.Position <= 2) nota += 1;
				j.RatingSum += Math.Clamp(nota + rng.Next(-1, 2), 3, 10);
				j.RatingCount++;
			}
			foreach (var j in c.Squad.Where(j => j.BanMatches > 0)) j.BanMatches--;
			foreach (var j in c.Squad.Where(j => j.InjuryMatches > 0 && !onze.Contains(j))) j.InjuryMatches--;
			if (gp > gc) { c.Wins++; c.Confidence = Math.Min(100, c.Confidence + (mando ? 4 : 6)); }
			else if (gp == gc) { c.Draws++; c.Confidence = Math.Min(100, c.Confidence + 1); }
			else { c.Losses++; c.Confidence = Math.Max(1, c.Confidence - (gc - gp >= 3 ? 8 : 5)); }
			if (mando)
			{
				double casaCheia = 0.45 + c.Confidence / 250.0 + (gp > gc ? 0.1 : 0);
				int preco = 12 + c.Reputation * 7 + Math.Max(0, 5 - c.Division) * 6;
				long renda = (long)(c.Capacity * Math.Clamp(casaCheia, 0.25, 1) * preco);
				c.Funds += renda;
			}
		}
		Time(casa, onzeC, p.HomeGoals, p.AwayGoals, true);
		Time(fora, onzeF, p.AwayGoals, p.HomeGoals, false);
		casa.HasTrained = false;
		fora.HasTrained = false;
	}

	static void AddGoal(Match p, int minuto, Club time, List<Player> onze, Random rng, bool casa)
	{
		if (onze.Count == 0) return;
		bool contra = rng.NextDouble() < 0.04;
		Player autor;
		double sorteio = rng.NextDouble();
		int extraGol = sorteio < 0.08 ? 2 : sorteio < 0.18 ? 3 : 0;
		if (contra)
		{
			autor = onze[rng.Next(onze.Count)];
			p.Events.Add(new MatchEvent
			{
				Minute = minuto,
				Kind = "goal",
				Extra = 1,
				ClubSlug = time.Slug,
				Player = autor.Id
			});
		}
		else
		{
			autor = PickScorer(onze, rng);
			autor.Goals++;
			autor.Morale = Math.Min(100, autor.Morale + 4);
			p.Events.Add(new MatchEvent
			{
				Minute = minuto,
				Kind = "goal",
				Extra = extraGol,
				ClubSlug = time.Slug,
				Player = autor.Id
			});
		}
	}

	static Player PickScorer(List<Player> onze, Random rng)
	{
		var pesos = onze.Select(j =>
		{
			int w = j.Position switch { 4 => 46, 3 => 28, 1 => 12, 2 => 10, _ => 4 };
			if (j.PrimaryTrait == 9 || j.SecondaryTrait == 9) w += 12;
			if (j.PrimaryTrait == 5 || j.SecondaryTrait == 5) w += 6;
			w += j.Strength / 12;
			return w;
		}).ToList();
		int soma = Math.Max(1, pesos.Sum());
		int t = rng.Next(soma);
		int acc = 0;
		for (int i = 0; i < onze.Count; i++)
		{
			acc += pesos[i];
			if (t < acc) return onze[i];
		}
		return onze[0];
	}

	static void IssueCards(Match p, Club time, List<Player> onze, Random rng)
	{
		int n = rng.Next(0, 4);
		for (int i = 0; i < n && onze.Count > 0; i++)
		{
			var j = onze[rng.Next(onze.Count)];
			int minuto = 1 + rng.Next(90);
			bool vermelho = rng.NextDouble() < 0.06 || j.YellowStreak >= 2 && rng.NextDouble() < 0.35;
			if (vermelho)
			{
				j.RedCards++;
				j.BanMatches = Math.Max(j.BanMatches, 1);
				j.YellowStreak = 0;
				p.Events.Add(new MatchEvent { Minute = minuto, Kind = "red", ClubSlug = time.Slug, Player = j.Id });
			}
			else
			{
				j.YellowCards++;
				j.YellowStreak++;
				if (j.YellowStreak >= 3)
				{
					j.YellowStreak = 0;
					j.BanMatches = Math.Max(j.BanMatches, 1);
				}
				p.Events.Add(new MatchEvent { Minute = minuto, Kind = "yellow", ClubSlug = time.Slug, Player = j.Id });
			}
		}
	}

	static void ApplyInjury(Match p, Club time, Player j, Random rng)
	{
		j.InjuryMatches = 1 + rng.Next(3);
		j.Fitness = Math.Min(j.Fitness, 40);
		int minuto = 1 + rng.Next(85);
		p.Events.Add(new MatchEvent { Minute = minuto, Kind = "injury", Extra = j.InjuryMatches, ClubSlug = time.Slug, Player = j.Id });
	}

	static List<int> GoalMinutes(int gols, Random rng)
	{
		var lista = new List<int>();
		for (int i = 0; i < gols; i++) lista.Add(1 + rng.Next(94));
		lista.Sort();
		return lista;
	}

	static double TeamStrength(Club c, List<Player> onze, bool mando)
	{
		if (onze.Count == 0) return 8;
		double s = onze.Average(j => j.Strength * (0.72 + 0.28 * j.Fitness / 100.0) * (0.9 + j.Morale / 500.0));
		if (mando) s *= 1.07;
		s *= 0.92 + c.Confidence / 400.0;
		return s;
	}

	static int Poisson(double lambda, Random rng)
	{
		double L = Math.Exp(-lambda);
		int k = 0;
		double p = 1;
		do
		{
			k++;
			p *= rng.NextDouble();
		} while (p > L && k < 12);
		return k - 1;
	}

	static int Hash(int semente, string id)
	{
		unchecked
		{
			int h = semente;
			foreach (char c in id) h = h * 31 + c;
			return h == int.MinValue ? 1 : Math.Abs(h);
		}
	}

	public static string NextHumanDate(CareerState est)
	{
		var datas = est.Competitions.SelectMany(c => c.Matches)
			.Where(p => !p.Played && p.HomeSlug != "?" && p.AwaySlug != "?" && (est.IsHumanClub(p.HomeSlug) || est.IsHumanClub(p.AwaySlug)))
			.Select(p => p.MatchDate)
			.Where(d => string.CompareOrdinal(d, est.Today) >= 0)
			.OrderBy(d => d)
			.ToList();
		return datas.FirstOrDefault();
	}

	public static void SimulateUntil(CareerState est, string dataAlvo, bool pararNosHumanos)
	{
		var datas = est.Competitions.SelectMany(c => c.Matches)
			.Where(p => !p.Played && string.CompareOrdinal(p.MatchDate, dataAlvo) <= 0)
			.Select(p => p.MatchDate).Distinct().OrderBy(d => d).ToList();
		foreach (var data in datas)
		{
			PayIfDue(est, data);
			foreach (var p in est.Competitions.SelectMany(c => c.Matches).Where(p => !p.Played && p.MatchDate == data).ToList())
			{
				if (pararNosHumanos && data == dataAlvo && (est.IsHumanClub(p.HomeSlug) || est.IsHumanClub(p.AwaySlug))) continue;
				Resolve(est, p, false);
			}
			est.Today = data;
			ResolveKnockout(est);
		}
	}

	public static void PayIfDue(CareerState est, string data)
	{
		if (data.Length < 7) return;
		string mes = data.Substring(0, 7);
		if (est.LastPaidMonth == mes) return;
		PayMonth(est, data);
		est.LastPaidMonth = mes;
	}

	public static void PayMonth(CareerState est, string data)
	{
		foreach (var c in est.ClubsInCompetition)
		{
			long folha = c.Squad.Sum(j => (long)j.Wage);
			long patrocinio = c.Reputation * 90_000L + Math.Max(0, c.Level) * 25_000L;
			if (c.Division == 1) patrocinio += 180_000;
			c.Funds += patrocinio - folha;
			foreach (var j in c.Squad)
			{
				if (j.Fitness < 100) j.Fitness = Math.Min(100, j.Fitness + 6);
			}
		}
		est.LastPaidMonth = data.Length >= 7 ? data.Substring(0, 7) : est.LastPaidMonth;
	}

	static void OpenKnockoutWhenRoundEnds(CareerState est, Competition comp)
	{
		if (comp.Kind != "groups" && comp.Kind != "league_knockout") return;
		if (!comp.Matches.Any(p => p.Stage == 0) || comp.Matches.Any(p => p.Stage == 0 && !p.Played)) return;
		if (comp.Matches.Any(p => p.Stage > 0)) return;
		var classificados = new List<string>();
		if (comp.Kind == "groups")
		{
			int g = Math.Max(1, comp.Groups);
			for (int gi = 0; gi < g; gi++)
			{
				var sub = new Competition
				{
					Kind = "league",
					ClubsInCompetition = comp.Matches.Where(p => p.Stage == 0 && p.Bracket == gi).SelectMany(p => new[] { p.HomeSlug, p.AwaySlug }).Distinct().ToList(),
					Matches = comp.Matches.Where(p => p.Stage == 0 && p.Bracket == gi).ToList()
				};
				classificados.AddRange(Standings(est, sub).Take(2).Select(t => t.Slug));
			}
		}
		else
		{
			int n = Math.Min(8, Standings(est, comp).Count);
			n -= n % 2;
			classificados.AddRange(Standings(est, comp).Take(n).Select(t => t.Slug));
		}
		int pot = 1;
		while (pot * 2 <= classificados.Count) pot *= 2;
		classificados = classificados.Take(pot).ToList();
		if (classificados.Count < 2) return;
		string ultima = comp.Matches.Max(p => p.MatchDate);
		var agenda = new FixtureCalendar();
		foreach (var p in est.Competitions.SelectMany(c => c.Matches))
			agenda.Mark(p.MatchDate, p.HomeSlug, p.AwaySlug);
		SeasonBuilder.ScheduleBracket(comp, agenda, DateTime.Parse(ultima).AddDays(5).ToString("yyyy-MM-dd"), classificados, true, 8);
	}

	public static void ResolveKnockout(CareerState est)
	{
		foreach (var comp in est.Competitions.Where(c => c.Kind == "knockout" || c.Kind == "groups" || c.Kind == "league_knockout"))
		{
			OpenKnockoutWhenRoundEnds(est, comp);
			var fases = comp.Matches.Where(p => p.Stage > 0).Select(p => p.Stage).Distinct().OrderBy(f => f).ToList();
			foreach (var fase in fases)
			{
				var chaves = comp.Matches.Where(p => p.Stage == fase).GroupBy(p => p.Bracket).ToList();
				if (chaves.Count == 0 || chaves.Any(g => g.Any(p => !p.Played))) continue;
				int proxFase = fase + 1;
				if (comp.Matches.Any(p => p.Stage == proxFase && p.HomeSlug != "?" && p.AwaySlug != "?" && p.HomeSlug != "")) continue;
				var vencedores = new List<string>();
				foreach (var g in chaves.OrderBy(g => g.Key))
				{
					string v = BracketWinner(g.ToList(), est);
					if (!string.IsNullOrEmpty(v)) vencedores.Add(v);
				}
				if (vencedores.Count == 1)
				{
					comp.Champion = vencedores[0];
					comp.Finished = true;
					var camp = est.FindClub(comp.Champion);
					if (camp != null)
					{
						long premio = comp.Id.StartsWith("cont-") ? 4_000_000 : comp.Id.StartsWith("copa-") ? 1_500_000 : 800_000;
						camp.Funds += premio;
						est.History.Add("history.champion|" + est.Year + "|" + camp.Name + "|" + comp.Name);
					}
					continue;
				}
				var pendentes = comp.Matches.Where(p => p.Stage == proxFase && (p.HomeSlug == "?" || p.HomeSlug == "")).OrderBy(p => p.Bracket).ThenBy(p => p.Leg).ToList();
				if (pendentes.Count == 0 && vencedores.Count >= 2)
				{
					string data = chaves.SelectMany(g => g).Max(p => p.MatchDate);
					var agenda = new FixtureCalendar();
					int jogos = vencedores.Count / 2;
					for (int i = 0; i < jogos; i++)
					{
						string a = vencedores[i];
						string b = vencedores[vencedores.Count - 1 - i];
						var d = DateTime.Parse(data).AddDays(12).ToString("yyyy-MM-dd");
						var ida = SeasonBuilder.CreateMatch(comp, a, b, d, proxFase);
						ida.Stage = proxFase;
						ida.Bracket = i;
						ida.Leg = 1;
						comp.Matches.Add(ida);
						if (vencedores.Count <= 4)
						{
							var volta = SeasonBuilder.CreateMatch(comp, b, a, DateTime.Parse(d).AddDays(7).ToString("yyyy-MM-dd"), proxFase);
							volta.Stage = proxFase;
							volta.Bracket = i;
							volta.Leg = 2;
							comp.Matches.Add(volta);
						}
					}
					_ = agenda;
				}
				else
				{
					int i = 0;
					foreach (var p in pendentes.Where(p => p.Leg == 1))
					{
						if (i + 1 >= vencedores.Count) break;
						p.HomeSlug = vencedores[i];
						p.AwaySlug = vencedores[i + 1];
						var volta = pendentes.FirstOrDefault(x => x.Bracket == p.Bracket && x.Leg == 2);
						if (volta != null)
						{
							volta.HomeSlug = vencedores[i + 1];
							volta.AwaySlug = vencedores[i];
						}
						i += 2;
					}
				}
			}
		}
	}

	static string BracketWinner(List<Match> jogos, CareerState est)
	{
		if (jogos.Count == 0 || jogos.Any(j => !j.Played)) return "";
		var ida = jogos.OrderBy(j => j.Leg).First();
		int gA = 0, gB = 0;
		string a = ida.HomeSlug;
		string b = ida.AwaySlug;
		foreach (var j in jogos)
		{
			if (j.HomeSlug == a) { gA += j.HomeGoals; gB += j.AwayGoals; }
			else { gA += j.AwayGoals; gB += j.HomeGoals; }
		}
		if (gA > gB) return a;
		if (gB > gA) return b;
		var rng = new Random(Hash(est.Seed, ida.Id + "pen"));
		return rng.Next(2) == 0 ? a : b;
	}

	public static List<TableRow> Standings(CareerState est, Competition comp)
	{
		IEnumerable<Match> jogos = comp.Matches.Where(p => p.Played && p.Stage == 0);
		var times = jogos.SelectMany(p => new[] { p.HomeSlug, p.AwaySlug }).Concat(comp.Kind == "groups" ? Array.Empty<string>() : comp.ClubsInCompetition).Distinct().Where(r => r != "?" && r != "").ToList();
		if (comp.Kind != "groups") times = comp.ClubsInCompetition.ToList();
		var map = times.ToDictionary(r => r, r => new TableRow { Slug = r, Name = est.FindClub(r)?.Name ?? r });
		foreach (var p in jogos)
		{
			if (!map.ContainsKey(p.HomeSlug) || !map.ContainsKey(p.AwaySlug)) continue;
			var c = map[p.HomeSlug];
			var f = map[p.AwaySlug];
			c.Played++; f.Played++;
			c.GoalsFor += p.HomeGoals; c.GoalsAgainst += p.AwayGoals;
			f.GoalsFor += p.AwayGoals; f.GoalsAgainst += p.HomeGoals;
			if (p.HomeGoals > p.AwayGoals) { c.Wins++; c.Points += 3; f.Losses++; }
			else if (p.HomeGoals < p.AwayGoals) { f.Wins++; f.Points += 3; c.Losses++; }
			else { c.Draws++; f.Draws++; c.Points++; f.Points++; }
		}
		return map.Values.OrderByDescending(l => l.Points).ThenByDescending(l => l.Wins).ThenByDescending(l => l.GoalsFor - l.GoalsAgainst).ThenByDescending(l => l.GoalsFor).ThenBy(l => l.Name).ToList();
	}

	public static List<(string nome, string time, int gols)> TopScorers(CareerState est, Competition comp, int n)
	{
		var refs = comp.ClubsInCompetition.ToHashSet();
		return est.ClubsInCompetition.Where(c => refs.Contains(c.Slug))
			.SelectMany(c => c.Squad.Where(j => j.Goals > 0).Select(j => (nome: j.Name, time: c.Name, gols: j.Goals)))
			.OrderByDescending(x => x.gols).ThenBy(x => x.nome).Take(n).ToList();
	}

	public static bool SeasonOver(CareerState est)
	{
		return !est.Competitions.SelectMany(c => c.Matches).Any(p => !p.Played && p.HomeSlug != "?" && p.HomeSlug != "" && p.AwaySlug != "?");
	}

	public static List<string> FinishSeason(CareerState est)
	{
		var msgs = new List<string>();
		foreach (var comp in est.Competitions.Where(c => c.Kind == "league" || c.Kind == "league_knockout" || (c.Kind == "groups" && c.Id.StartsWith("nac-"))))
		{
			if (comp.RelegationSpots <= 0 || comp.Division <= 0) continue;
			var tab = Standings(est, comp);
			if (tab.Count == 0) continue;
			var camp = est.FindClub(tab[0].Slug);
			if (camp != null && string.IsNullOrEmpty(comp.Champion))
			{
				comp.Champion = camp.Slug;
				long premio = comp.Division switch { 1 => 5_000_000, 2 => 2_000_000, 3 => 800_000, _ => 300_000 };
				premio += Math.Max(0, 8 - tab.FindIndex(t => t.Slug == camp.Slug)) * 100_000L;
				camp.Funds += premio;
				msgs.Add("season_end.champion|" + camp.Name + "|" + comp.Name);
				est.History.Add("history.champion|" + est.Year + "|" + camp.Name + "|" + comp.Name);
			}
		}
		var porPais = est.Competitions.Where(c => c.Id.StartsWith("nac-") && (c.Kind == "league" || c.Kind == "league_knockout")).GroupBy(c => c.CountryId);
		foreach (var g in porPais)
		{
			var ordenadas = g.OrderBy(c => c.Division).ToList();
			for (int i = 0; i < ordenadas.Count; i++)
			{
				var sup = ordenadas[i];
				var tab = Standings(est, sup);
				int reba = Math.Min(sup.RelegationSpots, Math.Max(0, tab.Count / 3));
				if (i + 1 >= ordenadas.Count)
				{
					var fora = est.ClubsInCompetition.Where(c => c.CountryId == sup.CountryId && c.Division == 0).OrderByDescending(c => c.Level).Take(reba).Select(c => c.Slug).ToList();
					var caem = tab.TakeLast(Math.Min(reba, fora.Count)).Select(t => t.Slug).ToList();
					for (int k = 0; k < caem.Count && k < fora.Count; k++)
					{
						var desce = est.FindClub(caem[k]);
						var sobe = est.FindClub(fora[k]);
						if (desce == null || sobe == null) continue;
						desce.Division = 0;
						sobe.Division = sup.Division;
						sup.ClubsInCompetition.Remove(desce.Slug);
						sup.ClubsInCompetition.Add(sobe.Slug);
						msgs.Add("season_end.promoted|" + sobe.Name + "|" + sup.Name + "|" + desce.Name);
					}
					continue;
				}
				var inf = ordenadas[i + 1];
				var tabInf = Standings(est, inf);
				var descem = tab.TakeLast(reba).Select(t => t.Slug).ToList();
				var sobem = tabInf.Take(reba).Select(t => t.Slug).ToList();
				for (int k = 0; k < descem.Count && k < sobem.Count; k++)
				{
					var d = est.FindClub(descem[k]);
					var s = est.FindClub(sobem[k]);
					if (d == null || s == null) continue;
					sup.ClubsInCompetition.Remove(d.Slug);
					inf.ClubsInCompetition.Remove(s.Slug);
					sup.ClubsInCompetition.Add(s.Slug);
					inf.ClubsInCompetition.Add(d.Slug);
					d.Division = inf.Division;
					s.Division = sup.Division;
					msgs.Add("season_end.promotion|" + s.Name + "|" + d.Name);
				}
			}
		}
		var rng = new Random(est.Seed + est.Year);
		foreach (var c in est.ClubsInCompetition)
		{
			c.Wins = c.Draws = c.Losses = 0;
			c.HasTrained = false;
			var saem = new List<Player>();
			foreach (var j in c.Squad)
			{
				j.Age++;
				j.Goals = 0;
				j.MatchesPlayed = 0;
				j.YellowCards = j.YellowStreak = j.RedCards = 0;
				j.RatingSum = 0;
				j.RatingCount = 0;
				j.BanMatches = 0;
				j.InjuryMatches = 0;
				j.ContractYears = Math.Max(0, j.ContractYears - 1);
				j.Fitness = Math.Min(100, j.Fitness + 10);
				if (j.Age >= 41 || (j.Age >= 37 && rng.NextDouble() < 0.45) || (j.ContractYears == 0 && !c.IsHuman && rng.NextDouble() < 0.5))
					saem.Add(j);
				else
				{
					int delta = j.Age <= 23 ? rng.Next(0, 3) : j.Age >= 33 ? -rng.Next(0, 3) : rng.Next(-1, 2);
					j.Strength = Math.Clamp(j.Strength + delta, 4, 97);
					j.Wage = Rules.WageFor(j.Strength, c.Division, j.Position, j.Age, j.IsStar);
					j.Price = Rules.MarketValue(j.Strength, c.Level, j.Age, j.Position, j.IsStar, j.PreferredStarter);
					if (j.ContractYears == 0) j.ContractYears = 2;
				}
			}
			foreach (var j in saem) c.Squad.Remove(j);
			while (c.Squad.Count < 18)
			{
				int pos = c.Squad.Count % 5;
				var j = new Player
				{
					Id = est.NextId++,
					Name = YouthName(est, c, rng),
					CountryId = c.CountryId,
					Age = 17,
					Position = pos,
					Side = rng.Next(2),
					PrimaryTrait = rng.Next(14),
					SecondaryTrait = rng.Next(14),
					Strength = Math.Clamp(c.Level + rng.Next(4, 14), 8, 40),
					Fitness = 90,
					Morale = 70,
					ContractYears = 3,
					PreferredStarter = false
				};
				j.Wage = Rules.WageFor(j.Strength, c.Division, j.Position, j.Age, false);
				j.Price = Rules.MarketValue(j.Strength, c.Level, j.Age, j.Position, false, false);
				c.Squad.Add(j);
			}
		}
		foreach (var comp in est.Competitions.ToList())
			comp.Matches.Clear();
		est.Year++;
		est.Today = est.Year + "-01-01";
		est.LastPaidMonth = "";
		est.Seed += 17;
		foreach (var comp in est.Competitions)
		{
			comp.Finished = false;
			comp.Champion = "";
		}
		SeasonBuilder.Schedule(est);
		PayMonth(est, est.Today);
		msgs.Add("season_end.new_season|" + est.Year);
		return msgs;
	}

	static string YouthName(CareerState est, Club c, Random rng)
	{
		var baseNomes = est.ClubsInCompetition.Where(x => x.CountryId == c.CountryId).SelectMany(x => x.Squad).Select(j => j.Name).Where(n => n.Contains(' ')).ToList();
		if (baseNomes.Count == 0) return "Jovem " + c.Name;
		var a = baseNomes[rng.Next(baseNomes.Count)].Split(' ')[0];
		var b = baseNomes[rng.Next(baseNomes.Count)].Split(' ').Last();
		return a + " " + b;
	}

	public static string TryPurchase(CareerState est, Club comprador, Player jogador, Club vendedor, int oferta)
	{
		if (comprador.Funds < oferta) return "market.no_funds";
		if (oferta < jogador.Price * 0.82) return "market.rejected_fee|" + vendedor.Name + "|" + Rules.Money((long)(jogador.Price * 0.95));
		double aceite = oferta >= jogador.Price ? 0.92 : oferta >= jogador.Price * 0.9 ? 0.7 : 0.45;
		if (jogador.IsStar) aceite -= 0.15;
		var rng = new Random(est.Seed + jogador.Id + oferta);
		if (rng.NextDouble() > aceite) return "market.rejected|" + vendedor.Name;
		vendedor.Squad.Remove(jogador);
		comprador.Squad.Add(jogador);
		comprador.Funds -= oferta;
		vendedor.Funds += oferta;
		jogador.ListedForSale = false;
		jogador.PreferredStarter = false;
		jogador.ContractYears = 3;
		jogador.Wage = Rules.WageFor(jogador.Strength, comprador.Division, jogador.Position, jogador.Age, jogador.IsStar);
		return "market.ok|" + jogador.Name + "|" + comprador.Name;
	}
}

public sealed class TableRow
{
	public string Slug = "";
	public string Name = "";
	public int Played, Wins, Draws, Losses, GoalsFor, GoalsAgainst, Points;
	public int GoalDifference => GoalsFor - GoalsAgainst;
}
