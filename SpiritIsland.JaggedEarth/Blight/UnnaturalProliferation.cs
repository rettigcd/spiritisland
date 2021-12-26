using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class UnnaturalProliferation : BlightCardBase {

		public UnnaturalProliferation():base("Unnatural Proliferation",3) {}

		protected override async Task BlightAction( GameState gs ) {
			// Immediately:  Each spirit
			await GameCmd.EachSpirit(Cause.Blight,
				// adds 1 presence to a land with their prescense.
				AddPresenceToOwnLand
			).Execute( gs );

			// On each board:
			await GameCmd.OnEachBoard( Cmd.Multiple(
				// Add 1 dahan to a land with dahan, and
				AddDahanToDahanLand,
				// 2 cities to the land with fewest town/city (min.1)
				Add2CitiesToLandWithFewest
			) ).Execute( gs );
		}


		static SelfAction AddPresenceToOwnLand => new SelfAction(
			"Add 1 presence to a land with own presence.", 
			ctx=> ctx.Presence.Place( ctx.Self.Presence.Spaces.ToArray() )
		);

		static ActionOption<BoardCtx> AddDahanToDahanLand => BoardCmd.PickSpaceThenTakeAction( 
			"Add 1 dahan to a land with dahan.",
			Cmd.AddDahan(1),
			tokens => tokens.Dahan.Any
		);

		static ActionOption<BoardCtx> Add2CitiesToLandWithFewest => new ActionOption<BoardCtx>(
			"Add 2 cities to the land with fewest town/city.", async ctx => {
				var spaceOptions = ctx.Board.Spaces
					.GroupBy( s=>ctx.GameState.Tokens[s].SumAny(Invader.Town,Invader.City) )
					.OrderBy( grp => grp.Key )
					.First()
					.ToArray();
				var space = await ctx.SelectSpace("Add 2 cities",spaceOptions);
				await space.Tokens.Add(Invader.City.Default, 2, AddReason.Added);
			}
		);

	}

}
