using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

    public partial class SpiritControl : Control {

		public SpiritControl() {
			InitializeComponent();
			this.BackColor = Color.LightYellow;
			this.Cursor = Cursors.Default;
		}

		public void Init( Spirit spirit, string presenceColor, IHaveOptions optionProvider ) {
			this.spirit = spirit;

			this.presenceColor = presenceColor ?? throw new ArgumentNullException(nameof(presenceColor));

			elPos = new Dictionary<Element, int>();
			int i=0;
			foreach(var innate in spirit.InnatePowers)
				foreach(var el in Highest(innate))
					if(!elPos.ContainsKey(el)) elPos[el] = i++;
			foreach(Element el in Enum.GetValues(typeof(Element)) )
				if(!elPos.ContainsKey( el )) elPos[el] = i++;

			optionProvider.OptionsChanged += OptionProvider_OptionsChanged;
		}
		Dictionary<Element, int> elPos;

		static Element[] Highest(InnatePower power) => power.GetTriggerThresholds()
            .OrderByDescending(list=>list.Length)
            .First();

        void OptionProvider_OptionsChanged( IOption[] obj ) {
			var tracks = obj.OfType<Track>().ToArray();
			trackOptions = new HashSet<Track>( tracks );
			innateOptions = obj
				.OfType<IActionFactory>()
				.Select( x => x.Original )
				.OfType<InnatePower>()
				.ToArray();
			this.Invalidate();
		}

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(spirit != null)
				DrawSpirit( pe.Graphics );
		}

		protected override void OnSizeChanged( EventArgs e ) {
			base.OnSizeChanged( e );
			this.Invalidate();
		}

		void DrawSpirit( Graphics graphics ) {

			using Font simpleFont = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );

//			using Font font = new( "Arial", 18f );
			using Brush coverBrush = new SolidBrush( Color.FromArgb( 128, Color.Gray ) );
			Brush currentBrush = Brushes.Yellow;
			using Pen highlightPen = new( Color.Red, 10f );

			// Load Presence image
			using var presenceStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.presence.{presenceColor}.png" );
			using Bitmap presence = new Bitmap( presenceStream );

			// calc slot width and presence height
			int maxLength = Math.Max( spirit.GetCardSequence().Length, spirit.EnergyTrack.Length );
			float coinWidth = (Width - 2 * margin) / maxLength;
			float presenceHeight = coinWidth * presence.Height / presence.Width;

			// Energy
			float x, y=10f;
			DrawEnergy( graphics, simpleFont, presence, coinWidth, presenceHeight, ref y );

			// Cards
			DrawCards( graphics, simpleFont, presence, coinWidth, presenceHeight, ref y );

			y += margin;

			// Innates
			x = DrawInnates( graphics, highlightPen, ref y );

			// activated elements
			ActivatedElements( graphics, simpleFont, ref y );

			// !Note! - If you do not specify output width/height of image, .Net will scale image based on screen DPI and image DPI
		}


		void DrawEnergy( Graphics graphics, Font simpleFont, Bitmap presence, float coinWidth, float presenceHeight, ref float y ) {

			float x = margin;

			// Title
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, x, y );
			y += lineHeight;

			int revealedEnergySpaces = spirit.RevealedEnergySpaces;
			int idx = 0;

			// bool highlightEnergy = trackOptions.Contains( Track.Energy1 );

			foreach(var energy in spirit.EnergyTrack) {
				
				// energy amount
				using( var imgStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.tokens.{energy.Text}.png" ) ){
					using var bitmap = new Bitmap( imgStream ); 
					graphics.DrawImage( bitmap, x, y, coinWidth, coinWidth );
				};

				// presence
				if(revealedEnergySpaces <= idx )
					graphics.DrawImage( presence, x, y-presenceHeight/2,coinWidth,presenceHeight );

				x += coinWidth;
				++idx;
			}

			y += coinWidth;
		}

		void DrawCards( Graphics graphics, Font simpleFont, Bitmap presence, float slotWidth, float presenceHeight, ref float y ) {

			float x = margin;

			// draw title
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, x, y );
			y += lineHeight;

			float maxCardHeight = 0;
			float cardWidth = slotWidth * 0.8f;
			float cardLeft = (slotWidth - cardWidth) / 2; // center

			int revealedCardSpaces = spirit.RevealedCardSpaces;
			int idx = 0;

//			bool highlightCard = trackOptions.Contains( Track.Card1 );

			foreach(int en in spirit.GetCardSequence()) {

				// card plays amount
				using(var imgStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.tokens.{en} cardplay.png" )) {
					using var bitmap = new Bitmap( imgStream );
					var cardHeight = cardWidth * bitmap.Height / bitmap.Width;
					maxCardHeight = Math.Max(cardHeight,maxCardHeight);
					graphics.DrawImage( bitmap, x+ cardLeft, y, cardWidth, cardHeight );
				};

				// presence
				if(revealedCardSpaces <= idx)
					graphics.DrawImage( presence, x, y - presenceHeight / 2, slotWidth, presenceHeight );

				x += slotWidth;
				++idx;
			}

			y += maxCardHeight;

		}

		float DrawInnates( Graphics graphics, Pen highlightPen, ref float y ) {
			float x = margin;

			// This non-sense is because Thunderspeaker has a fast & slow option with the same name.
			string[] innateOptionNames = innateOptions.Select( x => x.Name ).ToArray();
			foreach(var name in spirit.InnatePowers.Select( i => i.Name ).Distinct()) {
				var image = GetInnateImage( name );

				int drawWidth = Width - (int)x * 2;
				int drawHeight = drawWidth * image.Height / image.Width;
				graphics.DrawImage( image, x, y, drawWidth, drawHeight );

				if(innateOptionNames.Contains( name ))
					graphics.DrawRectangle( highlightPen, x, y, drawWidth, drawHeight );

				y += drawHeight;
				y += 10;
			}

			return x;
		}



		void ActivatedElements( Graphics graphics, Font simpleFont, ref float y ) {
			y += 20;
			const float elementSize = 50f;
			var elements = spirit.Elements; // cache, don't recalculate
			float x = margin;

			var orderedElements = elements.Keys.OrderBy( el => elPos[el] );
			foreach(var element in orderedElements) {
				if(elements[element] > 1) {
					graphics.DrawString(
						elements[element].ToString(),
						simpleFont,
						Brushes.Black
						, x, y
					);
					x += 20;
				}
				graphics.DrawImage( GetImage( element ), x, y, elementSize, elementSize );
				x += elementSize;
				x += 10;
			}
		}


		Image GetInnateImage( string innateCardName ) {
			if(!innateImages.ContainsKey( innateCardName )) {
				string filename = innateCardName.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
				Image image = Image.FromFile( $".\\images\\innates\\{filename}.jpg" );
				innateImages.Add( innateCardName, image );
			}
			return innateImages[innateCardName];
		}

		Image GetImage( Element element ) {

			if(!elementImages.ContainsKey( element )) {
				string filename = "Simple_" + element.ToString().ToLower();
				Image image = Image.FromFile( $".\\images\\tokens\\{filename}.png" );
				elementImages.Add( element, image );
			}
			return elementImages[element];
		}

		#region private fields

		readonly Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
		const float margin = 10f;
		const float lineHeight = 60f;

		string presenceColor;
		HashSet<Track> trackOptions;
		InnatePower[] innateOptions;
		Spirit spirit;

		readonly Dictionary<string, Image> innateImages = new();
		readonly Dictionary<Element, Image> elementImages = new();
		#endregion

	}
}
