using Godot;

namespace Brasfoot;

public partial class Badge : Control
{
	public override void _Ready() => Resized += QueueRedraw;

	public override void _Draw()
	{
		var c = Size / 2f;
		float raio = Mathf.Min(Size.X, Size.Y) * 0.38f;
		var tinta = new Color(0.93f, 0.95f, 0.92f);
		var linha = new Color(0.1f, 0.28f, 0.14f);
		DrawCircle(c, raio, tinta);
		DrawArc(c, raio, 0, Mathf.Tau, 48, linha, 3f, true);
		DrawArc(c, raio * 0.42f, 0, Mathf.Tau, 32, linha, 2f, true);
		DrawLine(c + new Vector2(-raio, 0), c + new Vector2(raio, 0), linha, 2f, true);
	}
}
