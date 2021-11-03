﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {

	public class ShiftingMemoryOfAges : Spirit {

		public const string Name = "Shifting Memory Of Ages";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[]{
			new SpecialRule("Long Ages of Knowledge and Forgetfulness","When you would Forget a Power Card from your hand, you may instead discard it."),
			new SpecialRule("Insights Into the World's Nature","Some of your Actions let you Prepare Element Markers, which are kept here until used.  Choose the Elements freely.  Each Element Marker spent grants 1 of that Element for a single Action.")
		};

		static Track Prepare(int energy){
			var t = Track.MkEnergy( energy );
			t.Action = new PrepareElement();
			return t;
		}

		static Track DiscardElementsForCardPlay => new Track("discard 2 elements for card play" ) { Action = new DiscardElementsForCardPlay(2) };

		public ShiftingMemoryOfAges() 
			:base(
				new SpiritPresence(
					new PresenceTrack(Track.Energy0,Track.Energy1,Track.Energy2,Prepare(3),Track.Energy4,Track.Reclaim1Energy,Track.Energy5,Prepare(6)),
					new PresenceTrack(Track.Card1,Track.Card2,Track.Card2,DiscardElementsForCardPlay,Track.Card3)
				),
				PowerCard.For<BoonOfAncientMemories>(),
				PowerCard.For<ElementalTeachings>(),
				PowerCard.For<ShareSecretsOfSurvival>(),
				PowerCard.For<StudyTheInvadersFears>()
			) 
		{
			growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption(new ReclaimAll(), new PlacePresence(0)),
				new GrowthOption(new DrawPowerCard(), new PlacePresence(2)),
				new GrowthOption(new PlacePresence(1),new GainEnergy(2)),
				new GrowthOption(new GainEnergy(9))
			);

			InnatePowers = new InnatePower[] {
				InnatePower.For<LearnTheInvadersActions>(),
				InnatePower.For<ObserveTheEverChangingWorld>()
			};
		}

		public override void Forget( PowerCard card ) {

			// (Source-1) Purchased / Active
			if(PurchasedCards.Contains( card )) {
				foreach(var el in card.Elements) Elements[el.Key]-=el.Value;// lose elements from forgotten card
				PurchasedCards.Remove( card );
				DiscardPile.Add( card );
				return;
			} 

			if(Hand.Contains( card )) {
				Hand.Remove( card );
				DiscardPile.Add( card );
				return;
			}

			if(DiscardPile.Contains( card )) {
				base.Forget( card );
				return;
			}

			throw new System.Exception("Can't find card to forget:"+card.Name);
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 2 presence on your starting board in the highest-number land that is Sands or Mountain.
			var space = board.Spaces.Last(x=>x.Terrain.IsOneOf(Terrain.Sand,Terrain.Mountain));
			Presence.PlaceOn( space );
			Presence.PlaceOn( space );

			// Prepare 1 moon, 1 air, and 1 earth marker.
			PreparedElements[Element.Moon] = 1;
			PreparedElements[Element.Air] = 1;
			PreparedElements[Element.Earth] = 1;
		}

	}


}
