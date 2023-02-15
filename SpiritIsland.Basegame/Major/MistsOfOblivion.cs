namespace SpiritIsland.Basegame;

public class MistsOfOblivion {

	const string Name = "Mists of Oblivion";

	[MajorCard( Name, 4, Element.Moon, Element.Air, Element.Water )]
	[Slow]
	[FromPresence(3)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		var originalScope = ActionScope.Current;

		// 1 fear per town/city this power destroys (to a max of 4)
		int mayDestroyed = 4;
		void DoMists( ITokenRemovedArgs args ) {
			if(0 < mayDestroyed
				&& ActionScope.Current == originalScope
				&& args.Reason == RemoveReason.Destroyed
				&& args.Removed.Class.IsOneOf( Human.Town_City )
			) {
				ctx.AddFear( 1 );
				--mayDestroyed;
			}
		};
		ctx.Tokens.Adjust( new TokenRemovedHandler( DoMists ), 1 );

		// 1 damage to each invader
		await ctx.DamageEachInvader(1);

		// if you have 2 moon 3 air 2 water
		if(await ctx .YouHave("2 moon,3 air,2 water"))
			// 3 damage
			await ctx.DamageInvaders(3);
	}

}