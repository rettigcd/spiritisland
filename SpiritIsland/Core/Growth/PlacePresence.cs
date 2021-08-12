using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class PlacePresence : GrowthActionFactory {

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

		public override Task Activate( ActionEngine engine ) {

			bool SpaceIsValid(Space space) => isValid(space,engine.GameState);

			var options = engine.Self.Presence
				.SelectMany(s => s.SpacesWithin(this.range))
				.Distinct()
				.Where(SpaceIsValid)
				.OrderBy(x=>x.Label)
				.ToArray();

			return ActAsync( engine, options );
		}

		static public async Task ActAsync( ActionEngine engine, Space[] destinationOptions ) {
			// From
			var from = await engine.SelectTrack();

			// To
			var to = await engine.SelectSpace( "Where would you like to place your presence?", destinationOptions );

			// from
			if(from == engine.Self.NextEnergyPresence)
				engine.Self.RevealedEnergySpaces++;
			else if(from == engine.Self.NextCardPresence)
				engine.Self.RevealedCardSpaces++;
			else
				throw new ArgumentException(from.ToString());

			// To
			engine.Self.Presence.Add( to );
		}


	}

}
