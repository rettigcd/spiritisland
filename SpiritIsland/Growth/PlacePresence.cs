﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

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

		public override IOption[] Options => spirit.Presence
			.SelectMany(s => s.SpacesWithin(this.range))
			.Distinct()
			.Where(SpaceIsValid)
			.OrderBy(x=>x.Label)
			.ToArray();

		public override void Select( IOption option ) {
			this.Target = (Space)option;
			this.Source = Track.Energy; // !!!!!!!!!!!!!!!!!!!!!!!!!  need to select this also
		}

		bool SpaceIsValid(Space space) => isValid(space,gameState);

	}

	public class ResolvePlacePresence : IResolver {

		readonly string placeOnSpace;
		readonly Track source;
		readonly int focus;

		public ResolvePlacePresence(string placeOnSpace, int focus, Track source) {
			this.placeOnSpace = placeOnSpace;
			this.source = source;
			this.focus = focus;
		}

		public void Apply(List<IAction> growthActions) {
			var placePresence = growthActions
				.OfType<PlacePresenceBase>()
				.ToArray()[focus];
			Update(placePresence);
			placePresence.Apply();
		}

		protected virtual void Update(PlacePresenceBase pp) {
			pp.Target = (Space)pp.Options.Single(s=>s.Text==placeOnSpace);
			pp.Source = source;
		}
	}

}
