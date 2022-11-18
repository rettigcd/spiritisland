namespace SpiritIsland.PromoPack1;

[InnatePower( SerpentRousesInAnger.Name ), Slow, FromPresence(0)]
public class SerpentRousesInAnger {

	public const string Name = "Serpent Rouses in Anger";

	[InnateOption( "1 fire,1 earth","For each fire earth you have, 1 Damage to 1 town / city." )]
	static public Task Option1Async( TargetSpaceCtx ctx ) {
		// For each fire & earth you have
		int count = Math.Min( ctx.Self.Elements[Element.Fire], ctx.Self.Elements[Element.Earth]);
		// 1 Damage to 1 town / city.
		return ctx.Invaders.UserSelectedDamage(count,ctx.Self,Invader.Town,Invader.City);
	}

	[InnateOption( "2 moon 2 earth", "For each 2 moon 2 earth you have, 2 fear and you may Push 1 town from target land." )]
	static public async Task Option2Async( TargetSpaceCtx ctx ) {
		await Option1Async( ctx );

		// For each 2 moon 2 earth you have
		int count = Math.Min( ctx.Self.Elements[Element.Moon], ctx.Self.Elements[Element.Earth]) / 2;
		// 2 fear
		ctx.AddFear( count*2 );
		// you may Push 1 town from target land.
		await ctx.PushUpTo( count, Invader.Town );
	}

	[InnateOption("5 moon,6 fire,6 earth", "-7 Energy.  In every land in the game: X Damage, where X is the number of presence you have in and adjacent to that land." )]
	static public async Task Option3Async( TargetSpaceCtx ctx ) {
		await Option2Async( ctx );

		if(7 <= ctx.Self.Energy && await ctx.Self.UserSelectsFirstText("Activate Teir 3?","Pay 7 to cause damage from presence", "skip" )){
			// -7 Energy.
			ctx.Self.Energy -= 7;
			// In every land in the game: X Damage, where X is the number of presence you have in and adjacent to that land.
			var invaderLands = ctx.GameState.AllActiveSpaces
				.Where(space => space.HasInvaders())
				.ToArray();
			foreach(var land in invaderLands) {
				var landsCreatingDamage = new HashSet<Space>(land.Range(1).Select(x=>x.Space));
				int damage = ctx.Self.Presence.Placed.Count( landsCreatingDamage.Contains );
				await ctx.DamageInvaders(damage);
			}
		}

	}

}