using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class LureOfTheDeepWilderness : Spirit {

		public const string Name = "Lure of the Deep Wilderness";

		public override string Text => Name;

		public override string SpecialRules => "Home of the Island's Heart - Your presence may only be added/moved to lands that are inlind."
			+" Enthrall the Foreign Explorers - For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action";

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
				new GrowthOption(new ReclaimAll(),new GainEnergy(1)),
				new GrowthOption(new PlacePresence(4,Target.Inland)),
				new GrowthOption(new GainElement(Element.Moon,Element.Air,Element.Plant), new GainEnergy(2)),
				new GrowthOption(new DrawPowerCard())
			).Pick(2);


			InnatePowers = new InnatePower[] {
				InnatePower.For<ForsakeSocietyToChaseAfterDreams>(),
				InnatePower.For<NeverHeardFromAgain>()
			};
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 3 presence on your starting board: 2 in land #8, and 1 in land #7.
			Presence.PlaceOn(board[8]);
			Presence.PlaceOn(board[8]);
			Presence.PlaceOn(board[7]);

			// Add 1 beast to land #8
			gameState.Tokens[board[8]].Beasts.Count++;
		}

		public override async Task DoGrowth(GameState gameState) {

			int count = growthOptionGroup.SelectionCount;
			List<GrowthOption> remainingOptions = growthOptionGroup.Options.ToList();
			var allOptions = growthOptionGroup.Options;

			while(count-- > 0) {
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

			await TriggerEnergyElementsAndReclaims();

		}

	}


	[InnatePower("Never Heard from Again"),Slow,FromPresence(0,Target.Inland)]
	[RepeatIf("6 plant")]
	public class NeverHeardFromAgain {


		// If this Power destroys any explorer, 1 Fear
		// if this Power destroys 5 or more explorer, +1 fear
		static int CalcFearFromExplorerDeath( int destroyCount ) {
			return 5 <= destroyCount ? 2
				: 1 <= destroyCount ? 1
				: 0;
		}


		[InnateOption("1 fire,3 air","Add 1 badland",0)]
		static public Task Option1(TargetSpaceCtx ctx ) {
			// add 1 badland
			ctx.Badlands.Count++;
			return Task.CompletedTask;
		}

		[InnateOption("2 plant","Destroy up to 2 explorer per badland/beast/disease/wilds.",1)]
		static public async Task Option2( TargetSpaceCtx ctx ) {
			// 2 plant - destroy up to 2 explorer per badland/beast/disease/wilds
			int destroyCount = await DestroyFromBadlandsBeastDiseaseWilds( ctx );
			ctx.AddFear( CalcFearFromExplorerDeath( destroyCount ) );
		}

		[InnateOption("4 plant,1 animal","2 Damage",1)]
		static public async Task Option3( TargetSpaceCtx ctx ) {
			int preExplorerCount = ctx.Tokens[Invader.Explorer[1]];
			await DestroyFromBadlandsBeastDiseaseWilds( ctx );
			await ctx.DamageInvaders(2);
			int destoryedExplorers = ctx.Tokens[Invader.Explorer[1]] - preExplorerCount;
			ctx.AddFear( CalcFearFromExplorerDeath( destoryedExplorers ) );
		}

		static async Task<int> DestroyFromBadlandsBeastDiseaseWilds( TargetSpaceCtx ctx ) {
			int srcCount = ctx.Badlands.Count + ctx.Beasts.Count + ctx.Disease.Count + ctx.Wilds.Count;
			int destroyCount = Math.Min( srcCount * 2, ctx.Tokens[Invader.Explorer[1]] );
			await ctx.Invaders.Destroy( destroyCount, Invader.Explorer );
			return destroyCount;
		}

		[InnateOption("6 plant","Repeat this Power.",AttributePurpose.DisplayOnly)]
		static public void Nothing() { }

	}

}
