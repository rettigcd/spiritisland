using System;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class FleeFromDangerousLands : IFearOptions {

		public const string Name = "Flee from Dangerous Lands";

		[FearLevel(1, "On Each Board: Push 1 Explorer / Town from a land with Badlands / Wilds / Dahan." )]
		public Task Level1( FearCtx ctx ) {

			// On Each Board
			return GameCmd.OnEachBoard(
				BoardCmd.PickSpaceThenTakeAction("Remove 1 Explorere / Town from a land with Badlands / Wilds / Dahan."
					// Remove 1 Explorer / Town
					, Cmd.RemoveExplorersOrTowns(1)
					// from a land with Badlands / Wilds / Dahan.
					,FindBadlandsWildsOrDahanSpaces( ctx )
				)
			).Execute( ctx.GameState );

		}

		[FearLevel(2, "On Each Board: Remove 1 Explorer / Town from a land with Badlands / Wilds / Dahan." )]
		public Task Level2( FearCtx ctx ) {

			// On Each Board
			return GameCmd.OnEachBoard(
				BoardCmd.PickSpaceThenTakeAction("Remove 1 Explorere / Town from a land with Badlands / Wilds / Dahan."
					// Remove 2 Explorer / Town
					, Cmd.RemoveExplorersOrTowns(2)
					// from a land with Badlands / Wilds / Dahan.
					,FindBadlandsWildsOrDahanSpaces( ctx )
				)
			).Execute( ctx.GameState );

		}

		[FearLevel(3, "On Each Board: Remove 1 Explorer / Town from any land, or Remove 1 City from a land with Badlands / Wilds / Dahan." )]
		public async Task Level3( FearCtx ctx ) {

			// Remove 1 Explorer / Town from any land
			var removeExplorerOrTownFromAnyLand = BoardCmd.PickSpaceThenTakeAction("Remove 1 Explorere / Town from any land"
				, Cmd.RemoveExplorersOrTowns(1)
				, _ => true
			);

			// Remove 1 City from a land with Badlands / Wilds / Dahan.
			var removeCity = BoardCmd.PickSpaceThenTakeAction("Remove 1 City from a land with Badlands / Wilds / Dahan."
				,Cmd.RemoveCities(1)
				,FindBadlandsWildsOrDahanSpaces( ctx )
			);

			await GameCmd.OnEachBoard(Cmd.Pick1( removeExplorerOrTownFromAnyLand, removeCity )).Execute( ctx.GameState );

		}

		static Func<Space,bool> FindBadlandsWildsOrDahanSpaces( FearCtx ctx ) {
			return (space) => {
				var tokens = ctx.GameState.Tokens[space];
				return tokens.Badlands.Any || tokens.Wilds.Any || tokens.Dahan.Any;
			};
		}

	}

}
