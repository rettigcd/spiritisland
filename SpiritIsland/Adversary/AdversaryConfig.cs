namespace SpiritIsland;

public record AdversaryConfig(string Name, int Level) {
	public static readonly AdversaryConfig NullAdversary = new AdversaryConfig("", 0);
}
