namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FearLevelAttribute : Attribute {

	public string Description { get; }

	public FearLevelAttribute(int _, string description ) {
		this.Description = description;
	}
}
