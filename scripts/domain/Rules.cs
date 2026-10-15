namespace Brasfoot;

public static class Rules
{
	public static readonly string[] PositionCodes = { "GOL", "LAT", "ZAG", "MEI", "ATA" };
	public static readonly string[] TraitCodes = { "Col", "DPe", "Ref", "SGo", "Arm", "Cab", "Cru", "Des", "Dri", "Fin", "Mar", "Pas", "Res", "Vel" };
	public static readonly string[] Formations = { "4-4-2", "4-3-3", "4-5-1", "3-5-2", "5-3-2", "3-4-3", "4-2-3-1" };
	public static readonly string[] Tactics = { "balanced", "all_out_attack", "counter" };
	public static readonly string[] Confederations = { "Champions League", "Copa Libertadores", "CAF Champions League", "AFC Champions League", "Concacaf Champions Cup", "OFC Champions League" };

	static readonly Dictionary<string, int[]> Slots = new()
	{
		["4-4-2"] = new[] { 1, 2, 2, 4, 2 },
		["4-3-3"] = new[] { 1, 2, 2, 3, 3 },
		["4-5-1"] = new[] { 1, 2, 2, 5, 1 },
		["3-5-2"] = new[] { 1, 0, 3, 5, 2 },
		["5-3-2"] = new[] { 1, 2, 3, 3, 2 },
		["3-4-3"] = new[] { 1, 0, 3, 4, 3 },
		["4-2-3-1"] = new[] { 1, 2, 2, 5, 1 },
	};

	public static string TraitCode(int c) => c >= 0 && c < TraitCodes.Length ? TraitCodes[c] : "?";
	public static string TraitName(int c) => c >= 0 && c < TraitCodes.Length ? Translator.T("trait." + c) : "?";
	public static string Position(int p) => p >= 0 && p < PositionCodes.Length ? PositionCodes[p] : "?";
	public static string Side(int l) => Translator.T(l == 1 ? "side.1" : "side.0");

	public static int[] SlotsFor(string formacao) => Slots.TryGetValue(formacao, out var s) ? s : Slots["4-4-2"];

	public static string Money(long v)
	{
		var neg = v < 0;
		var n = Math.Abs(v);
		var s = n.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
		return (neg ? "-R$ " : "R$ ") + s;
	}

	public static string ShortDate(string iso)
	{
		if (DateTime.TryParse(iso, out var d)) return d.ToString("dd/MM/yyyy");
		return iso;
	}

	public static int BaseStrength(ClubRecord time, PlayerRecord j, Random rng)
	{
		int n5 = time.Reputation switch { >= 5 => 22, 4 => 15, 3 => 8, 2 => 5, _ => 4 };
		int nivel = time.Level;
		int n7 = nivel switch
		{
			<= 15 => nivel,
			16 => 17,
			17 => 18,
			18 => 19,
			19 => 21,
			20 => 25,
			21 => 26,
			22 => 27,
			_ => nivel + 5
		};
		n5 = n7 + n5 + rng.Next(3);
		if (j.LineupStatus == 1) n5 += 8 + rng.Next(2);
		if (j.IsStar || j.WorldClass) n5 += 9 + rng.Next(3);
		int paisNivel = 15;
		if (nivel < 10)
		{
			double f = nivel < 3 ? 0.5 : nivel < 5 ? 0.6 : 0.75;
			n5 = (int)Math.Round(n5 * f);
		}
		else if (paisNivel <= 13 && nivel < 12)
		{
			n5 = (int)Math.Round(n5 * 0.8);
		}
		n5 += (j.PrimaryTrait == 13 || j.SecondaryTrait == 13) ? 2 : 0;
		n5 += (j.PrimaryTrait == 9 || j.SecondaryTrait == 9) && j.Position == 4 ? 2 : 0;
		if (j.Age <= 20) n5 -= 4;
		if (j.Age >= 35) n5 -= 3;
		return Math.Clamp(n5, 4, 96);
	}

	public static int WageFor(int forca, int divisao, int posicao, int idade, bool estrela)
	{
		int n4 = divisao switch { 1 => 600, 2 => 500, 3 => 450, 4 => 400, _ => 350 };
		if (posicao == 0) n4 -= 70;
		else if (posicao == 2) n4 -= 30;
		else if (posicao == 1) n4 -= 40;
		else if (posicao == 4) n4 -= 50;
		n4 = (int)Math.Round(0.5 * n4);
		int n3 = forca * 2 * Math.Max(80, n4);
		if (idade >= 32) n3 -= (idade - 32) * 300;
		if (estrela) n3 += forca * 250;
		if (n3 < 800) n3 = 800;
		return n3;
	}

	public static int MarketValue(int forca, int nivel, int idade, int posicao, bool estrela, bool titular)
	{
		int n4 = forca * 2;
		n4 *= n4;
		int n5 = nivel >= 20 ? 600 : nivel >= 18 ? 500 : nivel >= 12 ? 400 : 280;
		if (estrela) n5 = (int)Math.Round(n5 * 1.7);
		if (posicao == 4) n5 = (int)Math.Round(n5 * 1.3);
		if (titular) n5 = (int)Math.Round(n5 * 1.2);
		int idadeBonus = idade < 23 ? (30 - idade) * 18 : idade < 32 ? (32 - idade) * 8 : -(idade - 32) * 40;
		n5 += idadeBonus;
		if (n5 < 40) n5 = 40;
		return Math.Max(20_000, n4 * n5 / 8);
	}
}

