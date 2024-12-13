namespace SpiritIsland.NatureIncarnate;

public class ResoundingFootfallsSowDismay {
	const string Name = "Resounding Footfalls Sow Dismay";

	[SpiritCard( Name, 3, Element.Fire, Element.Air, Element.Earth ), Fast, FromPresence(0)]
	[Instructions( "1 Fear. Add 1 Quake. Skip all Ravage Actions." ), Artist( Artists.EmilyHancock )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await ctx.Space.AddAsync(Token.Quake,1);
		ctx.Space.SkipRavage(Name,UsageDuration.SkipAllThisTurn);
	}
}
