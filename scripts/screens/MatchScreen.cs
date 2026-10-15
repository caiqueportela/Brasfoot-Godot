using Godot;

namespace Brasfoot;

public partial class MatchScreen : ScreenBase
{
	Godot.Timer relogio;
	int minuto;
	bool jogando;
	bool terminou;
	string somMinuto = "";

	public override void _Ready()
	{
		if (Session.Instance.CurrentMatch == null || Session.Instance.Current == null)
		{
			Session.Instance.GoTo(Paths.Office);
			return;
		}
		jogando = true;
		base._Ready();
		relogio = GetNode<Godot.Timer>("%ClockNode");
		relogio.WaitTime = Session.Instance.Current.Speed switch { 1 => 0.42, 3 => 0.05, _ => 0.16 };
		relogio.Timeout += Advance;
		GetNode<Button>("%Pause").Pressed += TogglePause;
		GetNode<Button>("%Skip").Pressed += () => { minuto = 95; Finish(); };
		jogando = true;
		Reveal();
		relogio.Start();
		if (OS.GetEnvironment("BRASFOOT_FOTO") == "1" && !Session.Instance.MatchCaptured)
		{
			Session.Instance.MatchCaptured = true;
			GetTree().CreateTimer(1.2).Timeout += () =>
			{
				Screenshot.Save(this, "verificacao_partida.png");
				GetTree().Quit();
			};
		}
	}

	protected override void ApplyTexts()
	{
		GetNode<Button>("%Pause").Text = jogando ? Translator.T("match.pause") : Translator.T("match.resume");
		GetNode<Button>("%Skip").Text = Translator.T("match.skip");
		var p = Session.Instance.CurrentMatch;
		if (p == null) return;
		var comp = Session.Instance.Current.Competitions.FirstOrDefault(c => c.Id == p.CompetitionId);
		GetNode<Label>("%Competition").Text = (comp?.Name ?? "") + "   " + Rules.ShortDate(p.MatchDate);
	}

	void TogglePause()
	{
		if (terminou) return;
		jogando = !jogando;
		if (jogando) relogio.Start();
		else relogio.Stop();
		GetNode<Button>("%Pause").Text = jogando ? Translator.T("match.pause") : Translator.T("match.resume");
	}

	void Advance()
	{
		if (!jogando || terminou) return;
		minuto += Session.Instance.Current.Speed == 3 ? 3 : 1;
		Reveal();
		if (minuto >= 90) Finish();
	}

	void Reveal()
	{
		var p = Session.Instance.CurrentMatch;
		var est = Session.Instance.Current;
		int gc = 0;
		int gf = 0;
		foreach (var e in p.Events)
		{
			if (e.Kind != "goal" || e.Minute > minuto) continue;
			if (e.ClubSlug == p.HomeSlug) gc++;
			else gf++;
		}
		var casa = est.FindClub(p.HomeSlug);
		var fora = est.FindClub(p.AwaySlug);
		GetNode<Label>("%Score").Text = gc + "  x  " + gf;
		GetNode<Label>("%HomeName").Text = casa?.Name ?? p.HomeSlug;
		GetNode<Label>("%AwayName").Text = fora?.Name ?? p.AwaySlug;
		GetNode<TextureRect>("%HomeCrest").Texture = Crests.Get(p.HomeSlug);
		GetNode<TextureRect>("%AwayCrest").Texture = Crests.Get(p.AwaySlug);
		string rel = minuto >= 90 ? Translator.T("event.full_time") : minuto == 45 ? Translator.T("event.halftime") : minuto + "'";
		GetNode<Label>("%Clock").Text = rel;
		GetNode<ProgressBar>("%Possession").Value = p.HomePossession;
		GetNode<Pitch>("%Pitch").Show(casa?.Name ?? p.HomeSlug, fora?.Name ?? p.AwaySlug, gc + "  x  " + gf, "");
		var linhas = new List<string> { Translator.T("match.kickoff") };
		linhas.AddRange(p.Events.Where(e => e.Minute <= minuto).Select(e => Translator.Narrate(e, est)));
		GetNode<RichTextLabel>("%Commentary").Text = string.Join("\n", linhas);
		var ultimo = p.Events.LastOrDefault(e => e.Minute == minuto || (est.Speed == 3 && e.Minute <= minuto && e.Minute > minuto - 3));
		if (ultimo != null)
		{
			string chave = ultimo.Minute + ultimo.Kind;
			if (chave != somMinuto)
			{
				somMinuto = chave;
				if (ultimo.Kind == "goal" && ultimo.Extra == 2) Session.Instance.PlaySound("penalty.wav");
				else if (ultimo.Kind == "goal") Session.Instance.PlaySound("goal.wav");
				else if (ultimo.Kind == "red") Session.Instance.PlaySound("red_card.wav");
				else if (ultimo.Kind == "injury") Session.Instance.PlaySound("injury.wav");
				else if (ultimo.Kind == "halftime") Session.Instance.PlaySound("halftime.wav");
			}
		}
	}

	void Finish()
	{
		if (terminou) return;
		terminou = true;
		jogando = false;
		relogio.Stop();
		minuto = 99;
		Reveal();
		var p = Session.Instance.CurrentMatch;
		GetNode<Label>("%Score").Text = p.HomeGoals + "  x  " + p.AwayGoals;
		Session.Instance.PlaySound("full_time.wav");
		if (OS.GetEnvironment("BRASFOOT_FOTO") == "1") return;
		string proxima = Session.Instance.SceneAfterMatch();
		string texto = proxima switch
		{
			_ when proxima == Paths.Match => Translator.T("match.another"),
			_ when proxima == Paths.SeasonEnd => Translator.T("event.full_time"),
			_ when proxima == Paths.Dismissal => string.Format(Translator.T("sacking.notice"), Session.Instance.SackedClub?.Coach, Session.Instance.SackedClub?.Name),
			_ => string.Format(Translator.T("match.full_time"), p.HomeGoals, p.AwayGoals)
		};
		Alert(texto, () => Session.Instance.GoTo(proxima));
	}
}
