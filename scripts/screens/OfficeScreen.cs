using Godot;

namespace Brasfoot;

public partial class OfficeScreen : ScreenBase
{
	PackedScene linhaElenco;
	PackedScene faixaTabela;
	PackedScene linhaTexto;
	PackedScene botaoLinha;
	bool interno;

	public override void _Ready()
	{
		if (Session.Instance.Current == null)
		{
			Session.Instance.GoTo(Paths.Menu);
			return;
		}
		linhaElenco = GD.Load<PackedScene>("res://scenes/components/SquadRow.tscn");
		faixaTabela = GD.Load<PackedScene>("res://scenes/components/StandingsRow.tscn");
		linhaTexto = GD.Load<PackedScene>("res://scenes/components/TextRow.tscn");
		botaoLinha = GD.Load<PackedScene>("res://scenes/components/RowButton.tscn");
		Wire();
		base._Ready();
		if (OS.GetEnvironment("BRASFOOT_FOTO") == "1" && !Session.Instance.OfficeCaptured)
		{
			Session.Instance.OfficeCaptured = true;
			GetTree().CreateTimer(0.8).Timeout += () =>
			{
				Screenshot.Save(this, "verificacao_escritorio.png");
				Session.Instance.Continue();
			};
		}
	}

	protected override void ApplyTexts()
	{
		var abas = GetNode<TabContainer>("%Tabs");
		string[] chaves = { "tab.squad", "tab.lineup", "tab.table", "tab.calendar", "tab.finances", "tab.market", "tab.training", "tab.options" };
		for (int i = 0; i < chaves.Length && i < abas.GetTabCount(); i++)
			abas.SetTabTitle(i, Translator.T(chaves[i]));
		GetNode<Label>("%HeadName").Text = Translator.T("column.name");
		GetNode<Label>("%HeadPosition").Text = Translator.T("column.position");
		GetNode<Label>("%HeadAge").Text = Translator.T("column.age");
		GetNode<Label>("%HeadTrait").Text = Translator.T("column.trait");
		GetNode<Label>("%HeadStrength").Text = Translator.T("column.strength");
		GetNode<Label>("%HeadFitness").Text = Translator.T("column.fitness");
		GetNode<Label>("%HeadGoals").Text = Translator.T("column.goals");
		GetNode<Label>("%HeadWage").Text = Translator.T("column.wage");
		GetNode<Label>("%HeadRank").Text = Translator.T("column.number");
		GetNode<Label>("%HeadClub").Text = Translator.T("column.club");
		GetNode<Label>("%HeadPlayed").Text = Translator.T("column.played");
		GetNode<Label>("%HeadWins").Text = Translator.T("column.wins");
		GetNode<Label>("%HeadDraws").Text = Translator.T("column.draws");
		GetNode<Label>("%HeadLosses").Text = Translator.T("column.losses");
		GetNode<Label>("%HeadGoalsFor").Text = Translator.T("column.goals_for");
		GetNode<Label>("%HeadGoalsAgainst").Text = Translator.T("column.goals_against");
		GetNode<Label>("%HeadGoalDiff").Text = Translator.T("column.goal_diff");
		GetNode<Label>("%HeadPoints").Text = Translator.T("column.points");
		GetNode<Label>("%LabelFormation").Text = Translator.T("office.formation");
		GetNode<Label>("%LabelTactic").Text = Translator.T("office.tactic");
		GetNode<Button>("%AutoLineup").Text = Translator.T("office.auto");
		GetNode<Label>("%LineupHint").Text = Translator.T("office.lineup_hint");
		GetNode<Label>("%ScorersTitle").Text = Translator.T("office.scorers");
		GetNode<LineEdit>("%MarketSearch").PlaceholderText = Translator.T("office.search");
		GetNode<Label>("%TrainingNotice").Text = Translator.T("office.can_train");
		GetNode<Button>("%TrainingPhysical").Text = Translator.T("office.physical");
		GetNode<Button>("%TrainingTactical").Text = Translator.T("office.tactical");
		GetNode<Button>("%TrainingTechnical").Text = Translator.T("office.technical");
		GetNode<Label>("%LabelSpeed").Text = Translator.T("office.speed");
		GetNode<CheckBox>("%Sound").Text = Translator.T("office.sound");
		GetNode<Label>("%LabelLanguage").Text = Translator.T("office.language");
		GetNode<CheckBox>("%Cloud").Text = Translator.T("office.cloud");
		GetNode<Label>("%CloudHint").Text = Translator.T("office.cloud_hint");
		GetNode<Label>("%HistoryTitle").Text = Translator.T("office.history");
		GetNode<Button>("%Continue").Text = Translator.T("office.continue");
		GetNode<Button>("%Save").Text = Translator.T("office.save");
		GetNode<Button>("%Quit").Text = Translator.T("office.quit");
		GetNode<Label>("%NextTitle").Text = Translator.T("office.next");
		FillFixedOptions();
		if (linhaElenco != null) Refresh();
	}

	void Wire()
	{
		GetNode<Button>("%Continue").Pressed += () =>
		{
			if (Session.Instance.Continue() && IsInsideTree()) Refresh();
		};
		GetNode<Button>("%Save").Pressed += () => Session.Instance.GoTo(Paths.Save);
		GetNode<Button>("%Quit").Pressed += () =>
		{
			var c = GetNode<ConfirmationDialog>("%ConfirmDialog");
			c.DialogText = Translator.T("office.quit_confirm");
			c.OkButtonText = Translator.T("office.quit_yes");
			c.CancelButtonText = Translator.T("office.cancel");
			c.PopupCentered();
		};
		GetNode<ConfirmationDialog>("%ConfirmDialog").Confirmed += () => Session.Instance.Abandon();
		GetNode<TabContainer>("%Tabs").TabChanged += i =>
		{
			if (interno) return;
			Session.Instance.TabIndex = (int)i;
			RefreshTab();
		};
		GetNode<OptionButton>("%Swap").ItemSelected += i =>
		{
			if (interno) return;
			var est = Session.Instance.Current;
			string refe = est.HumanClubIds[(int)i];
			est.HumanClubIds.Remove(refe);
			est.HumanClubIds.Insert(0, refe);
			Refresh();
		};
		GetNode<OptionButton>("%Formation").ItemSelected += i =>
		{
			if (interno) return;
			var h = IsHuman();
			if (h == null) return;
			h.Formation = Rules.Formations[(int)i];
			RefreshSummary();
		};
		GetNode<OptionButton>("%Tactic").ItemSelected += i =>
		{
			if (interno) return;
			var h = IsHuman();
			if (h == null) return;
			h.Tactic = Rules.Tactics[(int)i];
		};
		GetNode<Button>("%AutoLineup").Pressed += () =>
		{
			var h = IsHuman();
			if (h == null) return;
			h.Lineup = new List<int>();
			RefreshTab();
		};
		GetNode<OptionButton>("%StandingsBody").ItemSelected += i =>
		{
			if (interno) return;
			Session.Instance.CompetitionIndex = (int)i;
			FillStandings();
		};
		GetNode<LineEdit>("%MarketSearch").TextChanged += _ => { if (!interno) BuildMarket(); };
		GetNode<Button>("%TrainingPhysical").Pressed += () => Train(c =>
		{
			foreach (var j in c.Squad) j.Fitness = Math.Min(100, j.Fitness + 14);
			c.Funds -= 40_000;
		});
		GetNode<Button>("%TrainingTactical").Pressed += () => Train(c =>
		{
			c.Confidence = Math.Min(100, c.Confidence + 5);
			c.Funds -= 20_000;
		});
		GetNode<Button>("%TrainingTechnical").Pressed += () => Train(c =>
		{
			var rng = new Random(Session.Instance.Current.Seed + c.Squad.Count + Session.Instance.Current.Year);
			foreach (var j in c.Squad)
				if (j.Age <= 30 && rng.NextDouble() < 0.35)
					j.Strength = Math.Min(97, j.Strength + 1);
			c.Funds -= 80_000;
		});
		GetNode<OptionButton>("%Speed").ItemSelected += i =>
		{
			if (interno) return;
			Session.Instance.Current.Speed = GetNode<OptionButton>("%Speed").GetItemId((int)i);
		};
		GetNode<CheckBox>("%Sound").Toggled += v => { if (!interno) Session.Instance.Current.SoundEnabled = v; };
		GetNode<OptionButton>("%Language").ItemSelected += i =>
		{
			if (interno) return;
			string codigo = GetNode<OptionButton>("%Language").GetItemId((int)i) switch { 1 => "en", 2 => "es", _ => "pt" };
			if (codigo != Preferences.Instance.Settings.Language)
				Preferences.Instance.SetLanguage(codigo);
		};
		GetNode<CheckBox>("%Cloud").Toggled += v => { if (!interno) Preferences.Instance.SetCloud(v); };
		GetNode<Button>("%SendOffer").Pressed += SendOffer;
		GetNode<Button>("%CloseOffer").Pressed += () => GetNode<Control>("%Offer").Visible = false;
	}

	void FillFixedOptions()
	{
		var h = IsHuman();
		interno = true;
		var formas = GetNode<OptionButton>("%Formation");
		formas.Clear();
		for (int i = 0; i < Rules.Formations.Length; i++) formas.AddItem(Rules.Formations[i], i);
		if (h != null)
		{
			int idx = System.Array.IndexOf(Rules.Formations, h.Formation);
			formas.Selected = Math.Max(0, idx);
		}
		var taticas = GetNode<OptionButton>("%Tactic");
		taticas.Clear();
		for (int i = 0; i < Rules.Tactics.Length; i++) taticas.AddItem(Translator.T("tactic." + i), i);
		if (h != null)
		{
			int idx = System.Array.IndexOf(Rules.Tactics, h.Tactic);
			taticas.Selected = Math.Max(0, idx);
		}
		var vel = GetNode<OptionButton>("%Speed");
		vel.Clear();
		vel.AddItem(Translator.T("speed.1"), 1);
		vel.AddItem(Translator.T("speed.2"), 2);
		vel.AddItem(Translator.T("speed.3"), 3);
		vel.Selected = vel.GetItemIndex(Session.Instance.Current.Speed);
		GetNode<CheckBox>("%Sound").ButtonPressed = Session.Instance.Current.SoundEnabled;
		var idioma = GetNode<OptionButton>("%Language");
		if (idioma.ItemCount == 0)
		{
			idioma.AddItem("Português", 0);
			idioma.AddItem("English", 1);
			idioma.AddItem("Español", 2);
		}
		idioma.Selected = Preferences.Instance.Settings.Language switch { "en" => 1, "es" => 2, _ => 0 };
		GetNode<CheckBox>("%Cloud").ButtonPressed = Preferences.Instance.Settings.CloudSync;
		interno = false;
	}

	Club IsHuman() => Session.Instance.Current?.HumanClubs().FirstOrDefault();

	void Refresh()
	{
		var est = Session.Instance.Current;
		var h = IsHuman();
		GetNode<Label>("%Date").Text = Rules.ShortDate(est.Today) + "    " + est.Year;
		GetNode<Label>("%Funds").Text = h == null ? "" : Rules.Money(h.Funds);
		var abas = GetNode<TabContainer>("%Tabs");
		int desejada = Mathf.Clamp(Session.Instance.TabIndex, 0, abas.GetTabCount() - 1);
		if (abas.CurrentTab != desejada)
		{
			interno = true;
			abas.CurrentTab = desejada;
			interno = false;
		}
		RefreshClub();
		RefreshTab();
	}

	void RefreshClub()
	{
		var est = Session.Instance.Current;
		var h = IsHuman();
		var troca = GetNode<OptionButton>("%Swap");
		interno = true;
		troca.Clear();
		troca.Visible = est.HumanClubIds.Count > 1;
		for (int i = 0; i < est.HumanClubIds.Count; i++)
			troca.AddItem(est.FindClub(est.HumanClubIds[i])?.Name ?? est.HumanClubIds[i]);
		if (troca.ItemCount > 0) troca.Selected = 0;
		interno = false;
		if (h == null)
		{
			GetNode<Label>("%ClubTitleLabel").Text = Translator.T("office.no_club");
			return;
		}
		GetNode<ColorRect>("%Row").Color = new Color(h.BackgroundColor);
		GetNode<TextureRect>("%Crest").Texture = Crests.Get(h.Slug);
		GetNode<Label>("%ClubTitleLabel").Text = h.Name;
		string origem = est.CatalogData.CountryName(h.CountryId);
		if (h.CountryId == 29) origem += " · " + est.CatalogData.StateName(h.StateId);
		GetNode<Label>("%Source").Text = origem;
		GetNode<Label>("%Stadium").Text = h.Stadium + " · " + h.Capacity.ToString("N0");
		GetNode<Label>("%Coach").Text = Translator.T("office.coach") + " " + h.Coach;
		GetNode<ProgressBar>("%Confidence").Value = h.Confidence;
		GetNode<Label>("%LabelConfidence").Text = Translator.T("office.confidence") + " " + h.Confidence;
		int cond = h.Squad.Count == 0 ? 0 : (int)h.Squad.Average(j => j.Fitness);
		GetNode<ProgressBar>("%Fitness").Value = cond;
		GetNode<Label>("%LabelFitness").Text = Translator.T("office.fitness") + " " + cond;
		var prox = NextFixture(h.Slug);
		var campo = GetNode<Pitch>("%Pitch");
		campo.Visible = prox != null;
		if (prox == null)
		{
			GetNode<Label>("%NextOpponent").Text = Translator.T("office.no_match");
			GetNode<Label>("%NextDate").Text = "";
		}
		else
		{
			var adv = prox.HomeSlug == h.Slug ? est.FindClub(prox.AwaySlug) : est.FindClub(prox.HomeSlug);
			string mando = prox.HomeSlug == h.Slug ? Translator.T("office.home") : Translator.T("office.away");
			var comp = est.Competitions.FirstOrDefault(c => c.Id == prox.CompetitionId);
			GetNode<Label>("%NextOpponent").Text = (adv?.Name ?? "") + " · " + mando;
			GetNode<Label>("%NextDate").Text = Rules.ShortDate(prox.MatchDate) + " · " + (comp?.Name ?? "");
			string casa = est.FindClub(prox.HomeSlug)?.Name ?? prox.HomeSlug;
			string fora = est.FindClub(prox.AwaySlug)?.Name ?? prox.AwaySlug;
			campo.Show(casa, fora, "x", Rules.ShortDate(prox.MatchDate));
		}
	}

	Match NextFixture(string refe)
	{
		var est = Session.Instance.Current;
		return est.Competitions.SelectMany(c => c.Matches)
			.Where(p => !p.Played && p.HomeSlug != "?" && p.HomeSlug != "" && (p.HomeSlug == refe || p.AwaySlug == refe) && string.CompareOrdinal(p.MatchDate, est.Today) >= 0)
			.OrderBy(p => p.MatchDate).ThenBy(p => p.Id)
			.FirstOrDefault();
	}

	void RefreshTab()
	{
		switch (GetNode<TabContainer>("%Tabs").CurrentTab)
		{
			case 1: BuildLineup(); break;
			case 2: BuildStandings(); break;
			case 3: BuildCalendar(); break;
			case 4: BuildFinances(); break;
			case 5: BuildMarket(); break;
			case 6: BuildTraining(); break;
			case 7: BuildOptions(); break;
			default: BuildSquad(); break;
		}
	}

	void BuildSquad()
	{
		var lista = GetNode<VBoxContainer>("%SquadList");
		Nodes.Clear(lista);
		var h = IsHuman();
		if (h == null) return;
		foreach (var j in h.Squad.OrderBy(j => j.Position).ThenByDescending(j => j.Strength))
		{
			var row = linhaElenco.Instantiate<SquadRow>();
			lista.AddChild(row);
			row.Fill(j);
		}
	}

	void BuildLineup()
	{
		var lista = GetNode<VBoxContainer>("%LineupList");
		Nodes.Clear(lista);
		var h = IsHuman();
		RefreshSummary();
		if (h == null) return;
		foreach (var j in h.Squad.OrderBy(j => j.Position).ThenByDescending(j => j.Strength))
		{
			bool dentro = h.Lineup != null && h.Lineup.Contains(j.Id);
			var b = botaoLinha.Instantiate<Button>();
			string marca = dentro ? Translator.T("office.starter") + " " : j.IsAvailable ? "" : Translator.T("office.unavailable") + " ";
			string extra = j.BanMatches > 0 ? "   " + Translator.T("office.suspended") : "";
			if (j.InjuryMatches > 0) extra += "   " + string.Format(Translator.T("office.injury_short"), j.InjuryMatches);
			b.Text = marca + j.Name + "   " + Rules.Position(j.Position) + " " + (j.Side == 1 ? Translator.T("side.1") : Translator.T("side.0")) + "   " + j.TraitSummary + "   " + j.Strength + extra;
			if (!j.IsAvailable) b.Modulate = new Color(0.7f, 0.7f, 0.7f);
			int id = j.Id;
			b.Pressed += () =>
			{
				h.Lineup ??= new List<int>();
				if (h.Lineup.Contains(id)) h.Lineup.Remove(id);
				else if (h.Lineup.Count < 11 && h.FindPlayer(id)?.IsAvailable == true) h.Lineup.Add(id);
				BuildLineup();
			};
			lista.AddChild(b);
		}
	}

	void RefreshSummary()
	{
		var h = IsHuman();
		int n = h?.Lineup?.Count ?? 0;
		GetNode<Label>("%LineupSummary").Text = n == 0
			? Translator.T("office.lineup_auto")
			: string.Format(Translator.T("office.lineup_count"), n);
	}

	void BuildStandings()
	{
		var h = IsHuman();
		var comps = h == null ? new List<Competition>() : Session.Instance.Current.Competitions.Where(c => c.ClubsInCompetition.Contains(h.Slug)).ToList();
		var seletor = GetNode<OptionButton>("%StandingsBody");
		interno = true;
		seletor.Clear();
		foreach (var c in comps) seletor.AddItem(c.Name);
		if (comps.Count > 0)
		{
			int idx = Mathf.Clamp(Session.Instance.CompetitionIndex, 0, comps.Count - 1);
			Session.Instance.CompetitionIndex = idx;
			seletor.Selected = idx;
		}
		interno = false;
		FillStandings();
	}

	void FillStandings()
	{
		var lista = GetNode<VBoxContainer>("%TableList");
		var art = GetNode<VBoxContainer>("%ScorerList");
		Nodes.Clear(lista);
		Nodes.Clear(art);
		var h = IsHuman();
		var comps = h == null ? new List<Competition>() : Session.Instance.Current.Competitions.Where(c => c.ClubsInCompetition.Contains(h.Slug)).ToList();
		if (comps.Count == 0)
		{
			var vazio = linhaTexto.Instantiate<Label>();
			vazio.Text = Translator.T("office.no_competition");
			lista.AddChild(vazio);
			return;
		}
		int idx = Mathf.Clamp(Session.Instance.CompetitionIndex, 0, comps.Count - 1);
		var comp = comps[idx];
		var tab = MatchEngine.Standings(Session.Instance.Current, comp);
		for (int i = 0; i < tab.Count; i++)
		{
			var row = faixaTabela.Instantiate<StandingsRow>();
			lista.AddChild(row);
			row.Fill(i + 1, tab[i]);
			int tipo = 0;
			if (i < 4 && comp.Division == 1) tipo = 1;
			else if (comp.RelegationSpots > 0 && i >= tab.Count - comp.RelegationSpots) tipo = 2;
			else if (tab[i].Slug == h.Slug) tipo = 3;
			row.Paint(tipo);
		}
		int pos = 1;
		foreach (var a in MatchEngine.TopScorers(Session.Instance.Current, comp, 10))
		{
			var item = linhaTexto.Instantiate<Label>();
			item.Text = pos + ". " + a.nome + " (" + a.time + ")  " + a.gols;
			art.AddChild(item);
			pos++;
		}
	}

	void BuildCalendar()
	{
		var lista = GetNode<VBoxContainer>("%CalendarList");
		Nodes.Clear(lista);
		var h = IsHuman();
		if (h == null) return;
		var est = Session.Instance.Current;
		var jogos = est.Competitions.SelectMany(c => c.Matches.Select(p => (c, p)))
			.Where(x => x.p.HomeSlug == h.Slug || x.p.AwaySlug == h.Slug)
			.OrderBy(x => x.p.MatchDate).ToList();
		AddHeading(lista, Translator.T("office.results"));
		foreach (var (c, p) in jogos.Where(x => x.p.Played).TakeLast(8))
			AddLine(lista, MatchLine(c, p, h.Slug));
		AddHeading(lista, Translator.T("office.upcoming"));
		foreach (var (c, p) in jogos.Where(x => !x.p.Played && x.p.HomeSlug != "?").Take(12))
			AddLine(lista, MatchLine(c, p, h.Slug));
	}

	string MatchLine(Competition c, Match p, string eu)
	{
		var est = Session.Instance.Current;
		string adv = p.HomeSlug == eu ? est.FindClub(p.AwaySlug)?.Name : est.FindClub(p.HomeSlug)?.Name;
		string mando = p.HomeSlug == eu ? Translator.T("office.home") : Translator.T("office.away");
		string plac = p.Played ? (p.HomeSlug == eu ? p.HomeGoals + " x " + p.AwayGoals : p.AwayGoals + " x " + p.HomeGoals) : "x";
		return Rules.ShortDate(p.MatchDate) + "   " + plac + "   " + adv + " (" + mando + ")   " + c.Name;
	}

	void BuildFinances()
	{
		var lista = GetNode<VBoxContainer>("%FinanceList");
		Nodes.Clear(lista);
		var c = IsHuman();
		if (c == null) return;
		long folha = c.Squad.Sum(j => (long)j.Wage);
		long patrocinio = c.Reputation * 90_000L + c.Level * 25_000L + (c.Division == 1 ? 180_000 : 0);
		long elenco = c.Squad.Sum(j => (long)j.Price);
		AddHeading(lista, string.Format(Translator.T("office.funds"), Rules.Money(c.Funds)));
		AddLine(lista, string.Format(Translator.T("office.wages"), Rules.Money(folha)));
		AddLine(lista, string.Format(Translator.T("office.sponsor"), Rules.Money(patrocinio)));
		AddLine(lista, string.Format(Translator.T("office.squad_value"), Rules.Money(elenco)));
		AddLine(lista, string.Format(Translator.T("office.stadium"), c.Stadium, c.Capacity.ToString("N0")));
		var dica = linhaTexto.Instantiate<Label>();
		dica.Text = Translator.T("office.gate");
		dica.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		dica.CustomMinimumSize = new Vector2(640, 0);
		lista.AddChild(dica);
		AddHeading(lista, Translator.T("office.top_earners"));
		foreach (var j in c.Squad.OrderByDescending(j => j.Wage).Take(8))
			AddLine(lista, j.Name + "   " + Rules.Money(j.Wage) + "   " + string.Format(Translator.T("common.contract"), j.ContractYears));
	}

	void BuildMarket()
	{
		var lista = GetNode<VBoxContainer>("%MarketList");
		Nodes.Clear(lista);
		var h = IsHuman();
		if (h == null) return;
		string filtro = GetNode<LineEdit>("%MarketSearch").Text ?? "";
		var query = Session.Instance.Current.ClubsInCompetition.Where(c => c.Slug != h.Slug)
			.SelectMany(c => c.Squad.Select(j => (c, j)))
			.Where(x => x.j.Strength >= 20 && (string.IsNullOrWhiteSpace(filtro) || x.j.Name.Contains(filtro, StringComparison.OrdinalIgnoreCase) || x.c.Name.Contains(filtro, StringComparison.OrdinalIgnoreCase)))
			.OrderByDescending(x => x.j.ListedForSale).ThenByDescending(x => x.j.Strength)
			.Take(25);
		foreach (var (c, j) in query)
		{
			var b = botaoLinha.Instantiate<Button>();
			string venda = j.ListedForSale ? "  " + Translator.T("office.for_sale") : "";
			b.Text = j.Name + "  " + Rules.Position(j.Position) + "  " + string.Format(Translator.T("common.years"), j.Age) + "  " + j.Strength + "  " + Rules.Money(j.Price) + "  " + c.Name + venda;
			var cc = c;
			var jj = j;
			b.Pressed += () => OpenOffer(cc, jj);
			lista.AddChild(b);
		}
		AddHeading(lista, Translator.T("office.your_squad"));
		foreach (var j in h.Squad.OrderByDescending(j => j.Strength))
		{
			var b = botaoLinha.Instantiate<Button>();
			b.Text = (j.ListedForSale ? Translator.T("office.listed") + " " : "") + j.Name + "  " + Rules.Money(j.Price);
			var jj = j;
			b.Pressed += () => { jj.ListedForSale = !jj.ListedForSale; BuildMarket(); };
			lista.AddChild(b);
		}
	}

	void OpenOffer(Club c, Player j)
	{
		Session.Instance.MarketClub = c;
		Session.Instance.MarketPlayer = j;
		var h = IsHuman();
		GetNode<Label>("%OfferTitle").Text = string.Format(Translator.T("market.offer"), j.Name);
		GetNode<Label>("%OfferDetail").Text = c.Name + " · " + Translator.T("position." + Mathf.Clamp(j.Position, 0, 4)) + " · " + string.Format(Translator.T("common.years"), j.Age) + " · " + j.Strength + " · " + j.TraitSummary;
		GetNode<Label>("%OfferPrice").Text = string.Format(Translator.T("office.asking_price"), Rules.Money(j.Price), Rules.Money(h?.Funds ?? 0));
		GetNode<LineEdit>("%OfferValue").Text = j.Price.ToString();
		GetNode<Button>("%SendOffer").Text = Translator.T("office.send");
		GetNode<Button>("%CloseOffer").Text = Translator.T("office.cancel");
		GetNode<Control>("%Offer").Visible = true;
	}

	void SendOffer()
	{
		var h = IsHuman();
		var j = Session.Instance.MarketPlayer;
		var c = Session.Instance.MarketClub;
		if (h == null || j == null || c == null) return;
		string limpo = GetNode<LineEdit>("%OfferValue").Text.Replace(".", "").Replace(",", "").Trim();
		if (!int.TryParse(limpo, out int valor))
		{
			Alert(Translator.T("market.bad_amount"));
			return;
		}
		string msg = MatchEngine.TryPurchase(Session.Instance.Current, h, j, c, valor);
		GetNode<Control>("%Offer").Visible = false;
		Alert(Translator.F(msg), Refresh);
	}

	void BuildTraining()
	{
		var c = IsHuman();
		GetNode<Label>("%TrainingNotice").Text = c != null && c.HasTrained ? Translator.T("office.already_trained") : Translator.T("office.can_train");
	}

	void Train(Action<Club> efeito)
	{
		var c = IsHuman();
		if (c == null || c.HasTrained) return;
		efeito(c);
		c.HasTrained = true;
		Refresh();
	}

	void BuildOptions()
	{
		var est = Session.Instance.Current;
		GetNode<Label>("%Competitions").Text = string.Format(
			Translator.T("options.competitions"),
			est.PlayStateCup ? Translator.T("common.yes") : Translator.T("common.no"),
			est.PlayNationalCup ? Translator.T("common.yes") : Translator.T("common.no"),
			est.PlayContinental ? Translator.T("common.yes") : Translator.T("common.no"));
		var lista = GetNode<VBoxContainer>("%History");
		Nodes.Clear(lista);
		foreach (var h in est.History.TakeLast(12))
			AddLine(lista, Translator.F(h));
	}

	void AddHeading(VBoxContainer lista, string texto)
	{
		var item = linhaTexto.Instantiate<Label>();
		item.Text = texto;
		item.AddThemeColorOverride("font_color", new Color(0.94f, 0.82f, 0.38f));
		lista.AddChild(item);
	}

	void AddLine(VBoxContainer lista, string texto)
	{
		var item = linhaTexto.Instantiate<Label>();
		item.Text = texto;
		lista.AddChild(item);
	}
}
