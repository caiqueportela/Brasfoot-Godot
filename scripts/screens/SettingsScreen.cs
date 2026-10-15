using Godot;

namespace Brasfoot;

public partial class SettingsScreen : ScreenBase
{
	static readonly Vector2I[] Bases =
	{
		new(1280, 720),
		new(1280, 800),
		new(1366, 768),
		new(1600, 900),
		new(1920, 1080),
		new(2560, 1440),
		new(3840, 2160)
	};

	readonly List<Vector2I> resolucoes = new();
	bool interno;

	public override void _Ready()
	{
		base._Ready();
		GetNode<HSlider>("%Volume").ValueChanged += v =>
		{
			if (interno) return;
			int pct = Mathf.RoundToInt((float)v);
			GetNode<Label>("%VolumeValue").Text = pct + "%";
			Preferences.Instance.SetVolume(pct / 100f);
		};
		GetNode<OptionButton>("%Language").ItemSelected += i =>
		{
			if (interno) return;
			string codigo = GetNode<OptionButton>("%Language").GetItemId((int)i) switch { 1 => "en", 2 => "es", _ => "pt" };
			Preferences.Instance.SetLanguage(codigo);
		};
		GetNode<OptionButton>("%Monitor").ItemSelected += i =>
		{
			if (interno) return;
			FillResolutions(GetNode<OptionButton>("%Monitor").GetItemId((int)i));
			ApplySelection();
		};
		GetNode<OptionButton>("%Resolution").ItemSelected += _ => { if (!interno) ApplySelection(); };
		GetNode<OptionButton>("%DisplayMode").ItemSelected += _ => { if (!interno) ApplySelection(); };
		GetNode<Button>("%Back").Pressed += () => Session.Instance.GoTo(Paths.Menu);
		if (OS.GetEnvironment("BRASFOOT_FOTO_CFG") == "1")
		{
			GetTree().CreateTimer(0.7).Timeout += () =>
			{
				Screenshot.Save(this, "verificacao_config.png");
				var prefs = Preferences.Instance;
				string idioma = prefs.Settings.Language;
				float volume = prefs.Settings.Volume;
				GetNode<HSlider>("%Volume").Value = 50;
				float db = AudioServer.GetBusVolumeDb(0);
				prefs.SetLanguage("en");
				string en = GetNode<Button>("%Back").Text;
				prefs.SetLanguage("pt");
				string pt = GetNode<Button>("%Back").Text;
				prefs.SetLanguage(idioma);
				prefs.SetVolume(volume);
				GD.Print($"CFG db={db:0.0} en={en} pt={pt} modos={GetNode<OptionButton>("%DisplayMode").ItemCount} telas={GetNode<OptionButton>("%Monitor").ItemCount} res={GetNode<OptionButton>("%Resolution").ItemCount}");
				GetTree().Quit();
			};
		}
	}

	protected override void ApplyTexts()
	{
		GetNode<Label>("%Title").Text = Translator.T("settings.title");
		GetNode<Label>("%LabelVolume").Text = Translator.T("settings.volume");
		GetNode<Label>("%LabelLanguage").Text = Translator.T("settings.language");
		GetNode<Label>("%LabelResolution").Text = Translator.T("settings.resolution");
		GetNode<Label>("%LabelMonitor").Text = Translator.T("settings.monitor");
		GetNode<Label>("%LabelMode").Text = Translator.T("settings.mode");
		GetNode<Button>("%Back").Text = Translator.T("settings.back");
		GetNode<HSlider>("%Volume").TooltipText = Translator.T("settings.tip.volume");
		var dados = Preferences.Instance.Settings;
		var volume = GetNode<HSlider>("%Volume");
		interno = true;
		volume.Value = Mathf.RoundToInt(dados.Volume * 100f);
		GetNode<Label>("%VolumeValue").Text = Mathf.RoundToInt(dados.Volume * 100f) + "%";
		var idioma = GetNode<OptionButton>("%Language");
		idioma.Clear();
		idioma.AddItem("Português", 0);
		idioma.AddItem("English", 1);
		idioma.AddItem("Español", 2);
		idioma.Selected = dados.Language switch { "en" => 1, "es" => 2, _ => 0 };
		var modo = GetNode<OptionButton>("%DisplayMode");
		modo.Clear();
		modo.AddItem(Translator.T("settings.window"), 0);
		modo.AddItem(Translator.T("settings.borderless"), 1);
		modo.AddItem(Translator.T("settings.fullscreen"), 2);
		modo.Selected = Mathf.Clamp(dados.DisplayMode, 0, 2);
		FillMonitors();
		FillResolutions(SelectedMonitor());
		interno = false;
	}

	void FillMonitors()
	{
		var lista = GetNode<OptionButton>("%Monitor");
		lista.Clear();
		int n = Math.Max(1, DisplayServer.GetScreenCount());
		int atual = Mathf.Clamp(Preferences.Instance.Settings.Monitor, 0, n - 1);
		for (int i = 0; i < n; i++)
		{
			var tamanho = DisplayServer.ScreenGetSize(i);
			lista.AddItem(string.Format(Translator.T("settings.monitor_name"), i + 1, tamanho.X, tamanho.Y), i);
		}
		lista.Selected = lista.GetItemIndex(atual);
	}

	int SelectedMonitor()
	{
		var lista = GetNode<OptionButton>("%Monitor");
		if (lista.Selected < 0) return 0;
		return lista.GetItemId(lista.Selected);
	}

	void FillResolutions(int monitor)
	{
		bool estava = interno;
		interno = true;
		var tela = DisplayServer.ScreenGetSize(Mathf.Clamp(monitor, 0, Math.Max(0, DisplayServer.GetScreenCount() - 1)));
		resolucoes.Clear();
		foreach (var r in Bases)
			if (r.X <= tela.X && r.Y <= tela.Y && !resolucoes.Contains(r))
				resolucoes.Add(r);
		if (!resolucoes.Contains(tela) && tela.X >= 640 && tela.Y >= 480)
			resolucoes.Add(tela);
		if (resolucoes.Count == 0)
			resolucoes.Add(new Vector2I(1280, 800));
		resolucoes.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));
		var lista = GetNode<OptionButton>("%Resolution");
		lista.Clear();
		var desejada = new Vector2I(Preferences.Instance.Settings.Width, Preferences.Instance.Settings.Height);
		int selecionar = 0;
		int melhor = int.MaxValue;
		for (int i = 0; i < resolucoes.Count; i++)
		{
			var r = resolucoes[i];
			lista.AddItem(r.X + "x" + r.Y, i);
			int dist = Math.Abs(r.X - desejada.X) + Math.Abs(r.Y - desejada.Y);
			if (dist < melhor)
			{
				melhor = dist;
				selecionar = i;
			}
		}
		lista.Selected = selecionar;
		interno = estava;
	}

	void ApplySelection()
	{
		var lista = GetNode<OptionButton>("%Resolution");
		if (lista.Selected < 0 || resolucoes.Count == 0) return;
		var r = resolucoes[lista.GetItemId(lista.Selected)];
		int modo = GetNode<OptionButton>("%DisplayMode").GetItemId(GetNode<OptionButton>("%DisplayMode").Selected);
		Preferences.Instance.SetDisplay(r.X, r.Y, SelectedMonitor(), modo);
	}
}
