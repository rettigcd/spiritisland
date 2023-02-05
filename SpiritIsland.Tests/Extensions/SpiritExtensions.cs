namespace SpiritIsland.Tests;

public static class SpiritExtensions {

	/// <summary> Binds to the Next Decision </summary>
	internal static DecisionContext NextDecision( this Spirit spirit ) => new DecisionContext( spirit );

	internal static SpiritConfigContext Configure( this Spirit spirit ) => new SpiritConfigContext( spirit );

	internal static void Adjust( this SpiritPresence presence, SpaceState space, int count ) => space.Adjust( presence.Token, count );

}
