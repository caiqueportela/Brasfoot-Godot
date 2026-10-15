using Godot;

namespace Brasfoot;

public partial class SeasonEndScreen : ScreenBase
{
	PackedScene linha;

	public override void _Ready()
	{
		base._Ready();
		linha = GD.Load<PackedScene>("res://scenes/components/TextRow.tscn");
		var lista = GetNode<VBoxContainer>("%List");
		Nodes.Clear(lista);
		var msgs = Session.Instance.SeasonMessages ?? new List<string>();
		if (msgs.Count == 0)
		{
			var vazio = linha.Instantiate<Label>();
			vazio.Text = Translator.T("season_end.empty");
			lista.AddChild(vazio);
		}
		foreach (var m in msgs.Take(24))
		{
			var item = linha.Instantiate<Label>();
			item.Text = Translator.F(m);
			lista.AddChild(item);
		}
		GetNode<Button>("%ContinueSeason").Pressed += () => Session.Instance.GoTo(Paths.Office);
	}

	protected override void ApplyTexts()
	{
		GetNode<Label>("%Title").Text = Translator.T("season_end.title");
		int ano = Session.Instance.Current?.Year ?? 0;
		GetNode<Button>("%ContinueSeason").Text = string.Format(Translator.T("season_end.continue_season"), ano);
	}
}
