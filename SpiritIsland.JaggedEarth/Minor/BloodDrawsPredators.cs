namespace SpiritIsland.JaggedEarth;

public class BloodDrawsPredators{ 

	const string Name = "Blood Draws Predators";

	[MinorCard(Name,1,Element.Sun,Element.Fire,Element.Water,Element.Animal),Fast, FromPresence(1)]
	[Instructions( "After the next time Invaders are Destroyed in target land: Add 1 Beasts, then 1 Damage per Beasts (max. 3 Damage)." ), Artist( Artists.KatGuevara )]
	static public Task ActAsync( TargetSpaceCtx ctx ){

		// After the next time Invaders are Destroyed in target land:
		ctx.Space.Adjust( new AddBeastAndDamageOnInvaderDestroyed( ctx ), 1 );

		return Task.CompletedTask;
	}

	public class AddBeastAndDamageOnInvaderDestroyed( TargetSpaceCtx ctx ) : BaseModEntity, IEndWhenTimePasses, IHandleTokenRemoved {
		public async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			if(args.Reason != RemoveReason.Destroyed || !args.Removed.Class.IsOneOf(Human.Invader)) return;
			Space from = (Space)args.From;
			from.Adjust(this,-1); // remove token

			// Add 1 Beast,
			await ctx.Beasts.AddAsync( 1 );

			// Then 1 Damage per Beast (max. 3 Damage)
			await ctx.DamageInvaders( ctx.Beasts.Count );
		}
	}

}