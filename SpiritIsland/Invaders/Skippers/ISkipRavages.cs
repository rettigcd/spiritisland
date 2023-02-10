namespace SpiritIsland;

public interface ISkipRavages : ISpaceEntity {
	UsageCost Cost { get; }
	/// <returns>True if Ravage is stopped</returns>
	/// <remarks>Uses Invader-Action to perform work since remove-disease/remove-wilds isn't really an 'Action'</remarks> 
	Task<bool> Skip( SpaceState space );
}