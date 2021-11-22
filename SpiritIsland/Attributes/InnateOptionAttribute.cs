using System;
using System.Linq;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class InnateOptionAttribute : Attribute, IDrawableInnateOption {

		public InnateOptionAttribute( string elementText, string description, int group=0 ) {
			Elements = ElementList.Parse( elementText );
			Description = description;
			Purpose = AttributePurpose.DisplayAndExecute;
			Group = group;
		}

		/// <summary>
		/// Use this to not trigger the method call. Just display the power to the user
		/// </summary>
		public InnateOptionAttribute( string elementText, string description, AttributePurpose purpose, int group=0 ) {
			Elements = ElementList.Parse( elementText );
			Description = description;
			Purpose = purpose;
			Group = group;
		}

		public ElementCounts Elements { get; }

		public string Description { get; }

		public AttributePurpose Purpose { get; }

		// Element attributes are evaluated against their group so each group can have 1 method to activate.
		public int Group { get; }

	}

	public enum AttributePurpose { DisplayAndExecute, ExecuteOnly, DisplayOnly }

	public interface IDrawableInnateOption {
		ElementCounts Elements { get; }
		string Description { get; }
	}

}