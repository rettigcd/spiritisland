﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpiritIsland {

	public class Growth {

		#region constructors

		public Growth( params GrowthOption[] options ) {
			groups.Add( new GrowthOptionGroup(1,options));
		}

		public Growth( int pick, params GrowthOption[] options ) {
			groups.Add( new GrowthOptionGroup(pick,options));
		}

		public Growth Add( GrowthOptionGroup grp ) {
			groups.Add(grp);
			return this;
		}

		#endregion

		public ReadOnlyCollection<GrowthOptionGroup> Groups => groups.AsReadOnly();

		public GrowthOption[] Options => groups.SelectMany(g=>g.Options).ToArray();

		public virtual IGrowthPhaseInstance GetInstance() => new GrowthPhaseInstance( groups.ToArray() );

		#region private

		readonly List<GrowthOptionGroup> groups = new List<GrowthOptionGroup>();

		#endregion

	}

	public interface IGrowthPhaseInstance {
		GrowthOption[] RemainingOptions(int energy);
		void MarkAsUsed( GrowthOption option );
	}

	public class GrowthOptionGroup {
		public int SelectCount { get; }
		public GrowthOption[] Options { get; private set; }
		public GrowthOptionGroup(int selectCount, params GrowthOption[] options ) {
			SelectCount = selectCount;
			Options = options;
		}
		public void Add( GrowthOption option ) { // hook for Starlight
			var options = Options.ToList();
			options.Add(option);
			Options = options.ToArray();
		}
	}

	class GrowthPhaseInstance : IGrowthPhaseInstance {

		public GrowthPhaseInstance( params GrowthOptionGroup[] gogs ) {
			remaining = gogs
				.Select(g=>new GOGRemaining( g ))
				.ToArray();
		}

		public GrowthOption[] RemainingOptions(int energy)
			=> remaining.Where(g=>g.count>0)
				.SelectMany(g=>g.options)
				.Where( o => o.GainEnergy + energy >= 0 )
				.ToArray();

		public void MarkAsUsed( GrowthOption option ) {
			var grp = remaining.First(grp=>grp.options.Contains(option));
			grp.options.Remove( option );
			--grp.count;
		}

		#region private

		readonly GOGRemaining[] remaining;

		#endregion

		class GOGRemaining {
			public GOGRemaining(GrowthOptionGroup grp ) {
				options = grp.Options.ToList();
				count = grp.SelectCount;
			}
			readonly public List<GrowthOption> options;
			public int count;
		}

	}


}
