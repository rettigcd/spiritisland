namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Allows the innate to be run twice.
/// </summary>
public class RepeatableInnatePower( Type actionType ) : InnatePower( actionType ), IHaveDynamicUseCounts {
	public int Used { get; set; } = 0;
	public int MaxUses { get; set; } = 1;

	protected override string IOption_Text => base.IOption_Text 
		+ (MaxUses == 1 ? "" : $"({(Used+1)} of {MaxUses})");
}
