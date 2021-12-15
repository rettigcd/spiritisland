using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class ShiftingMemoryOfAges : Spirit {

		public const string Name = "Shifting Memory Of Ages";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[]{
			new SpecialRule("Long Ages of Knowledge and Forgetfulness","When you would Forget a Power Card from your hand, you may instead discard it."),
			new SpecialRule("Insights Into the World's Nature","Some of your Actions let you Prepare Element Markers, which are kept here until used.  Choose the Elements freely.  Each Element Marker spent grants 1 of that Element for a single Action.")
		};

		static Track Prepare(int energy){
			return new Track(energy.ToString()+" energy" ){ 
				Energy = energy,
				Action = new PrepareElement( $"{energy} energy" ),
				Icon = new IconDescriptor {
					BackgroundImg = Img.Coin,
					Text = energy.ToString(),
					Sub = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_PrepareEl }
				},
			};
		}

		static Track DiscardElementsForCardPlay => new Track("discard 2 elements for card play" ) { 
			Action = new DiscardElementsForCardPlay(2),
			Icon = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_Discard2Prep },
		};

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
			Growth = new GrowthOptionGroup(
				new GrowthOption(new ReclaimAll(), new PlacePresence(0)),
				new GrowthOption(new DrawPowerCard(), new PlacePresence(2)),
				new GrowthOption(new PlacePresence(1),new GainEnergy(2)),
				new GrowthOption(new GainEnergy( 9 ) )
			);

			InnatePowers = new InnatePower[] {
				InnatePower.For<LearnTheInvadersActions>(),
				InnatePower.For<ObserveTheEverChangingWorld>()
			};

		}

		public override async Task<PowerCard> ForgetPowerCard( Present present = Present.Always ) {
			var options = SingleCardUse.GenerateUses(CardUse.Discard,InPlay.Union( Hand ))
				.Union( SingleCardUse.GenerateUses(CardUse.Forget,DiscardPile) );
			var decision = new Decision.PickPowerCard( "Select card to forget or discard", options, present );
			PowerCard cardToForgetOrDiscard = await this.Action.Decision( decision );
			if(cardToForgetOrDiscard != null)
				Forget( cardToForgetOrDiscard );
			return cardToForgetOrDiscard != null && !DiscardPile.Contains(cardToForgetOrDiscard) 
				? cardToForgetOrDiscard	// card not in discard pile, must have been forgotten
				: null; 
		}

		/// <summary>
		/// If in hand, allows discarding instead of forgetting.
		/// </summary>
		public override void Forget( PowerCard card ) {

			// (Source-1) Purchased / Active
			if(InPlay.Contains( card )) {
				foreach(var el in card.Elements) Elements[el.Key]-=el.Value;// lose elements from forgotten card
				InPlay.Remove( card );
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
			Presence.PlaceOn( space, gameState );
			Presence.PlaceOn( space, gameState );

			// Prepare 1 moon, 1 air, and 1 earth marker.
			PreparedElements[Element.Moon] = 1;
			PreparedElements[Element.Air] = 1;
			PreparedElements[Element.Earth] = 1;
		}

		public async Task PrepareElement(string context) {
			// This is only used by Shifting Memories
			var el = await this.SelectElementEx($"Prepare Element ({context})", ElementList.AllElements);
			PreparedElements[el]++;
		}

		public async Task<ElementCounts> DiscardElements(int totalNumToRemove, string effect ) {
			var discarded = new ElementCounts();

			int index = 0;
			while(index++ < totalNumToRemove) {
				Element el = await this.SelectElementEx($"Select element to discard for {effect} ({index} of {totalNumToRemove})",PreparedElements.Keys, Present.Done);
				if( el == default ) break;
				PreparedElements[el]--;
				discarded[el]++;
			}
			return discarded;
		}

		public override bool CouldHaveElements( ElementCounts subset ) {
			var els = PreparedElements.Any() ? Elements.Union(PreparedElements): Elements;
			return els.Contains(subset);
		}

		public readonly ElementCounts PreparedElements = new ElementCounts();

		protected ElementCounts actionElements; // null unless we are in the middle of an action

		public override async Task<bool> HasElements( ElementCounts subset ) {
			if( actionElements == null ) 
				actionElements = Elements.Clone();
			if( actionElements.Contains( subset ) ) return true;

			// Check if we have prepared element markers to fill the missing elements
			if(PreparedElements.Any()) {
				var missing = subset.Except(actionElements);
				if(PreparedElements.Contains(missing) && await this.UserSelectsFirstText($"Meet elemental threshold:"+subset.ToString(), "Yes, use prepared elements", "No, I'll pass.")) {
					foreach(var pair in missing)
						PreparedElements[pair.Key] -= pair.Value;
					return true;
				}
			}

			return false;
		}

		public override async Task<ElementCounts> GetHighestMatchingElements( IEnumerable<ElementCounts> elementOptions ) {
			if( actionElements == null ) 
				actionElements = Elements.Clone();

			var highestNaturalMatch = elementOptions
				.OrderByDescending(e=>e.Total)
				.FirstOrDefault( els => actionElements.Contains(els) );

			var canMeet = elementOptions
				.OrderBy(e=>e.Total)
				.Where( els => !actionElements.Contains(els) && PreparedElements.Contains(els.Except(actionElements)) )
				.ToArray();

			// If we can't extend with prepared, just return what we can
			if(canMeet.Length == 0)
				return highestNaturalMatch;

			// if we CAN meet something with Prepared, return 
			string prompt = highestNaturalMatch!=null 
				? "Extend element threshold? (current: "+highestNaturalMatch.BuildElementString()+")"
				: "Meet element threshold?";
			var choice = await this.SelectText(prompt,canMeet.Select(e=>e.BuildElementString()).ToArray(),Present.Done);
			var extended = canMeet.FirstOrDefault(e=>choice == e.BuildElementString());
			if(extended != null) {
				foreach(var pair in extended.Except(actionElements))
					PreparedElements[pair.Key] -= pair.Value;
				return extended;
			}
			return highestNaturalMatch;
		}

		public override async Task TakeAction(IActionFactory factory, SelfCtx ctx) {
			actionElements = null; // make sure these are cleared out for every action
			try {
				await base.TakeAction(factory,ctx);
			} finally {
				actionElements = null;
			}
		}

	}


}
