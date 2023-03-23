namespace SpiritIsland.Basegame;

public class HarbingersOfTheLightning {

	public const string Name = "Harbingers of the Lightning";

	[SpiritCard(HarbingersOfTheLightning.Name,0,Element.Fire,Element.Air), Slow,FromPresence(1,Target.Dahan)]
	[Instructions( "Push up to 2 Dahan. 1 Fear if you pushed any Dahan into a land with Town / City" ), Artist( Artists.RockyHammer )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){

		// Push up to 2 dahan.
		var destinationSpaces = await ctx.PushUpToNDahan(2);

		// if pushed dahan into town or city
		bool pushedToBuildingSpace = destinationSpaces
			.Any( neighbor => ctx.Target(neighbor).Tokens.HasAny(Human.Town_City) );

		if(pushedToBuildingSpace)
			ctx.AddFear(1);
	}

}
