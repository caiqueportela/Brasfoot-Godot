using Godot;

namespace Brasfoot;

public partial class Pitch : Control
{
	public override void _Ready() => Resized += QueueRedraw;

	public void Show(string casa, string fora, string placar, string relogio)
	{
		GetNode<Label>("HomeName").Text = casa;
		GetNode<Label>("AwayName").Text = fora;
		GetNode<Label>("Score").Text = placar;
		var rel = GetNode<Label>("Clock");
		rel.Text = relogio;
		rel.Visible = !string.IsNullOrEmpty(relogio);
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), new Color(0.11f, 0.42f, 0.2f));
		float faixa = Size.Y / 8f;
		for (int i = 0; i < 8; i += 2)
			DrawRect(new Rect2(0, i * faixa, Size.X, faixa), new Color(0.09f, 0.36f, 0.17f));
		var w = new Color(1, 1, 1, 0.88f);
		var area = new Rect2(8, 8, Size.X - 16, Size.Y - 16);
		DrawRect(area, w, false, 2);
		DrawLine(new Vector2(Size.X / 2, 8), new Vector2(Size.X / 2, Size.Y - 8), w, 2);
		DrawArc(Size / 2, Mathf.Min(Size.X, Size.Y) * 0.14f, 0, Mathf.Tau, 32, w, 2, true);
		float bh = Size.Y * 0.46f;
		float bw = Size.X * 0.14f;
		DrawRect(new Rect2(8, (Size.Y - bh) / 2, bw, bh), w, false, 2);
		DrawRect(new Rect2(Size.X - 8 - bw, (Size.Y - bh) / 2, bw, bh), w, false, 2);
	}
}
