using Godot;

namespace Brasfoot;

public static class Nodes
{
	public static void Clear(Node n)
	{
		while (n.GetChildCount() > 0)
			n.GetChild(0).Free();
	}

	public static T Criar<T>(PackedScene cena) where T : Node => cena.Instantiate<T>();
}
