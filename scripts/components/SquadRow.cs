using Godot;

namespace Brasfoot;

public partial class SquadRow : HBoxContainer
{
	public void Fill(Player j)
	{
		GetNode<Label>("Name").Text = j.Name;
		GetNode<Label>("Position").Text = Rules.Position(j.Position) + " " + (j.Side == 1 ? Translator.T("side.1") : Translator.T("side.0"));
		GetNode<Label>("Age").Text = j.Age.ToString();
		GetNode<Label>("Trait").Text = j.TraitSummary;
		GetNode<ProgressBar>("Strength").Value = j.Strength;
		GetNode<ProgressBar>("Fitness").Value = j.Fitness;
		GetNode<Label>("Goals").Text = j.Goals.ToString();
		GetNode<Label>("Wage").Text = Rules.Money(j.Wage);
		Modulate = j.IsAvailable ? Colors.White : new Color(0.72f, 0.72f, 0.72f);
	}
}
