namespace SpiritIsland.NatureIncarnate;

public class PlagueShipsSailToDistantPorts {

	public const string Name = "Plague Ships Sail to Distant Ports";

	[MajorCard(Name,4,"fire,air,water,animal"),Fast]
	[FromPresence(1,Filter.CoastalCity)]
	[Instructions( "1 Fear. Add 4 Disease among Coastal lands other than target land.","2 fire,2 water,2 animal","Instead: 1 Fear. 3 Damage. Spirits may jointly spend 3 Energy/player (aided by removing Disease for 3 Energy each) to remove the top Fear Card." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 1 Fear.
		ctx.AddFear(1);

		await Cmd.Pick1( 
			AddDisease, 
			PayToRemoveFearCard.OnlyExecuteIf(await ctx.YouHave("2 sun,2 water,2 animal") && CanAfford() )
		).ActAsync(ctx);

	}

	static SpaceAction AddDisease => new SpaceAction("Add Disease", async ctx => {
		// Add 4 Disease among Coastal lands other than target land.
		SpaceState[] options = GameState.Current.Spaces.Where(s=>s.Space.IsCoastal && s.Space != ctx.Space).ToArray();
		for(int i=0;i<4;++i) {
			Space space = await ctx.Self.SelectAsync(new A.Space($"Add Disease ({i+1} of 4)",options,Present.Always));
			if(space != null)
				await space.Tokens.Disease.AddAsync(1);
		}
	} );

	#region Element Threshold

	static SpaceAction PayToRemoveFearCard => new SpaceAction("Spend 3 Energy/Player (aided by Disease) to remove Fear Card", PayToRemoveFearCard_Imp );

	static async Task PayToRemoveFearCard_Imp(TargetSpaceCtx ctx ) {
		// 3 Damage.
		await ctx.DamageInvaders(3);
		// Spirits may jointly spend 3 Energy/player (aided by removing Disease for 3 Energy each) to remove the top Fear Card.
		var gs = GameState.Current;
		int cost = gs.Spirits.Length * 3;
		while(0 < cost) {
			IEnumerable<SpaceToken> diseaseTokens = Token.Disease.On(gs.Spaces);
			IEnumerable<Spirit> spiritsWithEnergy = gs.Spirits.Where(s=>0<s.Energy);
			// !!! Spirits should decide for themselves if they want pay, not card player
			IOption option = await ctx.Self.SelectAsync(new A.TypedDecision<IOption>(
				$"Pay {cost} to remove Fear Card (1/Spirit 3/Disease)",
				diseaseTokens.Cast<IOption>().Union(spiritsWithEnergy.Cast<IOption>()),
				Present.Done
			));
			if(option == null) break; // too bad if they already spent it.
			if(option is Spirit s) {
				--s.Energy;
				--cost;
			} else if (option is SpaceToken st) {
				await st.Remove();
				cost -= 3;
			}
		}
		
		if(cost == 0)
			gs.Fear.Deck.Pop();

	}

	static bool CanAfford() {
		var gs = GameState.Current;
		return gs.Spaces.Sum(s=>s.Disease.Count)*3 + gs.Spirits.Sum(s=>s.Energy) <= 3 * gs.Spirits.Length;
	}

	#endregion

}
