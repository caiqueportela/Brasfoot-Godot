using System.Globalization;
using System.Text;
using Godot;

namespace Brasfoot;

public partial class SquadEditor : ScreenBase
{
	static readonly int[][][] CrPorPos =
	{
		new[] { new[] { 0, 3 }, new[] { 0, 1 }, new[] { 2, 0 }, new[] { 1, 2 }, new[] { 3, 1 }, new[] { 0, 2 } },
		new[] { new[] { 6, 10 }, new[] { 6, 13 }, new[] { 10, 11 }, new[] { 10, 13 }, new[] { 10, 6 }, new[] { 10, 9 }, new[] { 6, 11 } },
		new[] { new[] { 7, 10 }, new[] { 7, 12 }, new[] { 7, 5 }, new[] { 10, 13 }, new[] { 7, 13 }, new[] { 5, 12 } },
		new[] { new[] { 4, 11 }, new[] { 4, 9 }, new[] { 9, 11 }, new[] { 11, 9 }, new[] { 4, 8 }, new[] { 4, 13 } },
		new[] { new[] { 9, 5 }, new[] { 13, 9 }, new[] { 8, 9 }, new[] { 9, 13 }, new[] { 5, 13 }, new[] { 9, 11 } }
	};
	readonly List<ClubRecord> ordem = new();
	readonly List<PlayerRecord> exibidos = new();
	bool principal = true;
	bool adicionandoTime = true;
	bool adicionandoJogador = true;
	bool interno;
	bool jogNomeCrescente = true;
	bool jogPosCrescente = true;
	bool jogIdadeCrescente = true;
	int ordenarJogador = 3;
	int colunaTime;
	bool crescenteTime = true;
	Func<ClubRecord, bool> filtro;
	string ultimaBuscaTime = "";
	int cursorTime;
	int cursorTimeJog;
	int cursorJog;
	string criado = "";

	public override void _Ready()
	{
		Wire();
		base._Ready();
		if (OS.GetEnvironment("BRASFOOT_FOTO_EDITOR") == "1")
		{
			GetTree().CreateTimer(0.7).Timeout += () =>
			{
				var alvo = ordem.FirstOrDefault(t => (t.Name ?? "").Contains("Agosto", StringComparison.OrdinalIgnoreCase)) ?? ordem.FirstOrDefault();
				if (alvo != null)
				{
					GetNode<EditorGrid>("%ClubGrid").SelectRow(ordem.IndexOf(alvo));
					Show(alvo);
				}
				GetTree().CreateTimer(0.35).Timeout += () =>
				{
					Screenshot.Save(this, "verificacao_editor.png");
					GD.Print($"EDITOR time={alvo?.Name} jog={exibidos.Count} times={ordem.Count}");
					GetTree().Quit();
				};
			};
		}
	}

	protected override void ApplyTexts()
	{
		SetLabel("%Add", "editor.add");
		SetLabel("%Edit", "editor.edit");
		SetLabel("%Delete", "editor.delete");
		SetLabel("%Home", "editor.home");
		SetLabel("%Quit", "editor.quit");
		SetLabel("%FirstTeam", "editor.first_team");
		SetLabel("%Youth", "editor.reserves");
		SetLabel("%Random", "editor.random");
		SetLabel("%MissingCrest", "editor.no_crest");
		SetLabel("%MissingShirts", "editor.no_shirts");
		SetLabel("%Duplicates", "editor.duplicates");
		GetNode<Label>("%LabelClub").Text = Translator.T("editor.find_club");
		GetNode<Label>("%LabelPlayer").Text = Translator.T("editor.find_player");
		GetNode<Label>("%LabelShow").Text = Translator.T("editor.show");
		var gradeTimes = GetNode<EditorGrid>("%ClubGrid");
		gradeTimes.Build(
			new[] { Translator.T("editor.column.club"), Translator.T("editor.column.country"), Translator.T("editor.column.level") },
			new[] { 0.52f, 0.34f, 0.14f }, false);
		gradeTimes.SetHeaderTips(new[] { Translator.T("editor.tip.col_club"), Translator.T("editor.tip.col_country"), Translator.T("editor.tip.col_level") });
		var gradeJogadores = GetNode<EditorGrid>("%PlayerGrid");
		gradeJogadores.Build(
			new[] { "", "", Translator.T("editor.column.name"), Translator.T("editor.column.position"), Translator.T("editor.column.country"), Translator.T("editor.column.age"), Translator.T("editor.column.trait"), Translator.T("editor.column.side") },
			new[] { 0.045f, 0.045f, 0.25f, 0.14f, 0.15f, 0.07f, 0.11f, 0.14f }, true);
		gradeJogadores.SetHeaderTips(new[]
		{
			Translator.T("editor.tip.col_shirt"), Translator.T("editor.tip.col_star"), Translator.T("editor.tip.col_name"),
			Translator.T("editor.tip.col_position"), Translator.T("editor.tip.col_country"), Translator.T("editor.tip.col_age"),
			Translator.T("editor.tip.col_trait"), Translator.T("editor.tip.col_side")
		});
		ApplyStaticTips();
		ApplyPlayerFormLabels();
		var menu = GetNode<PopupMenu>("%PlayerMenu");
		menu.Clear();
		menu.AddItem(Translator.T("editor.new_player"), 0);
		menu.AddItem(Translator.T("editor.edit_player"), 1);
		menu.AddItem(Translator.T("editor.delete"), 2);
		menu.AddItem(Translator.T("editor.transfer"), 3);
		FillPositions(GetNode<OptionButton>("%RandomPosition"), 0);
		ReloadClubs(TimeAtual());
	}

	void Wire()
	{
		foreach (var b in new[] { "%Add", "%Edit", "%Delete", "%Home", "%Quit", "%FirstTeam", "%Youth", "%Random", "%MissingCrest", "%MissingShirts", "%Duplicates", "%ClubSearch", "%PlayerSearch", "%ClubOk", "%ClubCancel", "%PlayerOk", "%PlayerCancel", "%TransferOk", "%TransferCancel", "%ConfirmYes", "%ConfirmNo" })
			Brighten(GetNode<Button>(b));
		Brighten(GetNode<OptionButton>("%RandomPosition"));
		foreach (var e in new[] { "%ClubSearchText", "%PlayerSearchText", "%ClubName", "%ClubCoach", "%ClubStadium", "%ClubCapacity", "%ClubFile", "%PlayerName" })
			PaintField(GetNode<LineEdit>(e));
		GetNode<Button>("%Add").Icon = EditorIcons.Get("add");
		GetNode<Button>("%Edit").Icon = EditorIcons.Get("pencil");
		GetNode<Button>("%Delete").Icon = EditorIcons.Get("x");
		GetNode<Button>("%Home").Icon = EditorIcons.Get("home");
		GetNode<Button>("%Quit").Icon = EditorIcons.Get("quit");
		GetNode<Button>("%ClubSearch").Icon = EditorIcons.Get("search");
		GetNode<Button>("%PlayerSearch").Icon = EditorIcons.Get("search");
		GetNode<Button>("%AddPlayer").Icon = EditorIcons.Get("add");
		GetNode<Button>("%EditPlayer").Icon = EditorIcons.Get("pencil");
		GetNode<Button>("%DeletePlayer").Icon = EditorIcons.Get("x");
		GetNode<Button>("%Random").Icon = EditorIcons.Get("add");
		var grupo = GetNode<PanelContainer>("%RandomGroup");
		var faixa = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0.28f), BorderColor = new Color(1, 1, 1, 0.72f) };
		faixa.SetBorderWidthAll(1);
		faixa.SetCornerRadiusAll(4);
		faixa.SetContentMarginAll(6);
		grupo.AddThemeStyleboxOverride("panel", faixa);
		GetNode<TextureRect>("%CountIcon").Texture = EditorIcons.Get("people");
		GetNode<TextureRect>("%StartersIcon").Texture = EditorIcons.Get("shirt_green");
		GetNode<TextureRect>("%StadiumIcon").Texture = EditorIcons.Get("stadium");
		GetNode<TextureRect>("%CoachIcon").Texture = EditorIcons.Get("coach");
		foreach (var b in new[] { "%AddPlayer", "%EditPlayer", "%DeletePlayer" })
		{
			var botao = GetNode<Button>(b);
			botao.Text = "";
			Brighten(botao);
			botao.CustomMinimumSize = new Vector2(36, 30);
		}
		GetNode<Button>("%Add").Pressed += () => OpenClubForm(true);
		GetNode<Button>("%Edit").Pressed += () => OpenClubForm(false);
		GetNode<Button>("%Delete").Pressed += AskDeleteClub;
		GetNode<Button>("%Home").Pressed += () => Session.Instance.GoTo(Paths.Menu);
		GetNode<Button>("%Quit").Pressed += () => GetTree().Quit();
		GetNode<Button>("%FirstTeam").Pressed += () => SwitchSquad(true);
		GetNode<Button>("%Youth").Pressed += () => SwitchSquad(false);
		GetNode<Button>("%ClubSearch").Pressed += FindNextClub;
		GetNode<Button>("%PlayerSearch").Pressed += FindNextPlayer;
		GetNode<Button>("%MissingCrest").Pressed += () => ApplyFilter(t => !Crests.FileExists("crests", t.Slug));
		GetNode<Button>("%MissingShirts").Pressed += () => ApplyFilter(t => !Crests.FileExists("shirts", t.Slug));
		GetNode<Button>("%Duplicates").Pressed += () => ApplyFilter(Duplicado);
		GetNode<Button>("%AddPlayer").Pressed += () => OpenPlayerForm(true);
		GetNode<Button>("%EditPlayer").Pressed += () => OpenPlayerForm(false);
		GetNode<Button>("%DeletePlayer").Pressed += DeletePlayer;
		GetNode<Button>("%Random").Pressed += CreateRandomPlayer;
		GetNode<Button>("%ClubOk").Pressed += ConfirmClub;
		GetNode<Button>("%ClubCancel").Pressed += () => GetNode<Control>("%ClubForm").Visible = false;
		GetNode<Button>("%PlayerOk").Pressed += ConfirmPlayer;
		GetNode<Button>("%PlayerCancel").Pressed += () => GetNode<Control>("%PlayerForm").Visible = false;
		GetNode<Button>("%TransferOk").Pressed += ConfirmTransfer;
		GetNode<Button>("%TransferCancel").Pressed += () => GetNode<Control>("%TransferForm").Visible = false;
		GetNode<Button>("%ConfirmYes").Pressed += DeleteClub;
		GetNode<Button>("%ConfirmNo").Pressed += () => GetNode<Control>("%Confirm").Visible = false;
		GetNode<LineEdit>("%ClubSearchText").TextSubmitted += _ => FindNextClub();
		GetNode<LineEdit>("%PlayerSearchText").TextSubmitted += _ => FindNextPlayer();
		GetNode<HSlider>("%PlayerAge").ValueChanged += v => GetNode<Label>("%PlayerAgeValue").Text = Mathf.RoundToInt((float)v).ToString();
		GetNode<OptionButton>("%PlayerPosition").ItemSelected += _ =>
		{
			if (!interno) FillTraits(SelectedPosition(), 0, 1);
		};
		GetNode<OptionButton>("%ClubCountry").ItemSelected += _ =>
		{
			if (!interno) UpdateStateField();
		};
		var times = GetNode<EditorGrid>("%ClubGrid");
		times.RowSelected += i => { if (i >= 0 && i < ordem.Count) Show(ordem[i]); };
		times.RowActivated += _ => OpenClubForm(false);
		times.HeaderClicked += SortClubs;
		var jogadores = GetNode<EditorGrid>("%PlayerGrid");
		jogadores.CellClicked += (i, c) => { if (c == 0) ToggleStarter(i); };
		jogadores.RowActivated += _ => OpenPlayerForm(false);
		jogadores.RightClicked += i =>
		{
			jogadores.SelectRow(i);
			var menu = GetNode<PopupMenu>("%PlayerMenu");
			menu.Position = (Vector2I)GetViewport().GetMousePosition();
			menu.Popup();
		};
		jogadores.HeaderClicked += SortPlayers;
		GetNode<PopupMenu>("%PlayerMenu").IdPressed += id =>
		{
			if (id == 0) OpenPlayerForm(true);
			else if (id == 1) OpenPlayerForm(false);
			else if (id == 2) DeletePlayer();
			else OpenTransfer();
		};
		foreach (var nome in new[] { "%Name", "%Reputation", "%Country", "%State", "%Stadium", "%Capacity", "%Coach", "%Level" })
		{
			var rotulo = GetNode<Label>(nome);
			rotulo.MouseFilter = MouseFilterEnum.Stop;
			rotulo.GuiInput += e =>
			{
				if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
					OpenClubForm(false);
			};
		}
	}

	void ReloadClubs(ClubRecord manter)
	{
		var banco = Catalog.Instance?.CatalogData;
		ordem.Clear();
		if (banco == null) return;
		ordem.AddRange(banco.Clubs);
		if (colunaTime == 2)
			ordem.Sort((a, b) => crescenteTime ? a.Level.CompareTo(b.Level) : b.Level.CompareTo(a.Level));
		else
			ordem.Sort((a, b) =>
			{
				int n = colunaTime == 1
					? string.Compare(banco.CountryName(a.CountryId), banco.CountryName(b.CountryId), StringComparison.CurrentCultureIgnoreCase)
					: string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
				if (!crescenteTime) n = -n;
				return n != 0 ? n : b.Level.CompareTo(a.Level);
			});
		var linhas = new List<EditorGrid.GridCell[]>();
		foreach (var t in ordem)
		{
			linhas.Add(new[]
			{
				new EditorGrid.GridCell { Text = t.Name, Tip = t.Name },
				new EditorGrid.GridCell { Text = banco.CountryName(t.CountryId), Icon = Flags.CountryFlag(t.CountryId), Tip = banco.CountryName(t.CountryId) },
				new EditorGrid.GridCell { Text = t.Level.ToString(), Centered = true, Tip = string.Format(Translator.T("editor.tip.level"), t.Level) }
			});
		}
		int indice = manter == null ? 0 : Math.Max(0, ordem.IndexOf(manter));
		var grade = GetNode<EditorGrid>("%ClubGrid");
		grade.Fill(linhas, indice);
		grade.SetSort(colunaTime, crescenteTime);
		if (filtro != null) grade.Mark(ordem.Select(filtro).ToList());
		if (ordem.Count > 0) Show(ordem[indice]);
	}

	void Show(ClubRecord time)
	{
		if (time == null) return;
		var tinta = Html(time.TextColor);
		var fundo = Html(time.BackgroundColor);
		GetNode<ColorRect>("%ClubBackgroundRect").Color = fundo;
		foreach (var nome in new[] { "%Name", "%Reputation", "%Country", "%State", "%Stadium", "%Capacity", "%Coach", "%Level", "%Count", "%Starters", "%Created", "%AtPosition" })
			GetNode<Label>(nome).AddThemeColorOverride("font_color", tinta);
		GetNode<Label>("%Name").Text = time.Name;
		GetNode<Label>("%Reputation").Text = Translator.T("editor.reputation." + Mathf.Clamp(time.Reputation, 0, 5));
		GetNode<Label>("%Country").Text = Catalog.Instance.CatalogData.CountryName(time.CountryId);
		GetNode<TextureRect>("%CountryFlag").Texture = Flags.CountryFlag(time.CountryId);
		bool brasil = time.CountryId == 29;
		GetNode<TextureRect>("%StateFlag").Visible = brasil;
		GetNode<Label>("%State").Visible = brasil;
		if (brasil)
		{
			GetNode<Label>("%State").Text = Catalog.Instance.CatalogData.StateName(time.StateId);
			GetNode<TextureRect>("%StateFlag").Texture = Flags.StateFlag(time.StateId);
		}
		GetNode<Label>("%Stadium").Text = time.Stadium;
		GetNode<Label>("%Capacity").Text = string.Format(Translator.T("editor.seats"), time.Capacity);
		GetNode<Label>("%Coach").Text = time.Coach;
		GetNode<TextureRect>("%CoachFlag").Texture = Flags.CountryFlag(time.CoachCountryId);
		GetNode<Label>("%Level").Text = time.Level.ToString();
		GetNode<TextureRect>("%Crest").Texture = Crests.Get(time.Slug) ?? Crests.Get(time.Slug, true);
		GetNode<TextureRect>("%Shirt1").Texture = Crests.Shirt(time.Slug, 1) ?? EditorIcons.Get("empty");
		GetNode<TextureRect>("%Shirt2").Texture = Crests.Shirt(time.Slug, 2) ?? EditorIcons.Get("empty");
		GetNode<TextureRect>("%Shirt3").Texture = Crests.Shirt(time.Slug, 3) ?? EditorIcons.Get("empty");
		GetNode<Label>("%Created").Text = criado;
		ApplyClubTips(time);
		GetNode<Button>("%FirstTeam").Icon = principal ? EditorIcons.Get("check") : null;
		GetNode<Button>("%Youth").Icon = principal ? null : EditorIcons.Get("check");
		FillPlayers();
	}

	void FillPlayers(int manter = 0)
	{
		var time = TimeAtual();
		exibidos.Clear();
		if (time == null) return;
		exibidos.AddRange(principal ? time.Players : time.Youth);
		exibidos.Sort(ComparePlayers);
		var linhas = new List<EditorGrid.GridCell[]>();
		foreach (var j in exibidos)
		{
			Texture2D marca = j.WorldClass ? EditorIcons.Get("star_red") : j.IsStar ? EditorIcons.Get("star") : null;
			linhas.Add(new[]
			{
				new EditorGrid.GridCell { Icon = EditorIcons.Get(j.LineupStatus == 1 ? "shirt_green" : "shirt_gray"), Tip = Translator.T(j.LineupStatus == 1 ? "editor.tip.starter" : "editor.tip.bench") },
				new EditorGrid.GridCell { Icon = marca, Tip = j.WorldClass ? Translator.T("editor.tip.world_class") : j.IsStar ? Translator.T("editor.tip.star") : Translator.T("editor.tip.no_star") },
				new EditorGrid.GridCell { Text = j.Name, Tip = j.Name },
				new EditorGrid.GridCell { Text = Translator.T("position." + Mathf.Clamp(j.Position, 0, 4)), Tip = Translator.T("position." + Mathf.Clamp(j.Position, 0, 4)) },
				new EditorGrid.GridCell { Text = Code(j.CountryId), Icon = Flags.CountryFlag(j.CountryId), Tip = Catalog.Instance.CatalogData.CountryName(j.CountryId) },
				new EditorGrid.GridCell { Text = j.Age.ToString(), Centered = true, Tip = string.Format(Translator.T("common.years"), j.Age) },
				new EditorGrid.GridCell { Text = Rules.TraitCode(j.PrimaryTrait) + "/" + Rules.TraitCode(j.SecondaryTrait), Centered = true, Tip = Rules.TraitName(j.PrimaryTrait) + " / " + Rules.TraitName(j.SecondaryTrait) },
				new EditorGrid.GridCell { Text = Rules.Side(j.Side), Centered = true, Tip = Rules.Side(j.Side) }
			});
		}
		var grade = GetNode<EditorGrid>("%PlayerGrid");
		grade.Fill(linhas, manter);
		grade.SetSort(ordenarJogador, PlayerSortAscending());
		int max = principal ? 30 : 15;
		int titulares = exibidos.Count(j => j.LineupStatus == 1);
		GetNode<Label>("%Count").Text = exibidos.Count + "/" + max;
		GetNode<Label>("%Starters").Text = titulares.ToString();
	}

	int ComparePlayers(PlayerRecord a, PlayerRecord b)
	{
		int n = ordenarJogador switch
		{
			2 => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase),
			5 => a.Age.CompareTo(b.Age),
			_ => a.Position.CompareTo(b.Position)
		};
		if (ordenarJogador == 2 && !jogNomeCrescente) n = -n;
		if (ordenarJogador == 3 && !jogPosCrescente) n = -n;
		if (ordenarJogador == 5 && !jogIdadeCrescente) n = -n;
		return n != 0 ? n : string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
	}

	bool PlayerSortAscending() => ordenarJogador switch
	{
		2 => jogNomeCrescente,
		5 => jogIdadeCrescente,
		_ => jogPosCrescente
	};

	void SortClubs(int coluna)
	{
		if (coluna == colunaTime) crescenteTime = !crescenteTime;
		else
		{
			colunaTime = coluna;
			crescenteTime = true;
		}
		ReloadClubs(TimeAtual());
	}

	void SortPlayers(int coluna)
	{
		if (coluna == 2) { ordenarJogador = 2; jogNomeCrescente = !jogNomeCrescente; }
		else if (coluna == 5) { ordenarJogador = 5; jogIdadeCrescente = !jogIdadeCrescente; }
		else if (coluna == 3) { ordenarJogador = 3; jogPosCrescente = !jogPosCrescente; }
		else return;
		FillPlayers();
	}

	void SwitchSquad(bool primeiro)
	{
		principal = primeiro;
		criado = "";
		var time = TimeAtual();
		if (time != null) Show(time);
	}

	void ToggleStarter(int indice)
	{
		if (indice < 0 || indice >= exibidos.Count) return;
		var j = exibidos[indice];
		int titulares = exibidos.Count(x => x.LineupStatus == 1);
		if (j.LineupStatus == 1) j.LineupStatus = 0;
		else if (titulares < 15) j.LineupStatus = 1;
		FillPlayers(indice);
		Persist();
	}

	void FindNextClub()
	{
		string q = SearchKey(GetNode<LineEdit>("%ClubSearchText").Text);
		if (q.Length == 0 || ordem.Count == 0) return;
		if (q != ultimaBuscaTime)
		{
			ultimaBuscaTime = q;
			cursorTime = 0;
		}
		for (int n = 0; n < ordem.Count; n++)
		{
			int i = (cursorTime + n) % ordem.Count;
			string nome = SearchKey(ordem[i].Name);
			if (nome.Contains(q))
			{
				cursorTime = (i + 1) % ordem.Count;
				GetNode<EditorGrid>("%ClubGrid").SelectRow(i);
				Show(ordem[i]);
				return;
			}
		}
	}

	void FindNextPlayer()
	{
		string q = SearchKey(GetNode<LineEdit>("%PlayerSearchText").Text);
		if (q.Length == 0 || ordem.Count == 0) return;
		for (int n = 0; n < ordem.Count; n++)
		{
			int t = (cursorTimeJog + n) % ordem.Count;
			var elenco = ordem[t].Players;
			int inicio = t == cursorTimeJog ? cursorJog : 0;
			for (int j = inicio; j < elenco.Count; j++)
			{
				if (SearchKey(elenco[j].Name).Contains(q))
				{
					cursorTimeJog = t;
					cursorJog = j + 1;
					principal = true;
					GetNode<EditorGrid>("%ClubGrid").SelectRow(t);
					Show(ordem[t]);
					int visivel = exibidos.IndexOf(elenco[j]);
					if (visivel >= 0) GetNode<EditorGrid>("%PlayerGrid").SelectRow(visivel);
					return;
				}
			}
		}
		cursorTimeJog = 0;
		cursorJog = 0;
	}

	void ApplyFilter(Func<ClubRecord, bool> teste)
	{
		filtro = teste;
		var marcas = ordem.Select(teste).ToList();
		var grade = GetNode<EditorGrid>("%ClubGrid");
		grade.Mark(marcas);
		int i = marcas.FindIndex(m => m);
		if (i < 0) return;
		grade.SelectRow(i);
		Show(ordem[i]);
	}

	bool Duplicado(ClubRecord time)
	{
		var vistos = new HashSet<string>();
		foreach (var j in time.Players)
			if (!vistos.Add(j.Name ?? "")) return true;
		return false;
	}

	void OpenClubForm(bool novo)
	{
		if (!novo && TimeAtual() == null) return;
		adicionandoTime = novo;
		var time = TimeAtual();
		GetNode<Label>("%ClubTitle").Text = Translator.T(novo ? "editor.new_club" : "editor.edit_club");
		GetNode<Label>("%ClubError").Text = "";
		GetNode<LineEdit>("%ClubName").Text = novo ? "" : time.Name;
		GetNode<LineEdit>("%ClubCoach").Text = novo ? "" : time.Coach;
		GetNode<LineEdit>("%ClubStadium").Text = novo ? "" : time.Stadium;
		GetNode<LineEdit>("%ClubCapacity").Text = novo ? "10000" : time.Capacity.ToString();
		GetNode<LineEdit>("%ClubFile").Text = novo ? "" : time.Slug;
		GetNode<LineEdit>("%ClubFile").Editable = novo;
		FillRange(GetNode<OptionButton>("%ClubLevel"), 1, 20, novo ? 10 : time.Level);
		FillReputation(GetNode<OptionButton>("%ClubReputation"), novo ? 3 : time.Reputation);
		FillCountries(GetNode<OptionButton>("%ClubCountry"), novo ? 29 : time.CountryId);
		FillCountries(GetNode<OptionButton>("%ClubCoachCountry"), novo ? 29 : time.CoachCountryId);
		FillStates(novo ? 0 : time.StateId);
		GetNode<ColorPickerButton>("%ClubTextColor").Color = Html(novo ? "#ffffff" : time.TextColor);
		GetNode<ColorPickerButton>("%ClubBackground").Color = Html(novo ? "#cc0000" : time.BackgroundColor);
		UpdateStateField();
		ApplyClubFormLabels();
		GetNode<Control>("%ClubForm").Visible = true;
	}

	void ConfirmClub()
	{
		string nome = GetNode<LineEdit>("%ClubName").Text.Trim();
		string tecnico = GetNode<LineEdit>("%ClubCoach").Text.Trim();
		string estadio = GetNode<LineEdit>("%ClubStadium").Text.Trim();
		string capacidadeText = GetNode<LineEdit>("%ClubCapacity").Text.Trim();
		string arquivo = GetNode<LineEdit>("%ClubFile").Text.Trim();
		if (nome.Length < 2 || nome.Length > 30) { ShowClubError("editor.bad_name"); return; }
		if (tecnico.Length < 2 || tecnico.Length > 40) { ShowClubError("editor.bad_coach"); return; }
		if (estadio.Length < 2 || estadio.Length > 40) { ShowClubError("editor.bad_stadium"); return; }
		if (!int.TryParse(capacidadeText, out int capacidade) || capacidade < 1000 || capacidade > 120000)
		{
			ShowClubError("editor.bad_capacity");
			return;
		}
		var banco = Catalog.Instance.CatalogData;
		ClubRecord time;
		if (adicionandoTime)
		{
			if (arquivo.Length < 2 || arquivo.Length > 30) { ShowClubError("editor.bad_file"); return; }
			if (banco.Clubs.Any(t => t.Slug.Equals(arquivo, StringComparison.OrdinalIgnoreCase)))
			{
				ShowClubError("editor.file_exists");
				return;
			}
			time = new ClubRecord { Slug = arquivo };
			banco.Clubs.Insert(0, time);
		}
		else time = TimeAtual();
		time.Name = nome;
		time.Coach = tecnico;
		time.Stadium = estadio;
		time.Capacity = capacidade;
		time.Level = Item(GetNode<OptionButton>("%ClubLevel"));
		time.Reputation = Item(GetNode<OptionButton>("%ClubReputation"));
		time.CountryId = Item(GetNode<OptionButton>("%ClubCountry"));
		time.CoachCountryId = Item(GetNode<OptionButton>("%ClubCoachCountry"));
		if (time.CountryId == 29) time.StateId = Item(GetNode<OptionButton>("%ClubState"));
		time.BackgroundColor = Hex(GetNode<ColorPickerButton>("%ClubBackground").Color);
		time.TextColor = Hex(GetNode<ColorPickerButton>("%ClubTextColor").Color);
		GetNode<Control>("%ClubForm").Visible = false;
		Persist();
		ReloadClubs(time);
	}

	void AskDeleteClub()
	{
		if (TimeAtual() == null) return;
		GetNode<Label>("%ConfirmText").Text = Translator.T("editor.delete_club");
		GetNode<Button>("%ConfirmYes").Text = Translator.T("editor.confirm_delete");
		GetNode<Button>("%ConfirmNo").Text = Translator.T("editor.cancel");
		GetNode<Control>("%Confirm").Visible = true;
	}

	void DeleteClub()
	{
		GetNode<Control>("%Confirm").Visible = false;
		var time = TimeAtual();
		if (time == null) return;
		Catalog.Instance.CatalogData.Clubs.Remove(time);
		Persist();
		ReloadClubs(null);
	}

	void OpenPlayerForm(bool novo)
	{
		var time = TimeAtual();
		if (time == null) return;
		var lista = Squad(time);
		if (novo && lista.Count >= (principal ? 30 : 15))
		{
			Alert(Translator.T("editor.limit"));
			return;
		}
		if (!novo && JogadorAtual() == null) return;
		adicionandoJogador = novo;
		var j = novo ? null : JogadorAtual();
		GetNode<Label>("%PlayerTitle").Text = Translator.T(novo ? "editor.new_player" : "editor.edit_player");
		GetNode<Label>("%PlayerError").Text = "";
		GetNode<LineEdit>("%PlayerName").Text = novo ? "" : j.Name;
		FillPositions(GetNode<OptionButton>("%PlayerPosition"), novo ? 0 : j.Position);
		FillSides(novo ? 0 : j.Side);
		FillCountries(GetNode<OptionButton>("%PlayerCountry"), novo ? time.CountryId : j.CountryId);
		FillTraits(novo ? 0 : j.Position, novo ? 0 : j.PrimaryTrait, novo ? 1 : j.SecondaryTrait);
		var idade = GetNode<HSlider>("%PlayerAge");
		idade.MinValue = 16;
		idade.MaxValue = principal ? 48 : 20;
		idade.Value = novo ? (principal ? 20 : 17) : j.Age;
		GetNode<Label>("%PlayerAgeValue").Text = Mathf.RoundToInt((float)idade.Value).ToString();
		GetNode<CheckBox>("%PlayerStar").ButtonPressed = !novo && j.IsStar;
		GetNode<CheckBox>("%PlayerTop").ButtonPressed = !novo && j.WorldClass;
		GetNode<CheckBox>("%PlayerKeep").Visible = novo;
		GetNode<CheckBox>("%PlayerKeep").ButtonPressed = false;
		ApplyPlayerFormLabels();
		GetNode<Control>("%PlayerForm").Visible = true;
	}

	void ConfirmPlayer()
	{
		var time = TimeAtual();
		if (time == null) return;
		string nome = GetNode<LineEdit>("%PlayerName").Text.Trim();
		if (nome.Length == 0) { ShowPlayerError("editor.bad_player"); return; }
		if (nome.Length < 2) { ShowPlayerError("editor.name_short"); return; }
		if (nome.Length > 30) { ShowPlayerError("editor.name_long"); return; }
		var lista = Squad(time);
		PlayerRecord j;
		if (adicionandoJogador)
		{
			if (lista.Count >= (principal ? 30 : 15)) { ShowPlayerError("editor.limit"); return; }
			j = new PlayerRecord();
			lista.Add(j);
		}
		else j = JogadorAtual();
		j.Name = nome;
		j.Position = Item(GetNode<OptionButton>("%PlayerPosition"));
		j.Side = Item(GetNode<OptionButton>("%PlayerSide"));
		j.CountryId = Item(GetNode<OptionButton>("%PlayerCountry"));
		j.PrimaryTrait = Item(GetNode<OptionButton>("%PlayerTrait1"));
		j.SecondaryTrait = Item(GetNode<OptionButton>("%PlayerTrait2"));
		j.Age = Mathf.RoundToInt((float)GetNode<HSlider>("%PlayerAge").Value);
		j.IsStar = GetNode<CheckBox>("%PlayerStar").ButtonPressed;
		j.WorldClass = GetNode<CheckBox>("%PlayerTop").ButtonPressed;
		bool seguir = adicionandoJogador && GetNode<CheckBox>("%PlayerKeep").ButtonPressed;
		Persist();
		Show(time);
		int visivel = exibidos.IndexOf(j);
		if (visivel >= 0) GetNode<EditorGrid>("%PlayerGrid").SelectRow(visivel);
		if (seguir)
		{
			GetNode<LineEdit>("%PlayerName").Text = "";
			GetNode<Label>("%PlayerError").Text = "";
		}
		else GetNode<Control>("%PlayerForm").Visible = false;
	}

	void DeletePlayer()
	{
		var time = TimeAtual();
		var j = JogadorAtual();
		if (time == null || j == null) return;
		Squad(time).Remove(j);
		Persist();
		Show(time);
	}

	void CreateRandomPlayer()
	{
		var time = TimeAtual();
		if (time == null) return;
		var lista = Squad(time);
		if (lista.Count >= (principal ? 30 : 15))
		{
			Alert(Translator.T("editor.limit"));
			return;
		}
		int pos = Item(GetNode<OptionButton>("%RandomPosition"));
		var cr = CrPorPos[Mathf.Clamp(pos, 0, 4)];
		var par = cr[(int)GD.RandRange(0, cr.Length - 1)];
		string nome = Catalog.Instance.CatalogData.ComposeName(time.CountryId, new Random());
		var j = new PlayerRecord
		{
			Name = nome,
			CountryId = time.CountryId,
			Position = pos,
			Side = (int)GD.RandRange(0, 1),
			Age = principal ? (int)GD.RandRange(18, 37) : (int)GD.RandRange(16, 19),
			PrimaryTrait = par[0],
			SecondaryTrait = par[1]
		};
		lista.Add(j);
		criado = string.Format(Translator.T("editor.created"), nome);
		Persist();
		Show(time);
		int visivel = exibidos.IndexOf(j);
		if (visivel >= 0) GetNode<EditorGrid>("%PlayerGrid").SelectRow(visivel);
	}

	void OpenTransfer()
	{
		if (JogadorAtual() == null) return;
		var lista = GetNode<ItemList>("%TransferClubs");
		lista.Clear();
		foreach (var t in ordem)
		{
			if (t == TimeAtual()) continue;
			lista.AddItem(t.Name);
		}
		if (lista.ItemCount > 0) lista.Select(0);
		GetNode<Label>("%TransferTitle").Text = Translator.T("editor.transfer_title");
		GetNode<Button>("%TransferOk").Text = Translator.T("editor.ok");
		GetNode<Button>("%TransferCancel").Text = Translator.T("editor.cancel");
		GetNode<Control>("%TransferForm").Visible = true;
	}

	void ConfirmTransfer()
	{
		var lista = GetNode<ItemList>("%TransferClubs");
		if (lista.GetSelectedItems().Length == 0) return;
		string nome = lista.GetItemText(lista.GetSelectedItems()[0]);
		var destino = ordem.FirstOrDefault(t => t.Name == nome);
		var origem = TimeAtual();
		var j = JogadorAtual();
		if (destino == null || origem == null || j == null) return;
		var alvo = principal ? destino.Players : destino.Youth;
		if (alvo.Count >= (principal ? 30 : 15))
		{
			Alert(Translator.T("editor.limit"));
			return;
		}
		Squad(origem).Remove(j);
		alvo.Add(j);
		GetNode<Control>("%TransferForm").Visible = false;
		Persist();
		Show(origem);
	}

	void FillCountries(OptionButton lista, int selecionado)
	{
		interno = true;
		lista.Clear();
		PrepareList(lista);
		var paises = Catalog.Instance.CatalogData.Countries.OrderBy(p => p.Name).ToList();
		foreach (var p in paises)
		{
			lista.AddItem(p.Name, p.Id);
			var bandeira = Flags.CountryFlag(p.Id);
			if (bandeira != null) lista.SetItemIcon(lista.ItemCount - 1, bandeira);
		}
		int indice = lista.GetItemIndex(selecionado);
		lista.Selected = indice >= 0 ? indice : 0;
		interno = false;
	}

	void FillStates(int selecionado)
	{
		interno = true;
		var lista = GetNode<OptionButton>("%ClubState");
		lista.Clear();
		PrepareList(lista);
		var estados = Catalog.Instance.CatalogData.States;
		for (int i = 0; i < estados.Count; i++)
		{
			lista.AddItem(estados[i].Name, estados[i].Id);
			var bandeira = Flags.StateFlag(estados[i].Id);
			if (bandeira != null) lista.SetItemIcon(lista.ItemCount - 1, bandeira);
		}
		int indice = lista.GetItemIndex(selecionado);
		lista.Selected = indice >= 0 ? indice : 0;
		interno = false;
	}

	static void PrepareList(OptionButton lista)
	{
		lista.AddThemeConstantOverride("icon_max_width", 20);
		var popup = lista.GetPopup();
		popup.MaxSize = new Vector2I(420, 280);
		popup.Transparent = false;
	}

	void FillReputation(OptionButton lista, int selecionado)
	{
		interno = true;
		lista.Clear();
		for (int i = 0; i < 6; i++) lista.AddItem(Translator.T("editor.reputation." + i), i);
		lista.Selected = Mathf.Clamp(selecionado, 0, 5);
		interno = false;
	}

	void FillRange(OptionButton lista, int min, int max, int selecionado)
	{
		interno = true;
		lista.Clear();
		for (int i = min; i <= max; i++) lista.AddItem(i.ToString(), i);
		if (selecionado < min || selecionado > max) lista.AddItem(selecionado.ToString(), selecionado);
		lista.Selected = lista.GetItemIndex(selecionado);
		interno = false;
	}

	void FillPositions(OptionButton lista, int selecionado)
	{
		interno = true;
		lista.Clear();
		for (int i = 0; i < 5; i++) lista.AddItem(Translator.T("position." + i), i);
		lista.Selected = Mathf.Clamp(selecionado, 0, 4);
		interno = false;
	}

	void FillSides(int selecionado)
	{
		interno = true;
		var lista = GetNode<OptionButton>("%PlayerSide");
		lista.Clear();
		lista.AddItem(Translator.T("side.0"), 0);
		lista.AddItem(Translator.T("side.1"), 1);
		lista.Selected = selecionado == 1 ? 1 : 0;
		interno = false;
	}

	void FillTraits(int posicao, int cr1, int cr2)
	{
		interno = true;
		int ini = posicao == 0 ? 0 : 4;
		int fim = posicao == 0 ? 3 : 13;
		FillTraitRange(GetNode<OptionButton>("%PlayerTrait1"), ini, fim, cr1);
		FillTraitRange(GetNode<OptionButton>("%PlayerTrait2"), ini, fim, cr2);
		interno = false;
	}

	void FillTraitRange(OptionButton lista, int ini, int fim, int selecionado)
	{
		lista.Clear();
		for (int i = ini; i <= fim; i++) lista.AddItem(Rules.TraitName(i), i);
		int indice = lista.GetItemIndex(selecionado);
		lista.Selected = indice >= 0 ? indice : 0;
	}

	void UpdateStateField()
	{
		GetNode<Control>("%ClubStateRow").Visible = Item(GetNode<OptionButton>("%ClubCountry")) == 29;
	}

	void ApplyClubFormLabels()
	{
		GetNode<Label>("%LabelClubName").Text = Translator.T("editor.name");
		GetNode<Label>("%LabelClubCoach").Text = Translator.T("editor.coach");
		GetNode<Label>("%LabelClubStadium").Text = Translator.T("editor.stadium");
		GetNode<Label>("%LabelClubCapacity").Text = Translator.T("editor.capacity");
		GetNode<Label>("%LabelClubLevel").Text = Translator.T("editor.level");
		GetNode<Label>("%LabelClubReputation").Text = Translator.T("editor.reputation");
		GetNode<Label>("%LabelClubCountry").Text = Translator.T("editor.country");
		GetNode<Label>("%LabelClubState").Text = Translator.T("editor.state");
		GetNode<Label>("%LabelClubFile").Text = Translator.T("editor.file");
		GetNode<Label>("%LabelClubText").Text = Translator.T("editor.text_color");
		GetNode<Label>("%LabelClubBackground").Text = Translator.T("editor.background_color");
		GetNode<Button>("%ClubOk").Text = Translator.T("editor.ok");
		GetNode<Button>("%ClubCancel").Text = Translator.T("editor.cancel");
	}

	void ApplyPlayerFormLabels()
	{
		GetNode<Label>("%LabelPlayerName").Text = Translator.T("editor.name");
		GetNode<Label>("%LabelPlayerPosition").Text = Translator.T("editor.position");
		GetNode<Label>("%LabelPlayerSide").Text = Translator.T("editor.side");
		GetNode<Label>("%LabelPlayerCountry").Text = Translator.T("editor.player_country");
		GetNode<Label>("%LabelPlayerAge").Text = Translator.T("editor.age");
		GetNode<CheckBox>("%PlayerStar").Text = Translator.T("editor.star");
		GetNode<CheckBox>("%PlayerTop").Text = Translator.T("editor.world_class");
		GetNode<CheckBox>("%PlayerKeep").Text = Translator.T("editor.keep_adding");
		GetNode<Button>("%PlayerOk").Text = Translator.T("editor.ok");
		GetNode<Button>("%PlayerCancel").Text = Translator.T("editor.cancel");
		Tip("%PlayerSide", "editor.tip.side");
		Tip("%PlayerTrait1", "editor.tip.trait_1");
		Tip("%PlayerTrait2", "editor.tip.trait_2");
		Tip("%PlayerAge", "editor.tip.age");
		Tip("%PlayerStar", "editor.tip.star");
		Tip("%PlayerTop", "editor.tip.world_class");
		Tip("%PlayerKeep", "editor.tip.continue");
	}

	void ApplyStaticTips()
	{
		Tip("%Add", "editor.tip.add_club");
		Tip("%Edit", "editor.tip.edit_club");
		Tip("%Delete", "editor.tip.delete_club");
		Tip("%Home", "editor.tip.home");
		Tip("%Quit", "editor.tip.quit");
		Tip("%FirstTeam", "editor.tip.first_team");
		Tip("%Youth", "editor.tip.reserves");
		Tip("%MissingCrest", "editor.tip.no_crest");
		Tip("%MissingShirts", "editor.tip.no_shirts");
		Tip("%Duplicates", "editor.tip.duplicates");
		Tip("%ClubSearch", "editor.tip.find_club");
		Tip("%PlayerSearch", "editor.tip.find_player");
		Tip("%ClubSearchText", "editor.tip.club_field");
		Tip("%PlayerSearchText", "editor.tip.player_field");
		Tip("%AddPlayer", "editor.tip.add_player");
		Tip("%EditPlayer", "editor.tip.edit_player");
		Tip("%DeletePlayer", "editor.tip.delete_player");
		Tip("%Random", "editor.tip.random");
		Tip("%RandomPosition", "editor.tip.position");
		Tip("%AtPosition", "editor.tip.at_position");
		Tip("%CountIcon", "editor.tip.count");
		Tip("%Count", "editor.tip.count");
		Tip("%StartersIcon", "editor.tip.starters");
		Tip("%Starters", "editor.tip.starters");
		GetNode<Label>("%AtPosition").Text = Translator.T("editor.at_position");
	}

	void ApplyClubTips(ClubRecord time)
	{
		var banco = Catalog.Instance.CatalogData;
		SetTipText("%Name", Translator.T("editor.tip.club_name"));
		SetTipText("%Reputation", string.Format(Translator.T("editor.tip.reputation"), Translator.T("editor.reputation." + Mathf.Clamp(time.Reputation, 0, 5))));
		SetTipText("%Country", string.Format(Translator.T("editor.tip.country"), banco.CountryName(time.CountryId)));
		SetTipText("%CountryFlag", string.Format(Translator.T("editor.tip.country"), banco.CountryName(time.CountryId)));
		SetTipText("%State", string.Format(Translator.T("editor.tip.state"), banco.StateName(time.StateId)));
		SetTipText("%StateFlag", string.Format(Translator.T("editor.tip.state"), banco.StateName(time.StateId)));
		SetTipText("%Stadium", string.Format(Translator.T("editor.tip.stadium"), time.Stadium));
		SetTipText("%StadiumIcon", string.Format(Translator.T("editor.tip.stadium"), time.Stadium));
		SetTipText("%Capacity", string.Format(Translator.T("editor.tip.capacity"), time.Capacity));
		SetTipText("%Coach", string.Format(Translator.T("editor.tip.coach"), time.Coach));
		SetTipText("%CoachIcon", string.Format(Translator.T("editor.tip.coach"), time.Coach));
		SetTipText("%CoachFlag", string.Format(Translator.T("editor.tip.coach_country"), banco.CountryName(time.CoachCountryId)));
		SetTipText("%Level", string.Format(Translator.T("editor.tip.level"), time.Level));
		SetTipText("%Crest", string.Format(Translator.T("editor.tip.crest"), time.Name));
		SetTipText("%Shirt1", string.Format(Translator.T("editor.tip.shirt"), 1));
		SetTipText("%Shirt2", string.Format(Translator.T("editor.tip.shirt"), 2));
		SetTipText("%Shirt3", string.Format(Translator.T("editor.tip.shirt"), 3));
	}

	void Tip(string no, string chave) => SetTipText(no, Translator.T(chave));

	void SetTipText(string no, string texto)
	{
		var controle = GetNode<Control>(no);
		controle.TooltipText = texto;
		controle.MouseFilter = MouseFilterEnum.Stop;
	}

	void ShowClubError(string chave) => GetNode<Label>("%ClubError").Text = Translator.T(chave);
	void ShowPlayerError(string chave) => GetNode<Label>("%PlayerError").Text = Translator.T(chave);

	List<PlayerRecord> Squad(ClubRecord time) => principal ? time.Players : time.Youth;

	ClubRecord TimeAtual()
	{
		int i = GetNode<EditorGrid>("%ClubGrid").Selecionada;
		return i >= 0 && i < ordem.Count ? ordem[i] : null;
	}

	PlayerRecord JogadorAtual()
	{
		int i = GetNode<EditorGrid>("%PlayerGrid").Selecionada;
		return i >= 0 && i < exibidos.Count ? exibidos[i] : null;
	}

	int SelectedPosition() => Item(GetNode<OptionButton>("%PlayerPosition"));

	static int Item(OptionButton lista)
	{
		if (lista.Selected < 0) return 0;
		return lista.GetItemId(lista.Selected);
	}

	string Code(int pais) => Catalog.Instance.CatalogData.FindCountry(pais)?.Code ?? "";

	void Persist()
	{
		if (OS.GetEnvironment("BRASFOOT_FOTO_EDITOR") == "1") return;
		try { Catalog.Instance.Save(); }
		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
			Alert(Translator.T("save.error"));
		}
	}

	void SetLabel(string no, string chave) => GetNode<Button>(no).Text = Translator.T(chave);

	static Color Html(string hex)
	{
		if (string.IsNullOrWhiteSpace(hex)) return Colors.White;
		if (!hex.StartsWith('#')) hex = "#" + hex;
		return new Color(hex);
	}

	static string Hex(Color cor) => "#" + cor.ToHtml(false);

	static string SearchKey(string texto)
	{
		if (string.IsNullOrWhiteSpace(texto)) return "";
		var sb = new StringBuilder();
		foreach (var c in texto.Normalize(NormalizationForm.FormD))
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				sb.Append(char.ToLowerInvariant(c));
		return sb.ToString();
	}

	static StyleBoxFlat Funds(Color fundo, Color borda, bool sombra)
	{
		var caixa = new StyleBoxFlat { BgColor = fundo, BorderColor = borda };
		caixa.SetBorderWidthAll(1);
		caixa.SetCornerRadiusAll(3);
		caixa.SetContentMarginAll(6);
		if (sombra)
		{
			caixa.ShadowColor = new Color(0, 0, 0, 0.16f);
			caixa.ShadowSize = 2;
			caixa.ShadowOffset = new Vector2(0, 1);
		}
		return caixa;
	}

	static void Brighten(Button botao)
	{
		botao.AddThemeStyleboxOverride("normal", Funds(new Color("f4f4f4"), new Color("9a9a9a"), true));
		botao.AddThemeStyleboxOverride("hover", Funds(Colors.White, new Color("4f4f4f"), true));
		botao.AddThemeStyleboxOverride("pressed", Funds(new Color("dcdcdc"), new Color("6a6a6a"), false));
		botao.AddThemeStyleboxOverride("focus", Funds(new Color("f7f7f7"), new Color("3d7ec9"), true));
		botao.AddThemeColorOverride("font_color", Colors.Black);
		botao.AddThemeColorOverride("font_hover_color", Colors.Black);
		botao.AddThemeColorOverride("font_pressed_color", Colors.Black);
		botao.AddThemeColorOverride("font_focus_color", Colors.Black);
		botao.AddThemeFontSizeOverride("font_size", 13);
	}

	static void PaintField(LineEdit campo)
	{
		campo.AddThemeStyleboxOverride("normal", Funds(Colors.White, new Color("666666"), false));
		campo.AddThemeStyleboxOverride("focus", Funds(Colors.White, new Color("3d7ec9"), false));
		campo.AddThemeColorOverride("font_color", Colors.Black);
		campo.AddThemeFontSizeOverride("font_size", 14);
	}
}
