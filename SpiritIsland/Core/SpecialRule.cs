namespace SpiritIsland;

public class SpecialRule {

	public string Title { get; }
	public string Description { get; }

	public SpecialRule(string title, string description) {
		this.Title = title;
		this.Description = description;
	}
	public override string ToString() => Title + " - " + Description;

}