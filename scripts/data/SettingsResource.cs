using Godot;

namespace Brasfoot;

[GlobalClass]
public partial class SettingsResource : Resource
{
	[Export] public int Version { get; set; }
	[Export] public string Language { get; set; } = "pt";
	[Export] public float Volume { get; set; } = 1f;
	[Export] public bool VolumeSet { get; set; }
	[Export] public int Width { get; set; } = 1366;
	[Export] public int Height { get; set; } = 768;
	[Export] public int Monitor { get; set; }
	[Export] public int DisplayMode { get; set; }
	[Export] public bool CloudSync { get; set; }
	[Export] public string PendingFile { get; set; } = "";
}
