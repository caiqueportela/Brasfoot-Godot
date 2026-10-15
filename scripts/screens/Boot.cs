using Godot;

namespace Brasfoot;

public partial class Boot : Node
{
	public override void _Ready()
	{
		var janela = GetWindow();
		if (janela != null) janela.Title = "Brasfoot Godot";
		if (Catalog.Instance.CatalogData == null)
		{
			GD.PrintErr("Missing catalog at res://catalog/brasfoot.res");
			GetTree().Quit();
			return;
		}
		if (OS.GetEnvironment("BRASFOOT_SMOKE") == "1")
		{
			Smoke();
			GetTree().Quit();
			return;
		}
		GetTree().CallDeferred("change_scene_to_file", Paths.Menu);
	}

	static void Smoke()
	{
		var banco = Catalog.Instance.CatalogData;
		var est = SeasonBuilder.CreateCareer(banco, new[] { "flarj" }, new[] { "Coach" }, 2026, 22, true, true, true);
		var fla = est.FindClub("flarj");
		var prox = est.Competitions.SelectMany(c => c.Matches).First(p => p.HomeSlug == "flarj" || p.AwaySlug == "flarj");
		MatchEngine.Resolve(est, prox, true);
		CareerFile.Save(est, "user://smoke.res");
		var est2 = CareerFile.Open("user://smoke.res", banco);
		var fla2 = est2.FindClub("flarj");
		var prox2 = est2.Competitions.SelectMany(c => c.Matches).First(p => p.Id == prox.Id);
		bool ok = fla2 != null
			&& fla2.Funds == fla.Funds
			&& fla2.Squad.Count == fla.Squad.Count
			&& fla2.Squad[0].Name == fla.Squad[0].Name
			&& fla2.Squad[0].Strength == fla.Squad[0].Strength
			&& est2.Competitions.Count == est.Competitions.Count
			&& est2.Competitions.Sum(c => c.Matches.Count) == est.Competitions.Sum(c => c.Matches.Count)
			&& prox2.HomeGoals == prox.HomeGoals
			&& prox2.AwayGoals == prox.AwayGoals
			&& prox2.Played;
		GD.Print($"SMOKE div={fla.Division} forca={fla.AverageStrength} jogo={prox.HomeGoals}x{prox.AwayGoals} eventos={prox.Events.Count} comps={est.Competitions.Count} partidas={est.Competitions.Sum(c => c.Matches.Count)} reload={(ok ? "OK" : "FAIL")} caixa={fla2?.Funds} nomes={banco.Names.Count}");
		if (!ok) GD.PrintErr("SMOKE_FAIL");
	}
}
