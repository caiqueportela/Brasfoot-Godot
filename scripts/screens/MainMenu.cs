using Godot;

namespace Brasfoot;

public partial class MainMenu : ScreenBase
{
	public override void _Ready()
	{
		base._Ready();
		PrepareButtons();
		Resized += Fit;
		Fit();
		GetNode<Button>("%NewGame").Pressed += () => Session.Instance.GoTo(Paths.NewGame);
		GetNode<Button>("%Load").Pressed += () => Session.Instance.GoTo(Paths.Load);
		GetNode<Button>("%Editor").Pressed += () => Session.Instance.GoTo(Paths.Editor);
		GetNode<Button>("%Config").Pressed += () => Session.Instance.GoTo(Paths.Settings);
		GetNode<Button>("%Quit").Pressed += () => GetTree().Quit();
		if (OS.GetEnvironment("BRASFOOT_FOTO_EDITOR") == "1")
		{
			GetTree().CreateTimer(0.4).Timeout += () => Session.Instance.GoTo(Paths.Editor);
			return;
		}
		if (OS.GetEnvironment("BRASFOOT_FOTO_CFG") == "1")
		{
			GetTree().CreateTimer(0.4).Timeout += () => Session.Instance.GoTo(Paths.Settings);
			return;
		}
		if (OS.GetEnvironment("BRASFOOT_FOTO") == "1")
		{
			GetTree().CreateTimer(0.8).Timeout += () =>
			{
				Screenshot.Save(this, "verificacao_menu.png");
				Session.Instance.StartCareer(new[] { "flarj" }, new[] { "Carlos" }, 2026, true, true, true, 22);
			};
		}
	}

	protected override void ApplyTexts()
	{
		SetCaption("%NewGame", "menu.new_game", "menu.tip.new_game");
		SetCaption("%Load", "menu.load", "menu.tip.load");
		SetCaption("%Editor", "menu.editor", "menu.tip.editor");
		SetCaption("%Config", "menu.settings", "menu.tip.settings");
		SetCaption("%Quit", "menu.quit", "menu.tip.quit");
		GetNode<Label>("%Year").Text = Translator.T("menu.year");
	}

	void SetCaption(string botao, string texto, string dica)
	{
		var controle = GetNode<Button>(botao);
		controle.Text = "";
		controle.GetNode<Label>("Caption").Text = Translator.T(texto);
		controle.TooltipText = Translator.T(dica);
	}

	static readonly string[] Acoes = { "%NewGame", "%Load", "%Editor", "%Config", "%Quit" };

	readonly Dictionary<string, Tween> realces = new();
	Texture2D brilhoTextura;

	void PrepareButtons()
	{
		brilhoTextura = GD.Load<Texture2D>("res://art/menu/brilho.png");
		var vazio = new StyleBoxEmpty();
		foreach (var nome in Acoes)
		{
			var botao = GetNode<Button>(nome);
			foreach (var estado in new[] { "normal", "hover", "pressed", "focus", "disabled" })
				botao.AddThemeStyleboxOverride(estado, vazio);
			botao.MouseDefaultCursorShape = CursorShape.PointingHand;
			var icone = botao.GetNode<TextureRect>("Icon");
			icone.TextureFilter = CanvasItem.TextureFilterEnum.LinearWithMipmaps;
			icone.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			var brilho = new TextureRect
			{
				Name = "Glow",
				MouseFilter = MouseFilterEnum.Ignore,
				Texture = brilhoTextura,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				TextureFilter = CanvasItem.TextureFilterEnum.Linear,
				Modulate = new Color(1, 0.94f, 0.62f, 0)
			};
			botao.AddChild(brilho);
			botao.MoveChild(brilho, icone.GetIndex());
			bool segurando = false;
			void Refresh() => ApplyHighlight(botao, botao.IsHovered() || botao.HasFocus(), segurando);
			botao.MouseEntered += Refresh;
			botao.MouseExited += Refresh;
			botao.FocusEntered += Refresh;
			botao.FocusExited += Refresh;
			botao.ButtonDown += () => { segurando = true; Refresh(); };
			botao.ButtonUp += () => { segurando = false; Refresh(); };
		}
	}

	void ApplyHighlight(Button botao, bool aceso, bool pressionado)
	{
		var icone = botao.GetNode<TextureRect>("Icon");
		var brilho = botao.GetNode<TextureRect>("Glow");
		var rotulo = botao.GetNode<Label>("Caption");
		float escala = pressionado ? 0.94f : aceso ? 1.14f : 1f;
		var corIcone = pressionado ? new Color(0.78f, 0.78f, 0.78f) : Colors.White;
		var corTexto = pressionado ? new Color(0.82f, 0.82f, 0.82f) : aceso ? new Color(1f, 0.93f, 0.55f) : Colors.White;
		var corBrilho = new Color(1f, 0.95f, 0.7f, aceso ? 1f : 0f);
		if (realces.TryGetValue(botao.Name, out var anterior))
			anterior.Kill();
		var tween = botao.CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		realces[botao.Name] = tween;
		tween.TweenProperty(icone, "scale", new Vector2(escala, escala), 0.12);
		tween.TweenProperty(icone, "modulate", corIcone, 0.12);
		tween.TweenProperty(brilho, "modulate", corBrilho, 0.12);
		tween.TweenProperty(rotulo, "modulate", corTexto, 0.12);
	}

	void Fit()
	{
		var palco = GetNode<Control>("%Stage");
		float s = Mathf.Min(Size.X / 532f, Size.Y / 300f);
		var tamanho = new Vector2(Mathf.Round(532f * s), Mathf.Round(300f * s));
		palco.Scale = Vector2.One;
		palco.Position = new Vector2(Mathf.Round((Size.X - tamanho.X) / 2f), Mathf.Round((Size.Y - tamanho.Y) / 2f));
		palco.Size = tamanho;
		var arte = palco.GetNode<TextureRect>("Backdrop");
		arte.TextureFilter = CanvasItem.TextureFilterEnum.Linear;
		var ano = GetNode<Label>("%Year");
		PlaceRect(ano, new Rect2(36, 58, 110, 28), s);
		ano.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(15f * s));

		const float icon = 56f;
		const float gap = 8f;
		const float labelH = 22f;
		const float colW = 150f;
		const float colGap = 22f;
		float block = icon + gap + labelH;
		float rowW = colW * 3f + colGap * 2f;
		float x0 = (532f - rowW) / 2f;
		float y0 = 104f;
		var colunas = new[] { "%NewGame", "%Load", "%Editor" };
		for (int i = 0; i < colunas.Length; i++)
			PlaceAction(colunas[i], x0 + i * (colW + colGap), y0, colW, icon, gap, labelH, s, true);
		PlaceAction("%Config", x0, y0 + block + 14f, colW, icon, gap, labelH, s, true);

		const float sairW = 104f;
		const float sairH = 26f;
		const float sairIcon = 16f;
		float sairX = 532f - 22f - sairW;
		float sairY = 268f;
		PlaceAction("%Quit", sairX, sairY, sairW, sairIcon, 8f, sairH, s, false);
	}

	void PlaceAction(string nome, float x, float y, float largura, float icon, float gap, float labelH, float escala, bool empilhado)
	{
		var botao = GetNode<Button>(nome);
		var icone = botao.GetNode<TextureRect>("Icon");
		var rotulo = botao.GetNode<Label>("Caption");
		var brilho = botao.GetNode<Control>("Glow");
		if (empilhado)
		{
			PlaceRect(botao, new Rect2(x, y, largura, icon + gap + labelH), escala);
			float iconX = (largura - icon) / 2f;
			PlaceRect(icone, new Rect2(iconX, 0, icon, icon), escala);
			PlaceRect(rotulo, new Rect2(0, icon + gap, largura, labelH), escala);
			rotulo.HorizontalAlignment = HorizontalAlignment.Center;
		}
		else
		{
			PlaceRect(botao, new Rect2(x, y, largura, labelH), escala);
			float iconY = (labelH - icon) / 2f;
			PlaceRect(icone, new Rect2(0, iconY, icon, icon), escala);
			PlaceRect(rotulo, new Rect2(icon + gap, 0, largura - icon - gap, labelH), escala);
			rotulo.HorizontalAlignment = HorizontalAlignment.Left;
		}
		rotulo.VerticalAlignment = VerticalAlignment.Center;
		icone.PivotOffset = icone.Size / 2f;
		var margem = new Rect2(icone.Position / escala - new Vector2(icon, icon) * 0.42f, new Vector2(icon, icon) * 1.84f);
		PlaceRect(brilho, margem, escala);
		rotulo.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(15f * escala));
	}

	static void PlaceRect(Control controle, Rect2 retangulo, float escala)
	{
		controle.AnchorLeft = controle.AnchorTop = controle.AnchorRight = controle.AnchorBottom = 0;
		controle.Position = new Vector2(Mathf.Round(retangulo.Position.X * escala), Mathf.Round(retangulo.Position.Y * escala));
		controle.Size = new Vector2(Mathf.Round(retangulo.Size.X * escala), Mathf.Round(retangulo.Size.Y * escala));
	}
}
