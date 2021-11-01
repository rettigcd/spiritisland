namespace SpiritIsland {
	public class SpecialRule {

		readonly string title;
		readonly string description;

		public SpecialRule(string title, string description) {
			this.title = title;
			this.description = description;
		}
		public override string ToString() => title + " - " + description;

	} 

}