using Godot;

namespace Brasfoot;

public partial class Session : Node
{
	public static Session Instance { get; private set; }
	public CareerState Current { get; set; }
	public Queue<Match> MatchQueue { get; } = new();
	public Match CurrentMatch { get; set; }
	public List<string> SeasonMessages { get; set; } = new();
	public Club SackedClub { get; set; }
	public int TabIndex { get; set; }
	public int CompetitionIndex { get; set; }
	public Player MarketPlayer { get; set; }
	public Club MarketClub { get; set; }
	public bool OfficeCaptured { get; set; }
	public bool MatchCaptured { get; set; }

	AudioStreamPlayer alto;
	readonly Dictionary<string, AudioStreamWav> sons = new();

	public override void _EnterTree() => Instance = this;

	public override void _Ready() => alto = GetNode<AudioStreamPlayer>("Speaker");

	public void GoTo(string cena) => GetTree().ChangeSceneToFile(cena);

	public void Abandon()
	{
		Current = null;
		MatchQueue.Clear();
		CurrentMatch = null;
		GoTo(Paths.Menu);
	}

	public void StartCareer(IReadOnlyList<string> times, IReadOnlyList<string> tecnicos, int ano, bool estadual, bool copa, bool continental, int semente)
	{
		TabIndex = 0;
		CompetitionIndex = 0;
		MatchQueue.Clear();
		Current = SeasonBuilder.CreateCareer(Catalog.Instance.CatalogData, times, tecnicos, ano, semente, estadual, copa, continental);
		GoTo(Paths.Office);
	}

	public bool Continue()
	{
		var est = Current;
		var alvo = MatchEngine.NextHumanDate(est);
		if (alvo == null)
		{
			MatchEngine.SimulateUntil(est, "9999-12-31", false);
			SeasonMessages = MatchEngine.FinishSeason(est);
			GoTo(Paths.SeasonEnd);
			return false;
		}
		MatchEngine.SimulateUntil(est, alvo, true);
		NegotiateSales();
		MatchQueue.Clear();
		foreach (var p in est.Competitions.SelectMany(c => c.Matches)
			.Where(p => !p.Played && p.MatchDate == alvo && p.HomeSlug != "?" && (est.IsHumanClub(p.HomeSlug) || est.IsHumanClub(p.AwaySlug)))
			.OrderBy(p => p.Id))
			MatchQueue.Enqueue(p);
		if (MatchQueue.Count == 0)
		{
			if (MatchEngine.SeasonOver(est))
			{
				SeasonMessages = MatchEngine.FinishSeason(est);
				GoTo(Paths.SeasonEnd);
				return false;
			}
			return true;
		}
		PrepareMatch(MatchQueue.Dequeue());
		GoTo(Paths.Match);
		return false;
	}

	public void PrepareMatch(Match p)
	{
		MatchEngine.Resolve(Current, p, true);
		CurrentMatch = p;
	}

	public string SceneAfterMatch()
	{
		var est = Current;
		var p = CurrentMatch;
		var humano = est.HumanClubs().FirstOrDefault(c => c.Slug == p.HomeSlug || c.Slug == p.AwaySlug);
		if (humano != null && humano.Confidence <= 8)
		{
			SackedClub = humano;
			return Paths.Dismissal;
		}
		if (MatchQueue.Count > 0)
		{
			PrepareMatch(MatchQueue.Dequeue());
			return Paths.Match;
		}
		if (MatchEngine.SeasonOver(est) && MatchEngine.NextHumanDate(est) == null)
		{
			SeasonMessages = MatchEngine.FinishSeason(est);
			return Paths.SeasonEnd;
		}
		return Paths.Office;
	}

	public void PlaySound(string arquivo)
	{
		if (Current != null && !Current.SoundEnabled) return;
		if (alto == null) return;
		if (!sons.TryGetValue(arquivo, out var fluxo))
		{
			fluxo = AudioWav.Read(Paths.ArtFile("sounds/" + arquivo));
			if (fluxo == null) return;
			sons[arquivo] = fluxo;
		}
		alto.Stream = fluxo;
		alto.Play();
	}

	void NegotiateSales()
	{
		var est = Current;
		var rng = new Random(est.Seed + est.Today.GetHashCode());
		foreach (var c in est.ClubsInCompetition.Where(c => c.IsHuman).ToList())
		{
			foreach (var j in c.Squad.Where(j => j.ListedForSale).ToList())
			{
				var comprador = est.ClubsInCompetition.Where(o => o.Slug != c.Slug && o.Funds > j.Price && o.Division > 0)
					.OrderBy(_ => rng.Next()).FirstOrDefault();
				if (comprador == null || rng.NextDouble() > 0.55) continue;
				int oferta = (int)(j.Price * (0.85 + rng.NextDouble() * 0.25));
				if (comprador.Funds < oferta) continue;
				c.Squad.Remove(j);
				comprador.Squad.Add(j);
				c.Funds += oferta;
				comprador.Funds -= oferta;
				j.ListedForSale = false;
				est.History.Add("sale.done|" + j.Name + "|" + comprador.Name + "|" + Rules.Money(oferta));
			}
		}
	}
}
