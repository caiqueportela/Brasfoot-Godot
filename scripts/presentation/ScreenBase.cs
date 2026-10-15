using Godot;

namespace Brasfoot;

public partial class ScreenBase : Control
{
	Action depois;

	public override void _Ready()
	{
		var d = GetNodeOrNull<AcceptDialog>("%Dialog");
		if (d != null)
			d.Confirmed += () => depois?.Invoke();
		ApplyTexts();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationTranslationChanged && IsNodeReady())
			ApplyTexts();
	}

	protected virtual void ApplyTexts() { }

	protected void Alert(string texto, Action acao = null)
	{
		depois = acao;
		var d = GetNodeOrNull<AcceptDialog>("%Dialog");
		if (d == null)
		{
			GD.Print(texto);
			acao?.Invoke();
			return;
		}
		d.Title = Translator.T("common.notice");
		d.OkButtonText = Translator.T("common.continue");
		d.DialogText = texto;
		d.PopupCentered();
	}
}
