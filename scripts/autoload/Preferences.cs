using Godot;

namespace Brasfoot;

public partial class Preferences : Node
{
	public static Preferences Instance { get; private set; }
	public SettingsResource Settings { get; private set; } = new();

	public override void _EnterTree() => Instance = this;

	public override void _Ready()
	{
		bool rewrite = false;
		if (ResourceLoader.Exists(Paths.SettingsPath))
		{
			var rec = ResourceLoader.Load<SettingsResource>(Paths.SettingsPath);
			if (rec != null && rec.Version >= Paths.FormatVersion) Settings = rec;
			else rewrite = true;
		}
		else rewrite = true;
		if (rewrite)
		{
			Settings = new SettingsResource
			{
				Version = Paths.FormatVersion,
				Language = "pt",
				Volume = 1f,
				VolumeSet = true,
				Width = 1366,
				Height = 768
			};
		}
		Apply();
		if (rewrite) Save();
	}

	public void Apply()
	{
		TranslationServer.SetLocale(Settings.Language);
		ApplyVolume();
		if (DisplayServer.GetName() == "headless") return;
		if (OS.GetEnvironment("BRASFOOT_SMOKE") == "1") return;
		if (OS.GetEnvironment("BRASFOOT_FOTO") == "1" || OS.GetEnvironment("BRASFOOT_FOTO_CFG") == "1" || OS.GetEnvironment("BRASFOOT_FOTO_EDITOR") == "1") return;
		ApplyDisplay();
	}

	public void SetLanguage(string codigo)
	{
		if (Settings.Language == codigo) return;
		Settings.Language = codigo;
		TranslationServer.SetLocale(codigo);
		Save();
	}

	public void SetVolume(float linear)
	{
		Settings.Volume = Mathf.Clamp(linear, 0f, 1f);
		Settings.VolumeSet = true;
		ApplyVolume();
		Save();
	}

	public void SetDisplay(int largura, int altura, int monitor, int modo)
	{
		Settings.Width = Math.Max(640, largura);
		Settings.Height = Math.Max(480, altura);
		Settings.Monitor = Math.Max(0, monitor);
		Settings.DisplayMode = Mathf.Clamp(modo, 0, 2);
		ApplyDisplay();
		Save();
	}

	public void ApplyVolume()
	{
		float v = Mathf.Clamp(Settings.Volume, 0f, 1f);
		AudioServer.SetBusVolumeDb(0, v <= 0.001f ? -80f : Mathf.LinearToDb(v));
	}

	public void ApplyDisplay()
	{
		if (DisplayServer.GetName() == "headless" || Engine.IsEmbeddedInEditor()) return;
		int telas = Math.Max(1, DisplayServer.GetScreenCount());
		int monitor = Mathf.Clamp(Settings.Monitor, 0, telas - 1);
		var tela = DisplayServer.ScreenGetSize(monitor);
		int largura = Math.Max(640, Settings.Width);
		int altura = Math.Max(480, Settings.Height);
		if (Settings.DisplayMode != 2)
		{
			largura = Math.Min(largura, Math.Max(640, tela.X));
			altura = Math.Min(altura, Math.Max(480, tela.Y));
		}
		var tamanho = new Vector2I(largura, altura);
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, Settings.DisplayMode == 1);
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.ResizeDisabled, Settings.DisplayMode != 2);
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.MaximizeDisabled, Settings.DisplayMode != 2);
		DisplayServer.WindowSetCurrentScreen(monitor);
		if (Settings.DisplayMode == 2)
		{
			DisplayServer.WindowSetMinSize(Vector2I.Zero);
			DisplayServer.WindowSetMaxSize(Vector2I.Zero);
			DisplayServer.WindowSetSize(tamanho);
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
			return;
		}
		DisplayServer.WindowSetMinSize(tamanho);
		DisplayServer.WindowSetMaxSize(tamanho);
		DisplayServer.WindowSetSize(tamanho);
		var origem = DisplayServer.ScreenGetPosition(monitor);
		var margem = (tela - tamanho) / 2;
		if (margem.X < 0) margem.X = 0;
		if (margem.Y < 0) margem.Y = 0;
		DisplayServer.WindowSetPosition(origem + margem);
	}

	public void SetCloud(bool ligada)
	{
		Settings.CloudSync = ligada;
		Save();
	}

	public void Save()
	{
		var err = ResourceSaver.Save(Settings, Paths.SettingsPath, ResourceSaver.SaverFlags.Compress);
		if (err != Error.Ok) GD.PushError("settings " + err);
	}
}
