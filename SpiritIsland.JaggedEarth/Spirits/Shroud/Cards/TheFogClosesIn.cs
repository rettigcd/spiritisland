namespace SpiritIsland.JaggedEarth;

public class TheFogClosesIn {

	[SpiritCard("The Fog Closes In",0,Element.Moon,Element.Air,Element.Water), Slow, FromPresence(0)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// For each adjacent land with your presence, 1 Damage to a different Invader.
		int count = ctx.Space.Adjacent.Count( s=>ctx.Target(s).Presence.IsHere );
		await ctx.Apply1DamageToDifferentInvaders( count );

		// Push 2 dahan
		await ctx.PushDahan(2);
	}

}