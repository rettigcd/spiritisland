using SpiritIsland;
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

			this.presenceColor = presenceColor ?? throw new ArgumentNullException( nameof( presenceColor ) );

			InitElementDisplayOrder( spirit );

			optionProvider.OptionsChanged += OptionProvider_OptionsChanged;
		}

		void InitElementDisplayOrder( Spirit spirit ) {
			static Element[] Highest( InnatePower power ) => power.GetTriggerThresholds()
				.OrderByDescending( list => list.Length )
				.First();

			elementOrder = new Dictionary<Element, int>();
			int i = 0;
			foreach(var innate in spirit.InnatePowers)
				foreach(var el in Highest( innate ))
					if(!elementOrder.ContainsKey( el )) elementOrder[el] = i++;
			foreach(Element el in Enum.GetValues( typeof( Element ) ))
				if(!elementOrder.ContainsKey( el )) elementOrder[el] = i++;
		}

		Dictionary<Element, int> elementOrder;

        void OptionProvider_OptionsChanged( IOption[] obj ) {

			trackOptions = obj.OfType<Track>().ToArray();

			innateOptions = obj
				.OfType<IActionFactory>()
				.Select( x => x.Original )
				.OfType<InnatePower>()
				.ToArray();

			this.Invalidate();
		}

		protected override void OnSizeChanged( EventArgs e ) {
			base.OnSizeChanged( e );
			this.Invalidate();
		}

		#region Draw

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(spirit != null)
				DrawSpirit( pe.Graphics );
		}

		void DrawSpirit( Graphics graphics ) {

			hotSpots.Clear();

			using Font simpleFont = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );

			using Brush coverBrush = new SolidBrush( Color.FromArgb( 128, Color.Gray ) );
			Brush currentBrush = Brushes.Yellow;
			using Pen highlightPen = new( Color.Red, 10f );

			// Load Presence image
			using var presenceStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.presence.{presenceColor}.png" );
			using Bitmap presence = new Bitmap( presenceStream );

			// calc slot width and presence height
			int maxLength = Math.Max( spirit.Presence.CardPlays.TotalCount, spirit.Presence.Energy.TotalCount );
			float coinWidth = (Width - 2 * margin) / maxLength;
			float presenceWidth = coinWidth * 0.9f;
			SizeF presenceSize = new SizeF(presenceWidth, presenceWidth * presence.Height / presence.Width );

			// Energy
			float x, y=10f;
			DrawEnergyTrack( graphics, simpleFont, presence, coinWidth, presenceSize, highlightPen, ref y );

			// Cards
			DrawCardPlayTrack( graphics, simpleFont, presence, coinWidth, presenceSize, highlightPen, ref y );

			y += margin;

			// Innates
			x = DrawInnates( graphics, highlightPen, ref y );

			// activated elements
			DrawActivatedElements( graphics, simpleFont, ref y );

			// !Note! - If you do not specify output width/height of image, .Net will scale image based on screen DPI and image DPI
		}

		void DrawEnergyTrack( Graphics graphics, Font simpleFont, Bitmap presence, float slotWidth, SizeF presenceSize, Pen highlightPen, ref float y ) {

			float x = margin;

			// Title
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, x, y );
			y += lineHeight;

			int revealedEnergySpaces = spirit.Presence.Energy.RevealedCount;
			int idx = 0;

			// bool highlightEnergy = trackOptions.Contains( Track.Energy1 );

			foreach(var energy in spirit.Presence.Energy.slots) {
				
				// energy amount
				using( var imgStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.tokens.{energy.Text}.png" ) ){
					using var bitmap = new Bitmap( imgStream ); 
					graphics.DrawImage( bitmap, x, y, slotWidth, slotWidth );
				};

				RectangleF presenceRect = new RectangleF( 
					x + (slotWidth-presenceSize.Width)/2, 
					y - presenceSize.Height / 2, 
					presenceSize.Width,
					presenceSize.Height
				);

				// Highlight Option
				if(revealedEnergySpaces == idx && trackOptions.Contains( energy )) {
					graphics.DrawEllipse(highlightPen, presenceRect );
					hotSpots.Add(energy,presenceRect);
				}

				// presence
				if(revealedEnergySpaces <= idx )
					graphics.DrawImage( presence, presenceRect );

				x += slotWidth;
				++idx;
			}

			y += slotWidth;
		}

		void DrawCardPlayTrack( Graphics graphics, Font simpleFont, Bitmap presence, float slotWidth, SizeF presenceSize, Pen highlightPen, ref float y ) {

			float x = margin;

			// draw title
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, x, y );
			y += lineHeight;

			float maxCardHeight = 0;
			float cardWidth = slotWidth * 0.8f;
			float cardLeft = (slotWidth - cardWidth) / 2; // center

			int revealedCardSpaces = spirit.Presence.CardPlays.RevealedCount;
			int idx = 0;

			foreach(var track in spirit.Presence.CardPlays.slots) {

				// card plays amount
				using(var imgStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.tokens.{track.Text}.png" )) {
					using var bitmap = new Bitmap( imgStream );
					var cardHeight = cardWidth * bitmap.Height / bitmap.Width;
					maxCardHeight = Math.Max(cardHeight,maxCardHeight);
					graphics.DrawImage( bitmap, x+ cardLeft, y, cardWidth, cardHeight );
				};

				RectangleF presenceRect = new RectangleF(
					x + (slotWidth - presenceSize.Width) / 2,
					y - presenceSize.Height / 2,
					presenceSize.Width,
					presenceSize.Height
				);

				// Highlight Option
				if(revealedCardSpaces == idx && trackOptions.Contains( track )) {
					graphics.DrawEllipse( highlightPen, presenceRect );
					hotSpots.Add( track, presenceRect );
				}


				// presence
				if(revealedCardSpaces <= idx)
					graphics.DrawImage( presence, presenceRect );

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

				if(innateOptionNames.Contains( name )) {
					var rect = new RectangleF(x,y,drawWidth,drawHeight);
					graphics.DrawRectangle( highlightPen, rect.X, rect.Y, rect.Width, rect.Height );
					hotSpots.Add(innateOptions.Single(x=>x.Name==name),rect);
				}

				y += drawHeight;
				y += 10;
			}

			return x;
		}

		void DrawActivatedElements( Graphics graphics, Font simpleFont, ref float y ) {
			y += 20;
			const float elementSize = 50f;
			var elements = spirit.Elements; // cache, don't recalculate
			float x = margin;

			var orderedElements = elements.Keys.OrderBy( el => elementOrder[el] );
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

		#endregion

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );
			Point clientCoord = this.PointToClient( Control.MousePosition );
			Cursor = HitTest( clientCoord ) != null ? Cursors.Hand : Cursors.Arrow;
		}

		protected override void OnClick( EventArgs e ) {
			base.OnClick( e );
			Point clientCoord = this.PointToClient( Control.MousePosition );
			var option = HitTest( clientCoord );
			if(option != null)
				OptionSelected?.Invoke(option);
		}

		public event Action<IOption> OptionSelected;

		IOption HitTest( Point clientCoord ) {
			return hotSpots.Keys.FirstOrDefault(key=>hotSpots[key].Contains(clientCoord));
		}

		readonly Dictionary<IOption,RectangleF> hotSpots = new Dictionary<IOption, RectangleF>();

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
		Track[] trackOptions;
		InnatePower[] innateOptions;
		Spirit spirit;

		readonly Dictionary<string, Image> innateImages = new();
		readonly Dictionary<Element, Image> elementImages = new();
		#endregion

	}
}
