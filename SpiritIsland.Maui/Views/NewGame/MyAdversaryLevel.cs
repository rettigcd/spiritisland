namespace SpiritIsland.Maui;

public class MyAdversaryLevel(SpiritIsland.AdversaryLevel src) {
	public int Level => src.Level;
	public string LevelText => src.Level == 0 ? "Escalation" : $"Level {src.Level}";
	public string Title => src.Title;
	public string Description => src.Description;

	public int Difficulty => src.Difficulty;
//	public int[] FearCards => src.FearCards;
	public Color ShadowColor { get; set; } = Colors.LightGray;
}