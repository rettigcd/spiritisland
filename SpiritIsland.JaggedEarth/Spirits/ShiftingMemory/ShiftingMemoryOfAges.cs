using System;

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


		public ShiftingMemoryOfAges() 
			:base(
				new SpiritPresence(
					new PresenceTrack(Track.Energy0,Track.Energy1,Track.Energy2,Prepare(3),Track.Energy4,Track.Reclaim1Energy,Track.Energy5,Prepare(6)),
					new PresenceTrack(Track.Card1,Track.Card2,Track.Card2,Track.PlantEnergy,Track.Card3) // !!! discard 2 element markers for +1 card play
				),
				PowerCard.For<BoonOfAncientMemories>(),
				PowerCard.For<ElementalTeachings>(),
				PowerCard.For<ShareSecretsOfSurvival>(),
				PowerCard.For<StudyTheInvadersFears>()
			) 
		{
			// Innates
			// Growth
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 2 presence on your starting board in the highest-number land that is Sands or Mountain.
			// Prepare 1 moon, 1 air, and 1 earth marker.
			PreparedElements[Element.Moon] = 1;
			PreparedElements[Element.Air] = 1;
			PreparedElements[Element.Earth] = 1;
		}

	}

	// Challenges / Tasks

	// 3: When showing Fast / Slow Actions, check if spending a marker would allow triggering an innate.
		// If there is an innate that runs this speed, and if there is a tier we can reach by spending markers.

	// 4: create Discard 2 elements for +1 card play action.

	// 5: Convert Any-Element to use this Ask-When-You-Need-It instead of force choosing.

	// 6: Draw Prepare (energy) track
	// 7: Draw Discard 2 element markers for +1 card play
}
