namespace SpiritIsland;

public class SpecialRule( string title, string description ) {

	public string Title { get; } = title;
	public string Description { get; } = description;

	public override string ToString() => Title + " - " + Description;

}