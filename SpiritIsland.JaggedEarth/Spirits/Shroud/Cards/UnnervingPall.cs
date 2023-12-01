namespace SpiritIsland.JaggedEarth;

public class UnnervingPall {

	[SpiritCard("Unnerving Pall",1,Element.Moon,Element.Air,Element.Animal), Fast, FromPresence(0,Filter.Invaders)]
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
		await ctx.SourceSelector
			.AddGroup(3,Human.Invader)
			.FilterSpaceToken(st => 0<((HumanToken)st.Token).Damage ) // is damaged
			.SelectFightersAndSitThemOut(ctx.Self);
	}

	class InvadersDontParticipateInRavage : BaseModEntity, IConfigRavages, IEndWhenTimePasses {

		readonly CountDictionary<HumanToken> _sitOuts;

		public InvadersDontParticipateInRavage(CountDictionary<HumanToken> sitOuts) {
			_sitOuts = sitOuts;
		}

		void IConfigRavages.Config( SpaceState space ) {

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

		}
	}
}