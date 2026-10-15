using Godot;

namespace Brasfoot;

public static class CareerFile
{
	public static void Save(CareerState est, string caminho)
	{
		var rec = CareerResource.FromState(est);
		Directory.CreateDirectory(ParentFolder(caminho));
		var err = ResourceSaver.Save(rec, caminho, ResourceSaver.SaverFlags.Compress);
		if (err != Error.Ok) throw new IOException(err.ToString());
		SyncQueue.Enqueue(caminho);
	}

	static string ParentFolder(string caminho) => Path.GetDirectoryName(Paths.AbsolutePath(caminho));

	public static CareerState Open(string caminho, CatalogData banco)
	{
		var rec = ResourceLoader.Load<CareerResource>(caminho);
		if (rec == null) throw new InvalidDataException(caminho);
		return rec.ToState(banco);
	}
}
