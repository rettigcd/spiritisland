
namespace SpiritIsland.Basegame;

[InnatePower("Reclaimed by the Deeps"), Slow]
[FromPresence(1, Filter.Deeps)]
public class ReclaimedByTheDeeps {


	[InnateTier("2 water", "1 Damage per Deeps to Town/City only.")]
	static public Task DeepsDamage1(TargetSpaceCtx ctx) {
		// 1 Damage per Deeps to Town/City only.
		return ctx.DamageInvaders(ctx.Space[Token.Deeps]);
	}

	[InnateTier("4 water,2 earth", "If at least 3 deeps and no Town/City are present: Drown all Explorer/Dahan.  Push all but 2 Deeps. Target land permanently becomes the Ocean on its board.",1)]
	static public async Task Make3DeepsAnOcean(TargetSpaceCtx ctx) {
		// If at least 3 deeps and no Town/City are present:
		int deepsCount = ctx.Space[Token.Deeps];
		if( 3 <= deepsCount && !ctx.Space.HasAny( Human.Town_City ) ) {

			// Drown all Explorer/Dahan.
			var drowner = Drowning.GetDrowner();
			foreach(var token in ctx.Space.HumanOfAnyTag(Human.Dahan,Human.Explorer).ToArray())
				await drowner.Drown(token.On(ctx.Space), ctx.Space[token]);

			// Push all but 2 Deeps.
			ctx.Space.Adjust(Token.Deeps,-2); deepsCount -= 2;
			await ctx.Push(deepsCount,Token.Deeps);

			// Target land permanently becomes the Ocean on its board.
			MakeOcean(ctx.Space);
		}
	}

	[InnateTier("2 moon,3 water", "Repeat this power in target land or in a land with your Presense.",2)]
	static public async Task RepeatInLandWithPresence(TargetSpaceCtx ctx) {

		// in target land or in a land with your Presense.
		List<Space> spaces = [ctx.Space, .. ctx.Self.Presence.Lands];
		var options = spaces
			.Where(s=>{ 
				int deeps = s[Token.Deeps];
				int townCityCount = s.SumAny(Human.Town_City);
				return 0 < townCityCount && 0<deeps // would trigger Option 1
					|| townCityCount == 0 && 3<=deeps;  // would trigger Option 2
			});
			
		var repeatSpace = await ctx.Self.Select("Repeate power", options, Present.Done);
		if(repeatSpace is null ) return;

		// Repeat this power
		var newCtx = ctx.Target(repeatSpace);
		await DeepsDamage1(newCtx);
		await Make3DeepsAnOcean(newCtx);
	}

	static void MakeOcean(Space space) {
		SpaceSpec spaceSpec = space.SpaceSpec;
		SingleSpaceSpec ocean = spaceSpec.Boards[0].Ocean;

		// Update boundaries
		ocean.SetLayout( MultiSpaceSpec.CalcLayout([ocean, space.SpaceSpec]) );

		// Add adjacents to ocean
		ocean.SetAdjacentToSpaces(space.SpaceSpec.Adjacent.ToArray());

		// Remove old from board
		spaceSpec.RemoveFromBoard();

		ActionScope.Current.Log(new Log.LayoutChanged($"{space.Label} sunk into the Deeps."));
	}

}
