using Godot;

namespace Brasfoot;

public partial class NewGameScreen : ScreenBase
{
	readonly List<(string refe, string tecnico)> escolhidos = new();
	List<ClubRecord> visiveis = new();
	bool interno;

	public override void _Ready()
	{
		base._Ready();
		GetNode<OptionButton>("%Country").ItemSelected += _ => { if (!interno) RefreshDivisions(); };
		GetNode<OptionButton>("%Division").ItemSelected += _ => { if (!interno) RefreshClubs(); };
		GetNode<ItemList>("%Clubs").ItemSelected += _ => RefreshClubCard();
		GetNode<Button>("%Add").Pressed += AddSelected;
		GetNode<Button>("%Start").Pressed += StartCareer;
		GetNode<Button>("%Back").Pressed += () => Session.Instance.GoTo(Paths.Menu);
		FillCountries();
	}

	protected override void ApplyTexts()
	{
		GetNode<Label>("%Title").Text = Translator.T("new_game.title");
		GetNode<Label>("%LabelCountry").Text = Translator.T("new_game.country");
		GetNode<Label>("%LabelDivision").Text = Translator.T("new_game.division");
		GetNode<Label>("%LabelCoach").Text = Translator.T("new_game.coach");
		GetNode<CheckBox>("%StateCup").Text = Translator.T("new_game.state_cup");
		GetNode<CheckBox>("%Cup").Text = Translator.T("new_game.cup");
		GetNode<CheckBox>("%Continental").Text = Translator.T("new_game.continental");
		GetNode<Button>("%Add").Text = Translator.T("new_game.add");
		GetNode<Button>("%Start").Text = Translator.T("new_game.start");
		GetNode<Button>("%Back").Text = Translator.T("new_game.back");
		RefreshHumans();
	}

	void FillCountries()
	{
		var paises = Catalog.Instance.CatalogData.Countries.OrderBy(p => p.Name).ToList();
		var lista = GetNode<OptionButton>("%Country");
		interno = true;
		lista.Clear();
		int selecionar = 0;
		for (int i = 0; i < paises.Count; i++)
		{
			lista.AddItem(paises[i].Name, paises[i].Id);
			if (paises[i].Id == 29) selecionar = i;
		}
		lista.Selected = selecionar;
		interno = false;
		RefreshDivisions();
	}

	int SelectedCountry() => GetNode<OptionButton>("%Country").GetItemId(GetNode<OptionButton>("%Country").Selected);

	void RefreshDivisions()
	{
		var previa = SeasonBuilder.Preview(Catalog.Instance.CatalogData, SelectedCountry());
		var lista = GetNode<OptionButton>("%Division");
		interno = true;
		lista.Clear();
		foreach (var f in previa) lista.AddItem(f.Name, f.Division);
		if (lista.ItemCount > 0) lista.Selected = 0;
		interno = false;
		RefreshClubs();
	}

	void RefreshClubs()
	{
		var lista = GetNode<ItemList>("%Clubs");
		lista.Clear();
		visiveis = new List<ClubRecord>();
		var div = GetNode<OptionButton>("%Division");
		if (div.ItemCount == 0)
		{
			RefreshClubCard();
			return;
		}
		int divisao = div.GetItemId(div.Selected);
		var previa = SeasonBuilder.Preview(Catalog.Instance.CatalogData, SelectedCountry());
		var fatia = previa.FirstOrDefault(f => f.Division == divisao);
		if (fatia.Clubs == null) return;
		foreach (var t in fatia.Clubs)
		{
			visiveis.Add(t);
			var tex = Crests.Get(t.Slug, true);
			if (tex != null) lista.AddItem(t.Name, tex);
			else lista.AddItem(t.Name);
		}
		if (lista.ItemCount > 0) lista.Select(0);
		RefreshClubCard();
	}

	ClubRecord TimeAtual()
	{
		var lista = GetNode<ItemList>("%Clubs");
		var sel = lista.GetSelectedItems();
		if (sel.Length == 0 || sel[0] >= visiveis.Count) return null;
		return visiveis[sel[0]];
	}

	void RefreshClubCard()
	{
		var t = TimeAtual();
		GetNode<TextureRect>("%Crest").Texture = t == null ? null : Crests.Get(t.Slug);
		GetNode<Label>("%MatchClub").Text = t?.Name ?? "";
		GetNode<Label>("%Stadium").Text = t == null ? "" : t.Stadium + " · " + t.Capacity.ToString("N0");
		if (t != null && string.IsNullOrWhiteSpace(GetNode<LineEdit>("%Coach").Text))
			GetNode<LineEdit>("%Coach").Text = t.Coach;
	}

	void AddSelected()
	{
		var t = TimeAtual();
		if (t == null) return;
		if (escolhidos.Any(e => e.refe == t.Slug)) return;
		if (escolhidos.Count >= 4)
		{
			Alert(Translator.T("new_game.limit"));
			return;
		}
		string tecnico = GetNode<LineEdit>("%Coach").Text.Trim();
		if (tecnico.Length == 0) tecnico = t.Coach;
		escolhidos.Add((t.Slug, tecnico));
		GetNode<LineEdit>("%Coach").Text = "";
		RefreshHumans();
	}

	void RefreshHumans()
	{
		var nomes = escolhidos.Select(e => Catalog.Instance.CatalogData.Clubs.FirstOrDefault(t => t.Slug == e.refe)?.Name ?? e.refe);
		string lista = escolhidos.Count == 0 ? Translator.T("new_game.choose") : string.Join(" / ", nomes);
		GetNode<Label>("%Humans").Text = Translator.T("new_game.humans") + " " + lista;
	}

	void StartCareer()
	{
		if (escolhidos.Count == 0) AddSelected();
		if (escolhidos.Count == 0)
		{
			Alert(Translator.T("new_game.choose"));
			return;
		}
		int semente = (int)(Godot.Time.GetUnixTimeFromSystem() % 100000) + 1;
		if (semente < 0) semente = 22;
		Session.Instance.StartCareer(
			escolhidos.Select(e => e.refe).ToArray(),
			escolhidos.Select(e => e.tecnico).ToArray(),
			2026,
			GetNode<CheckBox>("%StateCup").ButtonPressed,
			GetNode<CheckBox>("%Cup").ButtonPressed,
			GetNode<CheckBox>("%Continental").ButtonPressed,
			semente);
	}
}
