using Godot;

namespace Brasfoot;

public partial class SaveCareerScreen : ScreenBase
{
	public override void _Ready()
	{
		base._Ready();
		var humano = Session.Instance.Current?.HumanClubs().FirstOrDefault();
		GetNode<LineEdit>("%Name").Text = humano?.Name ?? "Career";
		GetNode<Button>("%Save").Pressed += Save;
		GetNode<Button>("%Cancel").Pressed += () => Session.Instance.GoTo(Paths.Office);
	}

	protected override void ApplyTexts()
	{
		GetNode<Label>("%Title").Text = Translator.T("save.title");
		GetNode<Button>("%Save").Text = Translator.T("save.ok");
		GetNode<Button>("%Cancel").Text = Translator.T("office.cancel");
	}

	void Save()
	{
		string arquivo = string.Join("_", GetNode<LineEdit>("%Name").Text.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
		if (arquivo.Length == 0) arquivo = "Career";
		try
		{
			CareerFile.Save(Session.Instance.Current, Paths.UserSave(arquivo + ".res"));
			Alert(string.Format(Translator.T("save.done"), arquivo), () => Session.Instance.GoTo(Paths.Office));
		}
		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
			Alert(Translator.T("save.error"));
		}
	}
}
