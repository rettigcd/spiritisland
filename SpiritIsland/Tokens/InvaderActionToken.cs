namespace SpiritIsland;

public class InvaderActionToken : ISpaceEntity {

	public InvaderActionToken( string label ) {
		Label = label;
	}

	public string Label { get; }

}