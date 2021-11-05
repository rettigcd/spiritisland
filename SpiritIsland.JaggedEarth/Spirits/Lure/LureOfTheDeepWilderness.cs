using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class LureOfTheDeepWilderness : Spirit {

		public const string Name = "Lure of the Deep Wilderness";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("Home of the Island's Heart", "Your presence may only be added/moved to lands that are inlind."), new SpecialRule("Enthrall the Foreign Explorers", "For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action.") };

		public LureOfTheDeepWilderness():base(
			new SpiritPresence(
				new PresenceTrack(Track.Energy1, Track.Energy2, Track.MoonEnergy, Track.MkEnergy(3,Element.Plant), Track.MkEnergy(4,Element.Air), Track.Energy5Reclaim1 ),
				new PresenceTrack(Track.Card1, Track.Card2, Track.AnimalEnergy, Track.Card3, Track.Card4, Track.Card5Reclaim1)
			)
			,PowerCard.For<GiftOfTheUntamedWild>()
			,PowerCard.For<PerilsOfTheDeepestIsland>()
			,PowerCard.For<SoftlyBeckonEverInward>()
			,PowerCard.For<SwallowedByTheWilderness>()
		) {
			growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption(new ReclaimAll(),new GainEnergy(1)){ AutoSelectSingle = true },
				new GrowthOption(new PlacePresence(4,Target.Inland)){ AutoSelectSingle = true },
				new GrowthOption(new Gain1Element(Element.Moon,Element.Air,Element.Plant), new GainEnergy(2)),
				new GrowthOption(new DrawPowerCard()){ AutoSelectSingle = true }
			).Pick(2);


			InnatePowers = new InnatePower[] {
				InnatePower.For<ForsakeSocietyToChaseAfterDreams>(),
				InnatePower.For<NeverHeardFromAgain>()
			};
		}

		/// <remarks> Overridden so that we can disable 2nd of pair being selected.</remarks>
		public override async Task DoGrowth(GameState gameState) {

			int remainingGrowths = growthOptionGroup.SelectionCount;
			List<GrowthOption> remainingOptions = growthOptionGroup.Options.ToList();
			var allOptions = growthOptionGroup.Options;

			while(remainingGrowths-- > 0) {
				var currentOptions = remainingOptions.Where( o => o.GainEnergy + Energy >= 0 ).ToArray();
				GrowthOption option = (GrowthOption)await this.Select( "Select Growth Option", currentOptions, Present.Always );

				if(option == allOptions[0] || option == allOptions[1] ){
					remainingOptions.Remove( allOptions[0] );
					remainingOptions.Remove( allOptions[1] );
				} else {
					remainingOptions.Remove( allOptions[2] );
					remainingOptions.Remove( allOptions[3] );
				}

				await GrowAndResolve( option, gameState );
			}

			await ApplyRevealedPresenceTracks(gameState);

		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 3 presence on your starting board: 2 in land #8, and 1 in land #7.
			Presence.PlaceOn(board[8]);
			Presence.PlaceOn(board[8]);
			Presence.PlaceOn(board[7]);

			// Add 1 beast to land #8
			gameState.Tokens[board[8]].Beasts.Count++;

			gameState.PreRavaging.ForEntireGame( EnthrallTheForeignExplorers );
		}

		async Task EnthrallTheForeignExplorers( GameState gs,RavagingEventArgs args ) {
			var ravageSpacesWithPresence = args.Spaces.Intersect(this.Presence.Spaces).ToArray();
			foreach(var space in args.Spaces) {
				int maxRemovable = this.Presence.CountOn(space) * 2;
				if( maxRemovable==0 ) continue;
				int explorerCount = gs.Tokens[space][Invader.Explorer.Default];
				if( explorerCount == 0) continue;

				int removableCount = System.Math.Min( maxRemovable, explorerCount );

				int skipCount = await this.SelectNumber($"Enthrall the Foreign Explorers ({explorerCount} on {space.Label}) Ignore how many?", removableCount,0);
				if(skipCount>0)
					gs.ModifyRavage(space,cfg=>cfg.NotParticipating[Invader.Explorer.Default] += skipCount);

			}
		}

	}

}
