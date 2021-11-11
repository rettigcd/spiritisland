using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
	public class RepeatIfAttribute : System.Attribute {

		public IDrawableInnateOption[] Repeats {get; }

		public RepeatIfAttribute(string elementThreshold, params string[] additionalThresholds) {
			var repeats = new List<IDrawableInnateOption> {
				new Drawable( elementThreshold, "Repeat this Power." )
			};
			if(additionalThresholds != null && additionalThresholds.Length>0)
				repeats.AddRange( additionalThresholds.Select( t => new Drawable(t,"Repeat this Power again.") ) );
			this.Repeats = repeats.ToArray();
		}

		public async Task<int> GetRepeatCount( Spirit spirit ) {
			int sum = 0;
			foreach(var repeat in Repeats)
				if( await spirit.HasElements( repeat.Elements ) )
					++sum;
			return sum;
		}

		class Drawable : IDrawableInnateOption {
			public Drawable( string thresholds, string description ) {
				Elements = ElementList.Parse(thresholds);
				Description = description;
			}
			public CountDictionary<Element> Elements { get; }

			public string Description { get; }
		}

	}

}
