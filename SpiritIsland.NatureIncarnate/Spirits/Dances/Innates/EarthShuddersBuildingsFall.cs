namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Earth Shudders, Buildings Fall" ), Slow, FromPresence( 0, Target.Quake )] // !!! Target.Quake
public class EarthShuddersBuildingsFall {

	[InPlayOption( "2 fire,3 earth", 3, "2 Damage per Quake, to Town/City only.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		// 2 Damage per Quake, to Town/City only.
		await ctx.DamageInvaders( 2 * ctx.Tokens[Token.Quake], Human.Town_City );
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
		ctx.AddFear( 1 );

		// In any # of lands with Quake:
		var options = GameState.Current.Spaces.Where( x => x.Has( Token.Quake ) && x.HasAny( Human.Invader ) ).Downgrade().ToList();
		while(0 < options.Count) {
			Space space = await ctx.Self.Gateway.Decision( new Select.ASpace( "Select land to generate 2 damage/quake and remove 1 quake.", options, Present.Done ) );
			if(space == null) break;
			options.Remove( space );

			var spaceCtx = ctx.Target( space );
			if(singleDamageToAll)
				await spaceCtx.Invaders.ApplyDamageToEach(1);
			// 2 Damage per Quake, to Town/City only.
			await spaceCtx.DamageInvaders( spaceCtx.Tokens[Token.Quake] * 2 );
			// Remove 1 Quake.
			await spaceCtx.Tokens.Remove( Token.Quake, 1 );
		}
	}


}

public class InPlayOptionAttribute : InnateOptionAttribute {
	public InPlayOptionAttribute( string elementText, int cardsInPlay, string description, int group )
		: base( elementText, description, group ) {
		CardsInPlay = cardsInPlay;
	}
	public int CardsInPlay { get; }

	public override string ThresholdString => base.ThresholdString + $" {CardsInPlay} cardplay";
}
