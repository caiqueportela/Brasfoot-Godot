using Godot;

namespace Brasfoot;

public static class Translator
{
	public static string T(string chave) => TranslationServer.Translate(chave);

	public static string F(string bruto)
	{
		if (string.IsNullOrEmpty(bruto) || !bruto.Contains('|'))
		{
			string direto = T(bruto);
			return direto == bruto ? bruto : direto;
		}
		var p = bruto.Split('|');
		string modelo = T(p[0]);
		if (modelo == p[0] && !modelo.Contains('{')) return bruto;
		try { return string.Format(modelo, p.Skip(1).Cast<object>().ToArray()); }
		catch (FormatException) { return modelo; }
	}

	public static string Narrate(MatchEvent e, CareerState est)
	{
		string time = est?.FindClub(e.ClubSlug)?.Name ?? "";
		string jog = "";
		if (est != null && e.Player != 0)
			jog = est.FindClub(e.ClubSlug)?.FindPlayer(e.Player)?.Name ?? "";
		return e.Kind switch
		{
			"goal" when e.Extra == 1 => string.Format(T("event.own_goal"), e.Minute, jog, time),
			"goal" when e.Extra == 2 => string.Format(T("event.penalty_goal"), e.Minute, jog, time),
			"goal" when e.Extra == 3 => string.Format(T("event.header_goal"), e.Minute, jog, time),
			"goal" => string.Format(T("event.goal"), e.Minute, jog, time),
			"yellow" => string.Format(T("event.yellow"), e.Minute, jog),
			"red" => string.Format(T("event.red"), e.Minute, jog, time),
			"injury" => string.Format(T("event.injury"), e.Minute, jog, e.Extra),
			"halftime" => T("event.halftime"),
			"full_time" => T("event.full_time"),
			_ => e.Description ?? ""
		};
	}
}
