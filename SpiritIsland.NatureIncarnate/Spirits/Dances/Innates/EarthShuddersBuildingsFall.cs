namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Earth Shudders, Buildings Fall" ), Slow, FromPresence( 0, Filter.Quake )] // !!! Target.Quake
public class EarthShuddersBuildingsFall {

	[InPlayOption( "2 fire,3 earth", 3, "2 Damage per Quake, to Town/City only.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		// 2 Damage per Quake, to Town/City only.
		await ctx.DamageInvaders( 2 * ctx.Space[Token.Quake], Human.Town_City );
	}

	[InPlayOption( "3 fire,4 earth", 5, "1 Fear. In any # of lands with Quake: 2 Damage per Quake, to Town/City only. Remove 1 Quake.", 1 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		await QuakesCauseDamage( ctx, false );
	}

	[InPlayOption( "4 fire,5 earth", 7, "2 Fear. In each land where you removed Quake: 1 Damage to each Invader.", 1 )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		await QuakesCauseDamage( ctx, true );
	}

	static async Task QuakesCauseDamage( TargetSpaceCtx ctx, bool singleDamageToAll ) {
		// 1 Fear.
		await ctx.AddFear( 1 );

		// In any # of lands with Quake:
		var options = ActionScope.Current.Spaces.Where( x => x.Has( Token.Quake ) && x.HasAny( Human.Invader ) ).ToList();
		while(0 < options.Count) {
			Space? space = await ctx.Self.Select("Select land to generate 2 damage/quake and remove 1 quake.", options, Present.Done );
			if(space is null) break;
			options.Remove( space );

			var spaceCtx = ctx.Target( space );
			if(singleDamageToAll)
				await spaceCtx.Invaders.ApplyDamageToEach(1);
			// 2 Damage per Quake, to Town/City only.
			await spaceCtx.DamageInvaders( spaceCtx.Space[Token.Quake] * 2 );
			// Remove 1 Quake.
			await spaceCtx.Space.RemoveAsync( Token.Quake, 1 );
		}
	}


}

public class InPlayOptionAttribute( string elementText, int cardsInPlay, string description, int group ) : InnateTierAttribute( elementText, description, group ) {
	public int CardsInPlay { get; } = cardsInPlay;

	public override string ThresholdString => base.ThresholdString + $" {CardsInPlay} cardplay";
}
