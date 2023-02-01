namespace SpiritIsland;

/// <summary> Stops either 1 or ALL Explores. </summary>
public interface ISkipExploreTo : IToken {
	UsageCost Cost { get; }
	/// <returns>True if Build is stopped</returns>
	/// <remarks>Uses Invader-Action to perform work since remove-disease/remove-wilds isn't really an 'Action'</remarks> 
	Task<bool> Skip( GameCtx gameCtx, SpaceState space );
}