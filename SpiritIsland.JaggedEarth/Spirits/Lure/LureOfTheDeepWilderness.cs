using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.Lure {
	public class LureOfTheDeepWilderness : Spirit {

		public const string Name = "Lure of the Deep Wilderness";

		public override string Text => Name;

		public override string SpecialRules => "Home of the Island's Heart - Your presence may only be added/moved to lands that are inlind."
			+" Enthrall the Foreign Explorers - For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action";

		public LureOfTheDeepWilderness():base(
			new SpiritPresence(
				new PresenceTrack(Track.Energy1, Track.Energy2, Track.MoonEnergy, Track.Energy3, Track.Energy4, Track.Energy5), // !!! 3, 4, 5 have secondary
				new PresenceTrack(Track.Card1, Track.Card2, Track.AnimalEnergy, Track.Card5, Track.Card4, Track.Card5Reclaim1)
			)
			,PowerCard.For<GiftOfTheUntamedWild>()
			,PowerCard.For<PerilsOfTheDeepestIsland>()
			,PowerCard.For<SoftlyBeckonEverInward>()
			,PowerCard.For<SwallowedByTheWilderness>()
		) { }

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 3 presence on your starting board: 2 in land #8, and 1 in land #7.
			Presence.PlaceOn(board[8]);
			Presence.PlaceOn(board[8]);
			Presence.PlaceOn(board[7]);

			// Add 1 beast to land #8
			gameState.Tokens[board[8]].Beasts.Count++;
		}
	}


	[InnatePower("Forsake Society to Chase After Dreams"),Slow,FromPresence(1,Target.Invaders)]
	public class ForsakeSocietyToChaseAfterDreams {

		// after this power replaces pieces with explorer, Gather any number of those explorers into your lands.
		// If target land has any town/city remaining, 1 fear.

		[InnateOption("2 moon","Replace 1 explorer with 1 explorer.")]
		public static Task Option1(TargetSpaceCtx ctx ) {
			return Execute(ctx,Invader.Explorer);
		}

		[InnateOption("2 moon,1 air","Instead, replace 1 town with 2 explorer.")]
		public static Task Option2(TargetSpaceCtx ctx ) {
			return Execute(ctx,Invader.Town);
		}

		// 4 moon 2 air 1 animal - instead, replace 1 city with 3 explorer.
		[InnateOption("4 moon,2 air,1 animal","Instaed, replace 1 city with 3 explorer.")]
		public static Task Option3(TargetSpaceCtx ctx ) {
			return Execute(ctx,Invader.City);
		}

		static async Task Execute(TargetSpaceCtx ctx, TokenGroup group) {
			await Dissolve( ctx, group );
			if(RepeatPower( ctx )) {

			}
		}

		private static async Task Dissolve( TargetSpaceCtx ctx, TokenGroup group ) {
			var decision = new Decision.TokenOnSpace( "Select invader to dissolve.", ctx.Space, ctx.Tokens.OfType( group ), Present.Always );
			var token = await ctx.Self.Action.Decision( decision );
			ctx.Tokens[token]--;
			ctx.Tokens[Invader.Explorer.Default] += token.Health;
			await ctx.Pusher
				.AddGroup( token.Health, Invader.Explorer )
				.FilterDestinations( ctx.Self.Presence.Spaces.Contains )
				.MoveUpToN();
		}

		// 4 Repeat this Power
		[InnateOption("4 air","Repeat this Power.",AttributePurpose.DisplayOnly)]
		static bool RepeatPower(TargetSpaceCtx ctx) => ctx.YouHave("4 air");

	}

	// Never Heard From Again
	// slow range 0, inland
	// If this Power destroys any explorer, 1 Fear
	// if this Power destroys 5 or more explorer, +1 fear
	// 1 fire, 3 air- add 1 badland
	// 2 plant - destroy up to 2 explorer per badland/beast/disease/wilds
	// 4 plant 1 animal 2 Damage
	// 6 plant - Repeat this Power


}
