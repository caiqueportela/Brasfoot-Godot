using Godot;

namespace Brasfoot;

public partial class Catalog : Node
{
	public static Catalog Instance { get; private set; }
	public CatalogData CatalogData { get; private set; }

	public override void _EnterTree() => Instance = this;

	public override void _Ready()
	{
		Load();
	}

	public void Load()
	{
		if (!ResourceLoader.Exists(Paths.CatalogPath))
		{
			CatalogData = null;
			return;
		}
		var rec = ResourceLoader.Load<CatalogResource>(Paths.CatalogPath);
		CatalogData = rec?.ToData();
	}

	public void Save()
	{
		if (CatalogData == null) return;
		Directory.CreateDirectory(ProjectSettings.GlobalizePath("res://catalog"));
		var err = ResourceSaver.Save(CatalogResource.FromData(CatalogData), Paths.CatalogPath, ResourceSaver.SaverFlags.Compress);
		if (err != Error.Ok) throw new IOException(err.ToString());
		SyncQueue.Enqueue(Paths.CatalogPath);
	}
}
