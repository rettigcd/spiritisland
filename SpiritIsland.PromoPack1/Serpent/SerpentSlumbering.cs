﻿using System.Linq;

namespace SpiritIsland.PromoPack1 {

	public class SerpentSlumbering : Spirit {

		public SerpentSlumbering() :base (
			new SerpentPresence()
			,PowerCard.For<ElementalAegis>()
			,PowerCard.For<AbsorbEssence>()
			,PowerCard.For<GiftOfFlowingPower>()
			,PowerCard.For<GiftOfThePrimordialDeeps>()	
		) {
			InnatePowers = new InnatePower[] {
				InnatePower.For<SerpentWakesInPower>(),
				InnatePower.For<SerpentRousesInAnger>()
			};

			growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption( new ReclaimAll(), new MovePresence() ),
				new GrowthOption( new DrawPowerCard(), new GainEnergy(1) ),
				new GrowthOption( new GainEnergy(4) ),
				new GrowthOption( new PlacePresence(3,Target.NoBlight) )
			).Pick(2);

		}

		public const string Name = "Serpent Slumbering Beneath the Island";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[] {
			new SpecialRule("Deep Slumber","You start off limited to 5 presence on the tisland.  Raise this with your 'Absorb Essence' Power Card.  Each use covers the lowest revealed number; your presence limit is the lowest uncovered number.")
		} ;

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Setup: put 1 presence on #5
			Presence.PlaceOn(board[5], gameState);
		}

	}

}
