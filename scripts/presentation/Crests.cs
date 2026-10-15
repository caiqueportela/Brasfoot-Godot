using Godot;

namespace Brasfoot;

public static class Crests
{
	static readonly Dictionary<string, Texture2D> cache = new();

	public static Texture2D Get(string slug, bool small = false)
	{
		if (string.IsNullOrEmpty(slug)) return null;
		string folder = small ? "crests_small" : "crests";
		string key = folder + ":" + slug;
		if (cache.TryGetValue(key, out var ready)) return ready;
		var tex = Load(folder, slug);
		if (tex == null && small) tex = Load("crests", slug);
		if (tex != null) cache[key] = tex;
		return tex;
	}

	public static Texture2D Shirt(string slug, int which)
	{
		if (string.IsNullOrEmpty(slug)) return null;
		string folder = which switch { 2 => "shirts_away", 3 => "shirts_third", _ => "shirts" };
		string key = folder + ":" + slug;
		if (cache.TryGetValue(key, out var ready)) return ready;
		var tex = Load(folder, slug);
		if (tex != null) cache[key] = tex;
		return tex;
	}

	public static bool FileExists(string folder, string slug)
	{
		if (string.IsNullOrEmpty(slug)) return false;
		return File.Exists(Paths.ArtFile("clubs/" + folder + "/" + slug + ".png"));
	}

	static Texture2D Load(string folder, string slug)
	{
		string path = Paths.ArtFile("clubs/" + folder + "/" + slug + ".png");
		if (!File.Exists(path)) return null;
		var img = Image.LoadFromFile(path);
		if (img == null || img.IsEmpty()) return null;
		return ImageTexture.CreateFromImage(img);
	}
}
