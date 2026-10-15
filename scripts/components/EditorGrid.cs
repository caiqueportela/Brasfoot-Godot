using Godot;

namespace Brasfoot;

public partial class EditorGrid : Control
{
	public class GridCell
	{
		public string Text = "";
		public string Tip = "";
		public Texture2D Icon;
		public bool Centered;
	}

	[Signal] public delegate void RowSelectedEventHandler(int indice);
	[Signal] public delegate void RowActivatedEventHandler(int indice);
	[Signal] public delegate void CellClickedEventHandler(int indice, int coluna);
	[Signal] public delegate void HeaderClickedEventHandler(int coluna);
	[Signal] public delegate void RightClickedEventHandler(int indice);

	readonly List<GridCell[]> linhas = new();
	string[] titulos = System.Array.Empty<string>();
	float[] pesos = System.Array.Empty<float>();
	int selecionada = -1;
	int rolagem;
	int ultimoClique = -1;
	ulong ultimoQuando;
	bool amarela;
	bool arrastando;
	float arrasteNoPolegar;
	int colunaOrdem = -1;
	bool ordemCrescente = true;
	string[] dicasTitulo = System.Array.Empty<string>();
	readonly List<bool> marcas = new();
	static readonly Color CorMarca = new(0.96f, 0.62f, 0.55f);
	Font fonte;
	const int Height = 24;
	const int Cabeca = 26;
	const int Barra = 16;

	public int Selecionada => selecionada;

	public override void _Ready()
	{
		fonte = GetThemeDefaultFont();
		ClipContents = true;
		MouseFilter = MouseFilterEnum.Stop;
		Resized += QueueRedraw;
	}

	public void Build(string[] colunas, float[] larguras, bool selecaoAmarela)
	{
		titulos = colunas;
		pesos = larguras;
		amarela = selecaoAmarela;
		QueueRedraw();
	}

	public void Fill(IEnumerable<GridCell[]> dados, int manter = -1)
	{
		linhas.Clear();
		marcas.Clear();
		foreach (var linha in dados) linhas.Add(linha);
		selecionada = linhas.Count == 0 ? -1 : Mathf.Clamp(manter, 0, linhas.Count - 1);
		if (manter < 0) selecionada = linhas.Count == 0 ? -1 : 0;
		EnsureVisible();
		QueueRedraw();
	}

	public void SetHeaderTips(string[] dicas)
	{
		dicasTitulo = dicas ?? System.Array.Empty<string>();
	}

	public void SetSort(int coluna, bool crescente)
	{
		colunaOrdem = coluna;
		ordemCrescente = crescente;
		QueueRedraw();
	}

	public void Mark(IEnumerable<bool> quais)
	{
		marcas.Clear();
		if (quais != null) marcas.AddRange(quais);
		QueueRedraw();
	}

	public void SelectRow(int indice)
	{
		selecionada = linhas.Count == 0 ? -1 : Mathf.Clamp(indice, 0, linhas.Count - 1);
		EnsureVisible();
		QueueRedraw();
	}

	public override void _GuiInput(InputEvent e)
	{
		if (e is InputEventMouseMotion movimento)
		{
			if (arrastando) DragScrollbar(movimento.Position.Y);
			else UpdateTooltip(movimento.Position);
			AcceptEvent();
			return;
		}
		if (e is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.WheelUp && mb.Pressed) { ScrollBy(-Height * 3); AcceptEvent(); return; }
			if (mb.ButtonIndex == MouseButton.WheelDown && mb.Pressed) { ScrollBy(Height * 3); AcceptEvent(); return; }
			if (mb.ButtonIndex == MouseButton.Left && HitsScrollbar(mb.Position))
			{
				if (mb.Pressed) BeginScrollDrag(mb.Position.Y);
				else arrastando = false;
				AcceptEvent();
				return;
			}
			if (!mb.Pressed)
			{
				arrastando = false;
				return;
			}
			if (mb.ButtonIndex == MouseButton.Left || mb.ButtonIndex == MouseButton.Right)
			{
				HandleClick(mb);
				AcceptEvent();
			}
		}
	}

	public override void _Input(InputEvent e)
	{
		if (!arrastando) return;
		if (e is InputEventMouseMotion)
		{
			DragScrollbar(GetLocalMousePosition().Y);
			GetViewport().SetInputAsHandled();
		}
		else if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
		{
			arrastando = false;
			GetViewport().SetInputAsHandled();
		}
	}

	bool HitsScrollbar(Vector2 ponto) => MaxScroll() > 0 && ponto.X >= Size.X - Barra && ponto.Y >= Cabeca;

	void UpdateTooltip(Vector2 ponto)
	{
		string dica = "";
		if (!HitsScrollbar(ponto))
		{
			int coluna = ColumnAt(ponto.X);
			if (ponto.Y < Cabeca)
			{
				if (coluna >= 0 && coluna < dicasTitulo.Length) dica = dicasTitulo[coluna] ?? "";
			}
			else
			{
				int indice = ((int)ponto.Y - Cabeca + rolagem) / Height;
				if (indice >= 0 && indice < linhas.Count && coluna >= 0 && coluna < linhas[indice].Length)
					dica = linhas[indice][coluna]?.Tip ?? "";
			}
		}
		if (TooltipText != dica) TooltipText = dica;
	}

	void BeginScrollDrag(float y)
	{
		var (topo, alt) = MedirPolegar();
		if (y < topo || y > topo + alt)
		{
			arrasteNoPolegar = alt * 0.5f;
			DragScrollbar(y);
		}
		else arrasteNoPolegar = y - topo;
		arrastando = true;
	}

	void DragScrollbar(float y)
	{
		var (_, alt) = MedirPolegar();
		float livre = Math.Max(1, Size.Y - Cabeca - alt);
		float t = Mathf.Clamp((y - arrasteNoPolegar - Cabeca) / livre, 0, 1);
		rolagem = (int)Mathf.Round(t * MaxScroll());
		QueueRedraw();
	}

	(float topo, float alt) MedirPolegar()
	{
		float trilha = Math.Max(1, Size.Y - Cabeca);
		float conteudo = Math.Max(trilha, linhas.Count * Height);
		float alt = Math.Max(28, trilha * trilha / conteudo);
		float topo = Cabeca;
		if (MaxScroll() > 0) topo += (trilha - alt) * rolagem / MaxScroll();
		return (topo, alt);
	}

	void HandleClick(InputEventMouseButton mb)
	{
		if (mb.Position.Y < Cabeca)
		{
			EmitSignal(SignalName.HeaderClicked, ColumnAt(mb.Position.X));
			return;
		}
		int indice = ((int)mb.Position.Y - Cabeca + rolagem) / Height;
		if (indice < 0 || indice >= linhas.Count) return;
		selecionada = indice;
		QueueRedraw();
		if (mb.ButtonIndex == MouseButton.Right)
		{
			EmitSignal(SignalName.RightClicked, indice);
			EmitSignal(SignalName.RowSelected, indice);
			return;
		}
		ulong agora = Time.GetTicksMsec();
		bool duplo = indice == ultimoClique && agora - ultimoQuando < 400;
		ultimoClique = indice;
		ultimoQuando = agora;
			EmitSignal(SignalName.CellClicked, indice, ColumnAt(mb.Position.X));
		EmitSignal(duplo ? SignalName.RowActivated : SignalName.RowSelected, indice);
	}

	int ColumnAt(float x)
	{
		float largura = ContentWidth();
		float soma = 0;
		float cursor = 0;
		foreach (var p in pesos) soma += p;
		if (soma <= 0) return 0;
		for (int i = 0; i < pesos.Length; i++)
		{
			float w = largura * pesos[i] / soma;
			if (x < cursor + w) return i;
			cursor += w;
		}
		return Math.Max(0, pesos.Length - 1);
	}

	void ScrollBy(int delta)
	{
		rolagem = Mathf.Clamp(rolagem + delta, 0, MaxScroll());
		QueueRedraw();
	}

	void EnsureVisible()
	{
		if (selecionada < 0) return;
		int topo = selecionada * Height;
		int vista = Math.Max(Height, (int)Size.Y - Cabeca);
		if (topo < rolagem) rolagem = topo;
		else if (topo + Height > rolagem + vista) rolagem = topo + Height - vista;
		rolagem = Mathf.Clamp(rolagem, 0, MaxScroll());
	}

	int MaxScroll() => Math.Max(0, linhas.Count * Height - Math.Max(Height, (int)Size.Y - Cabeca));

	float ContentWidth() => Math.Max(40, Size.X - (MaxScroll() > 0 ? Barra : 0));

	public override void _Draw()
	{
		if (fonte == null) fonte = GetThemeDefaultFont();
		DrawRect(new Rect2(Vector2.Zero, Size), Colors.White);
		float largura = ContentWidth();
		float soma = 0;
		foreach (var p in pesos) soma += p;
		if (soma <= 0) return;
		float cursor = 0;
		int primeira = rolagem / Height;
		int vista = (int)Size.Y - Cabeca;
		int ultima = Math.Min(linhas.Count - 1, (rolagem + vista) / Height + 1);
		for (int i = primeira; i <= ultima; i++)
		{
			float y = Cabeca + i * Height - rolagem;
			bool marcada = i < marcas.Count && marcas[i];
			Color fundo = i == selecionada
				? (amarela ? new Color("ffe84a") : new Color("9dc7ee"))
				: marcada ? CorMarca
				: (i % 2 == 0 ? Colors.White : new Color("f3f1e4"));
			DrawRect(new Rect2(0, y, largura, Height), fundo);
			cursor = 0;
			var linha = linhas[i];
			for (int c = 0; c < pesos.Length; c++)
			{
				float w = largura * pesos[c] / soma;
				var celula = c < linha.Length ? linha[c] : null;
				float textoX = cursor + 4;
				if (celula?.Icon != null)
				{
					float ih = Mathf.Min(celula.Icon.GetHeight(), Height - 4);
					float iw = celula.Icon.GetWidth() * (ih / Math.Max(1, celula.Icon.GetHeight()));
					var r = new Rect2(cursor + 4, y + (Height - ih) / 2f, iw, ih);
					DrawTextureRect(celula.Icon, r, false);
					textoX = r.End.X + 4;
				}
				if (celula != null && !string.IsNullOrEmpty(celula.Text))
					DrawLabel(celula.Text, textoX, y, cursor + w - textoX - 4, Height, celula.Centered, Colors.Black);
				cursor += w;
			}
		}
		DrawRect(new Rect2(0, 0, largura, Cabeca), new Color("d9d6cf"));
		cursor = 0;
		for (int i = 0; i < titulos.Length; i++)
		{
			float w = largura * pesos[i] / soma;
			bool ordenada = i == colunaOrdem;
			float reserva = ordenada ? 18 : 0;
			DrawLabel(titulos[i], cursor + 4, 0, w - 8 - reserva, Cabeca, true, Colors.Black);
			if (ordenada)
			{
				var icone = EditorIcons.Get(ordemCrescente ? "up" : "down");
				if (icone != null)
					DrawTextureRect(icone, new Rect2(cursor + w - 18, (Cabeca - 14) / 2f, 14, 14), false);
			}
			cursor += w;
		}
		if (MaxScroll() > 0)
		{
			DrawRect(new Rect2(Size.X - Barra, Cabeca, Barra, Size.Y - Cabeca), new Color("d7d4cc"));
			var (y, polegar) = MedirPolegar();
			DrawRect(new Rect2(Size.X - Barra + 2, y, Barra - 4, polegar), arrastando ? new Color("5f5f5f") : new Color("8a8a8a"));
		}
	}

	void DrawLabel(string texto, float x, float y, float w, float h, bool centro, Color cor)
	{
		if (fonte == null || w < 4 || string.IsNullOrEmpty(texto)) return;
		int tamanho = 14;
		texto = FitText(texto, w, tamanho);
		var medida = fonte.GetStringSize(texto, HorizontalAlignment.Left, -1, tamanho);
		float px = centro ? x + Math.Max(0, (w - medida.X) / 2f) : x;
		float py = y + (h + fonte.GetAscent(tamanho)) / 2f - 1;
		fonte.DrawString(GetCanvasItem(), new Vector2(px, py), texto, HorizontalAlignment.Left, -1, tamanho, cor);
	}

	string FitText(string texto, float largura, int tamanho)
	{
		if (fonte.GetStringSize(texto, HorizontalAlignment.Left, -1, tamanho).X <= largura) return texto;
		while (texto.Length > 1 && fonte.GetStringSize(texto + "…", HorizontalAlignment.Left, -1, tamanho).X > largura)
			texto = texto[..^1];
		return texto + "…";
	}
}
