using Godot;

namespace Brasfoot;

public partial class LoadGameScreen : ScreenBase
{
	readonly List<string> caminhos = new();

	public override void _Ready()
	{
		base._Ready();
		GetNode<Button>("%Open").Pressed += Open;
		GetNode<Button>("%DeleteFile").Pressed += Delete;
		GetNode<Button>("%Back").Pressed += () => Session.Instance.GoTo(Paths.Menu);
		Reload();
	}

	protected override void ApplyTexts()
	{
		GetNode<Label>("%Title").Text = Translator.T("load.title");
		GetNode<Button>("%Open").Text = Translator.T("load.open");
		GetNode<Button>("%DeleteFile").Text = Translator.T("load.delete");
		GetNode<Button>("%Back").Text = Translator.T("load.back");
		GetNode<Label>("%Empty").Text = Translator.T("load.empty");
	}

	void Reload()
	{
		var lista = GetNode<ItemList>("%List");
		lista.Clear();
		caminhos.Clear();
		string pasta = Paths.SavesFolder();
		if (!Directory.Exists(pasta)) return;
		foreach (var arq in Directory.GetFiles(pasta, "*.res").OrderByDescending(File.GetLastWriteTimeUtc))
		{
			caminhos.Add("user://saves/" + Path.GetFileName(arq));
			lista.AddItem(Path.GetFileNameWithoutExtension(arq));
		}
		GetNode<Label>("%Empty").Visible = caminhos.Count == 0;
	}

	int SelectedIndex()
	{
		var sel = GetNode<ItemList>("%List").GetSelectedItems();
		return sel.Length == 0 ? -1 : sel[0];
	}

	void Open()
	{
		int i = SelectedIndex();
		if (i < 0)
		{
			Alert(Translator.T("load.choose"));
			return;
		}
		try
		{
			Session.Instance.Current = CareerFile.Open(caminhos[i], Catalog.Instance.CatalogData);
			Session.Instance.TabIndex = 0;
			Session.Instance.MatchQueue.Clear();
			Session.Instance.GoTo(Paths.Office);
		}
		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
			Alert(Translator.T("load.invalid"));
		}
	}

	void Delete()
	{
		int i = SelectedIndex();
		if (i < 0) return;
		string disco = Paths.AbsolutePath(caminhos[i]);
		if (File.Exists(disco)) File.Delete(disco);
		Reload();
	}
}
