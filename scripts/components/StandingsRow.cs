using Godot;

namespace Brasfoot;

public partial class StandingsRow : HBoxContainer
{
	public void Fill(int pos, TableRow linha)
	{
		GetNode<Label>("Position").Text = pos.ToString();
		GetNode<Label>("Name").Text = linha.Name;
		GetNode<Label>("Played").Text = linha.Played.ToString();
		GetNode<Label>("Wins").Text = linha.Wins.ToString();
		GetNode<Label>("Draws").Text = linha.Draws.ToString();
		GetNode<Label>("Losses").Text = linha.Losses.ToString();
		GetNode<Label>("For").Text = linha.GoalsFor.ToString();
		GetNode<Label>("Against").Text = linha.GoalsAgainst.ToString();
		GetNode<Label>("Diff").Text = linha.GoalDifference.ToString();
		GetNode<Label>("Points").Text = linha.Points.ToString();
	}

	public void Paint(int tipo)
	{
		Modulate = tipo switch
		{
			1 => new Color(0.62f, 0.82f, 1f),
			2 => new Color(1f, 0.68f, 0.68f),
			3 => new Color(0.98f, 0.88f, 0.48f),
			_ => Colors.White
		};
	}
}
