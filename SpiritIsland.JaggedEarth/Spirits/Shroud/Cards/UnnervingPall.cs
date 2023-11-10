namespace SpiritIsland.JaggedEarth;

public class UnnervingPall {

	[SpiritCard("Unnerving Pall",1,Element.Moon,Element.Air,Element.Animal), Fast, FromPresence(0,Target.Invaders)]
	[Instructions( "1 Fear. Up to 3 damaged Invaders do not participate in Ravage. -or- 1 Fear. Defend 1 per Presence you have in target land (when this Power is used)." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear.
		ctx.AddFear(1);

		// up to 3 Damaged Invaders do not participate in Ravage.
		var doNotParticipate = new SpaceCmd(
			"up to 3 damaged Invaders do not participate in Ravage",
			SelectUpTo3DamagedInvadersToNotParticipate
		);
		// Defend 1 per presence you have in target land (when this Power is used).
		var defend = new SpaceCmd(
			"Defend 1 per presence you have in target land", // (when power is used)
			ctx => ctx.Defend( ctx.Presence.Count )
		);

		await ctx.SelectActionOption( doNotParticipate, defend );

	}

	static async Task SelectUpTo3DamagedInvadersToNotParticipate( TargetSpaceCtx ctx ) {

		// Find Damaged Invaders
		var damagedInvaders = new List<ISpaceEntity>();
		foreach(var token in ctx.Tokens.InvaderTokens().Where( t => t.RemainingHealth < t.FullHealth ))
			for(int i = 0; i < ctx.Tokens[token]; ++i)
				damagedInvaders.Add( token );
		if(damagedInvaders.Count == 0)
			return;

		// Create a list to hold ones we've selected to exclude
		var skipInvaders = new CountDictionary<HumanToken>();
		// Select up to 3 to put in the skip-list
		int remaining = 3;
		while(0 < remaining-- && 0 < damagedInvaders.Count) {
			var decision = new A.SpaceToken(
				"Select invader to not participate in ravage",
				damagedInvaders.Distinct().Cast<IToken>().On( ctx.Space ),
				Present.Done
			);
			var skip = (await ctx.SelectAsync( decision ))?.Token;
			if(skip == null) break;
			skipInvaders[(HumanToken)skip]++;
			damagedInvaders.Remove( skip );
		}

		// If we selected any, remove them from the fight
		if(0 < skipInvaders.Count)
			ctx.Tokens.Adjust(new InvadersDontParticipateInRavage(skipInvaders),1);

	}

	class InvadersDontParticipateInRavage : BaseModEntity, ISkipRavages, IEndWhenTimePasses {
		public UsageCost Cost => UsageCost.Free;
		readonly CountDictionary<HumanToken> _sitOuts;

		public InvadersDontParticipateInRavage(CountDictionary<HumanToken> sitOuts) {
			_sitOuts = sitOuts;
		}

		public Task<bool> Skip( SpaceState space ) {

			Dictionary<HumanToken,HumanToken> restore = new Dictionary<HumanToken, HumanToken>();

			foreach(var original in _sitOuts.Keys ) {
				int count = _sitOuts[original];
				var nonParticipating = original.SetRavageSide( RavageSide.None );
				restore.Add(nonParticipating,original );
				space.Adjust( nonParticipating, count );
				space.Adjust( original, -count );
			}

			ActionScope.Current.AtEndOfThisAction( scope => {
				foreach(var sitOut in restore.Keys) {
					int count = _sitOuts[sitOut];
					space.Adjust( sitOut, -count );
					space.Adjust( restore[sitOut], count );
				}
			} );

			return Task.FromResult(false);
		}
	}
}