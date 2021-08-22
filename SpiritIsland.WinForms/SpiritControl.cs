using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

	public partial class SpiritControl : Control {

		#region Constructor / Init

		public SpiritControl() {
			InitializeComponent();
			this.BackColor = Color.LightYellow;
			this.Cursor = Cursors.Default;
		}

		public void Init( Spirit spirit, string presenceColor, IHaveOptions optionProvider, ResourceImages resourceImages ) {
			this.spirit = spirit;
			this.images = resourceImages;

			this.presenceColor = presenceColor ?? throw new ArgumentNullException( nameof( presenceColor ) );

			InitElementDisplayOrder( spirit );

			optionProvider.OptionsChanged += OptionProvider_OptionsChanged;
		}

		void InitElementDisplayOrder( Spirit spirit ) {
			static Element[] Highest( InnatePower power ) => power.GetTriggerThresholds()
				.OrderByDescending( list => list.Length )
				.First();

			int i = 0;
			foreach(var innate in spirit.InnatePowers)
				foreach(var el in Highest( innate ))
					if(!elementOrder.ContainsKey( el )) elementOrder[el] = i++;
			foreach(Element el in Enum.GetValues( typeof( Element ) ))
				if(!elementOrder.ContainsKey( el )) elementOrder[el] = i++;
		}

		#endregion

		public event Action<IOption> OptionSelected;

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
			using Bitmap presence = images.GetPresenceIcon(presenceColor);

			// calc slot width and presence height
			int maxLength = Math.Max( spirit.Presence.CardPlays.TotalCount, spirit.Presence.Energy.TotalCount ) + 2; // +2 for energy & Destroyed

			// Calc Presence and coin widths
			float slotWidth = (Width - 2 * margin) / maxLength;
			float presenceWidth = slotWidth * 0.9f;
			SizeF presenceSize = new SizeF( presenceWidth, presenceWidth * presence.Height / presence.Width );

			int y = margin;
			// Image
			y += DrawSpiritImage( graphics, margin, y ).Height;
			y += margin;

			// Energy
			y += DrawEnergyTrack( graphics, simpleFont, presence, (int)slotWidth, presenceSize, highlightPen, margin, y ).Height;
			y += margin;

			// Cards
			y += DrawCardPlayTrack( graphics, simpleFont, presence, slotWidth, presenceSize, highlightPen, y ).Height;
			y += margin;
			y += margin;

			// Innates
			float x = margin;
			int maxHeight = 0;
			foreach(string name in spirit.InnatePowers.Select( i => i.Name ).Distinct()) {
				var sz = DrawInnates( graphics, name, highlightPen, x, y );
				x += (sz.Width + margin);
				maxHeight = Math.Max( maxHeight, sz.Height );
			}
			y += (maxHeight + margin);

			// activated elements
			DrawActivatedElements( graphics, simpleFont, y );

			// !Note! - If you do not specify output width/height of image, .Net will scale image based on screen DPI and image DPI
		}

		Size DrawSpiritImage( Graphics graphics, int x, int y ) {
			var image = spiritImage ??= LoadSpiritImage();
			Size sz = new Size(180,120);
			graphics.DrawImage(image,x,y,sz.Width,sz.Height);
			return sz;
		}

		Size DrawEnergyTrack( Graphics graphics, Font simpleFont, Bitmap presence, int slotWidth, SizeF presenceSize, Pen highlightPen, int x, int y ) {

			int startingX = x; // capture so we calc differene at end.
			int startingY = y; // capture so we calc differene at end.

			float coinWidth = slotWidth * 0.8f;
			float coinLeftOffset = (slotWidth - coinWidth) / 2;


			// Title
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, x, y );
			SizeF textSize = graphics.MeasureString( "Energy", simpleFont );
			y += margin + (int)textSize.Height;

			int revealedEnergySpaces = spirit.Presence.Energy.RevealedCount;

			int idx = 0;
			int maxY = y; // inc 

			int presenceOffset = (int)presenceSize.Height / 2;

			foreach(var energy in spirit.Presence.Energy.slots) {
				
				// Draw - energy icons
				using( var bitmap = this.images.GetEnergyIcon(energy.Text))
					graphics.DrawImage( bitmap, x + coinLeftOffset, y + presenceOffset, coinWidth, coinWidth );

				RectangleF presenceRect = new RectangleF( x + (slotWidth-presenceSize.Width)/2, y, presenceSize.Width, presenceSize.Height );

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
				maxY = Math.Max(maxY,y+presenceOffset+(int)slotWidth);
			}

			// Current $$ balance
			graphics.DrawString( "$"+spirit.Energy, simpleFont, SystemBrushes.ControlDarkDark, x+slotWidth/2, y );

			return new Size(
				x - startingX, 
				maxY - startingY // 
			);
		}

		Size DrawCardPlayTrack( Graphics graphics, Font simpleFont, Bitmap presence, float slotWidth, SizeF presenceSize, Pen highlightPen, int y ) {
			int startingY = y; // capture so we can calc Height

			float x = margin;

			// draw title
			SizeF titleSize = graphics.MeasureString("Cards", simpleFont);
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, x, y );
			y += (int)(titleSize.Height + margin);

			float maxCardHeight = 0;
			float cardWidth = slotWidth * 0.6f;
			float cardLeft = (slotWidth - cardWidth) / 2; // center

			int revealedCardSpaces = spirit.Presence.CardPlays.RevealedCount;
			int idx = 0;

			int presenceYOffset = (int)presenceSize.Height / 2;

			int maxY = y;
			int cardY = y + presenceYOffset;
			foreach(var track in spirit.Presence.CardPlays.slots) {

				// card plays amount
				using( var bitmap = images.GetCardplayIcon(track.Text)) {
					float cardHeight = cardWidth * bitmap.Height / bitmap.Width;
					maxCardHeight = Math.Max(cardHeight,maxCardHeight);
					graphics.DrawImage( bitmap, x+ cardLeft, cardY, cardWidth, cardHeight );
					maxY = Math.Max( maxY, cardY + (int)cardHeight );
				};


				RectangleF presenceRect = new RectangleF( x + (slotWidth - presenceSize.Width) / 2, y, presenceSize.Width, presenceSize.Height );

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

			// Current $$ balance
			graphics.DrawString( $"{spirit.Presence.Destroyed} Destroyed", simpleFont, SystemBrushes.ControlDarkDark, x + slotWidth / 2, cardY );

			return new Size(
				0, // not used, ignored
				maxY - startingY
			);

		}

		Size DrawInnates( Graphics graphics, string name, Pen highlightPen, float x, float y ) {

			var image = GetInnateImage( name ); // This non-sense is because Thunderspeaker has a fast & slow option with the same name.

			int drawWidth = (Width - 3*margin)/2; // 3 margins => left, center, right
			Size sz = new Size(
				drawWidth,
				drawWidth * image.Height / image.Width
			);

			graphics.DrawImage( image, x, y, sz.Width, sz.Height );

			if( innateOptions.Any( x => x.Name == name ) ) {
				var rect = new RectangleF(x,y,drawWidth,sz.Height);
				graphics.DrawRectangle( highlightPen, rect.X, rect.Y, rect.Width, rect.Height );
				hotSpots.Add(innateOptions.Single(x=>x.Name==name),rect);
			}

			return sz;
		}

		void DrawActivatedElements( Graphics graphics, Font simpleFont, float y ) {
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
				graphics.DrawImage( GetElementImage( element ), x, y, elementSize, elementSize );
				x += elementSize;
				x += 10;
			}
		}

		Image LoadSpiritImage() {
			string filename = spirit.Text.Replace( ' ', '_' );
			return Image.FromFile( $".\\images\\spirits\\{filename}.png" );
		}
		Image spiritImage;

		Image GetInnateImage( string innateCardName ) {
			if(!innateImages.ContainsKey( innateCardName )) {
				string filename = innateCardName.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
				Image image = Image.FromFile( $".\\images\\innates\\{filename}.jpg" );
				innateImages.Add( innateCardName, image );
			}
			return innateImages[innateCardName];
		}

		Image GetElementImage( Element element ) {

			if(!elementImages.ContainsKey( element )) {
				string filename = "Simple_" + element.ToString().ToLower();
				Image image = Image.FromFile( $".\\images\\tokens\\{filename}.png" );
				elementImages.Add( element, image );
			}
			return elementImages[element];
		}

		#endregion

		#region UI event handlers

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

		IOption HitTest( Point clientCoord ) {
			return hotSpots.Keys.FirstOrDefault(key=>hotSpots[key].Contains(clientCoord));
		}

		#endregion

		#region private fields

		ResourceImages images;

		const int margin = 10;

		string presenceColor;
		Track[] trackOptions;
		InnatePower[] innateOptions;
		Spirit spirit;

		readonly Dictionary<string, Image> innateImages = new();
		readonly Dictionary<Element, Image> elementImages = new();
		readonly Dictionary<Element, int> elementOrder = new();
		readonly Dictionary<IOption, RectangleF> hotSpots = new();

		#endregion

	}

}
