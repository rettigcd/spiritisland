using System;
using System.Linq;
using System.Reflection;

namespace SpiritIsland {

	/// <summary> Text is where the fear card is currently located </summary>
	public class PositionFearCard : IOption {

		/// <summary> The location of the card in the deck, active stack, discard etc. (so user can pick which to see </summary>
		public string Text { get; set; }
		public IFearOptions FearOptions { get; set; }

	}

	/// <summary> Text is Name : Terror Level : Instructions </summary>
	public class ActivatedFearCard : IOption {

		public ActivatedFearCard( IFearOptions options ) {
			FearOptions = options;
			Name = GetFearCardName( options );
			Text = Name;
		}

		public ActivatedFearCard(IFearOptions options, int activation ) {
			FearOptions = options ?? throw new ArgumentNullException(nameof(options));
			Name = GetFearCardName( options );

			var memberName = "Level"+activation;

			// This does not find interface methods declared as: void IFearCardOption.Level2(...)
			var member = FearOptions.GetType().GetMethod(memberName) 
				?? throw new Exception(memberName +" not found on "+options.GetType().Name);

			var attr = (FearLevelAttribute)member.GetCustomAttributes(typeof( FearLevelAttribute )).Single();

			Text = $"{Name} : {activation} : {attr.Description}";
		}

		public string Name { get; }

		public string Text { get; }

		public IFearOptions FearOptions { get; }

		static public string GetFearCardName( IFearOptions card ) {
			Type type = card.GetType();
			return (string)type.GetField( "Name" ).GetValue( null );
		}

	}

}
