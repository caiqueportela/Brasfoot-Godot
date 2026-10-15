using Godot;

namespace Brasfoot;

public partial class DismissalScreen : ScreenBase
{
	List<Club> opcoes = new();

	public override void _Ready()
	{
		base._Ready();
		var antigo = Session.Instance.SackedClub;
		var est = Session.Instance.Current;
		if (antigo == null || est == null)
		{
			Session.Instance.GoTo(Paths.Menu);
			return;
		}
		GetNode<Label>("%Message").Text = string.Format(Translator.T("sacking.text"), antigo.Name);
		var lista = GetNode<ItemList>("%List");
		opcoes = est.ClubsInCompetition.Where(c => c.CountryId == antigo.CountryId && !c.IsHuman && c.Division > 0)
			.OrderBy(c => c.Division).ThenByDescending(c => c.Level).Take(40).ToList();
		foreach (var c in opcoes)
		{
			var tex = Crests.Get(c.Slug, true);
			string rotulo = c.Division + "  " + c.Name;
			if (tex != null) lista.AddItem(rotulo, tex);
			else lista.AddItem(rotulo);
		}
		GetNode<Button>("%TakeJob").Pressed += () => TakeJob(antigo);
	}

	protected override void ApplyTexts()
	{
		GetNode<Label>("%Title").Text = Translator.T("sacking.title");
		GetNode<Button>("%TakeJob").Text = Translator.T("sacking.take_job");
	}

	void TakeJob(Club antigo)
	{
		var sel = GetNode<ItemList>("%List").GetSelectedItems();
		if (sel.Length == 0) return;
		var novo = opcoes[sel[0]];
		var est = Session.Instance.Current;
		antigo.IsHuman = false;
		novo.IsHuman = true;
		novo.Coach = antigo.Coach;
		novo.Confidence = 60;
		est.HumanClubIds.Remove(antigo.Slug);
		est.HumanClubIds.Insert(0, novo.Slug);
		Session.Instance.GoTo(Paths.Office);
	}
}
