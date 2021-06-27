using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class PlacePresence : PlacePresenceBase {

		readonly int range;
		readonly Func<Space,GameState, bool> isValid;

		public override string ShortDescription {get;}

		#region constructors

		public PlacePresence( int range ){
			static bool IsNotOcean(Space s,GameState _) => s.IsLand;
			this.range = range;
			isValid = IsNotOcean;
			ShortDescription = $"PlacePresence({range})";
		}

		public PlacePresence(
			int range,
			Func<Space, GameState, bool> isValid,
			string funcDescriptor
		){
			this.range = range;
			this.isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
			ShortDescription = $"PlacePresence({range},{funcDescriptor})";
		}

		#endregion

		public override IOption[] Options { get {
			return Source == default
				? (new Track[]{Track.Energy,Track.Card})
				: (IOption[])spirit.Presence
					.SelectMany(s => s.SpacesWithin(this.range))
					.Distinct()
					.Where(SpaceIsValid)
					.OrderBy(x=>x.Label)
					.ToArray();
			}
		}

		public override void Select( IOption option ) {
			if(Source == default){
				this.Source = (Track)option;
				return;
			}
			this.Target = (Space)option;
		}

		bool SpaceIsValid(Space space) => isValid(space,gameState);

	}

}
