using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

	public partial class CardControl : Control {

		public CardControl() {
			InitializeComponent();
			SetStyle(ControlStyles.AllPaintingInWmPaint 
				| ControlStyles.UserPaint 
				| ControlStyles.OptimizedDoubleBuffer 
				| ControlStyles.ResizeRedraw, true
			);
			this.Cursor = Cursors.Default;
			this.options = Array.Empty<PowerCard>();
			InitIconPositions();
			handImage    = Image.FromFile( ".\\images\\hand.png" );
			inPlayImage  = Image.FromFile( ".\\images\\inplay.png" );
			discardImage = Image.FromFile( ".\\images\\discard.png" );
		}

		public void Init(Spirit spirit,IHaveOptions iHaveOptions){
			CurrentLocation = CardLocation.Hand;
			this.spirit = spirit;
			iHaveOptions.NewDecision += Options_NewDecision;
		}

		void Options_NewDecision( IDecision decision ) {

			pickPowerCard = decision as Decision.PickPowerCard; // capture so we can display card-action
			this.options = pickPowerCard != null
				? pickPowerCard.CardOptions 
				: decision.Options.OfType<PowerCard>().ToArray();

			choiceLocations.Clear();
			if( spirit.Hand.Intersect(options).Any() ) choiceLocations.Add( CardLocation.Hand );
			if( spirit.InPlay.Intersect(options).Any() ) choiceLocations.Add( CardLocation.InPlay );
			if( spirit.DiscardPile.Intersect(options).Any() ) choiceLocations.Add( CardLocation.Discarded );

			// If there are cards to display but we aren't on them
			if( choiceLocations.Any() && !choiceLocations.Contains(CurrentLocation) )
				CurrentLocation = choiceLocations.First(); // switch

			this.Invalidate();
		}

		public event Action<PowerCard> CardSelected;

		#region Paint / Draw

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(spirit == null) return;

			this.locations.Clear();

			// == Draw Hand / Played / Discarded

			pe.Graphics.FillRectangle( Brushes.Yellow, GetButtonRect(CurrentLocation) );

			pe.Graphics.DrawImageFitBoth(handImage,handRect);			
			pe.Graphics.DrawImageFitBoth(inPlayImage,inPlayRect);		
			pe.Graphics.DrawImageFitBoth(discardImage,discardRect);		

			foreach(var loc in choiceLocations)
				pe.Graphics.DrawRectangle(Pens.Red, GetButtonRect(loc) );

			// Draw AFTER choiceLocations so # is on top
			pe.Graphics.DrawCount(handRect,spirit.Hand.Count);
			pe.Graphics.DrawCount(inPlayRect,spirit.InPlay.Count);
			pe.Graphics.DrawCount(discardRect,spirit.DiscardPile.Count);

			this.x = 60; // give room to draw H / P / D

			// Cards that are not in our hand - new cards from the deck
			var missingCards = options
				.Except( spirit.InPlay )
				.Except(spirit.Hand)
				.Except(spirit.DiscardPile)
				.ToArray();
			if(missingCards.Length>0)
				foreach(var card in missingCards)
					DrawCard( pe.Graphics, card, false );

			// Hand
			if(CurrentLocation == CardLocation.Hand)
				foreach(var card in spirit.Hand)
					DrawCard( pe.Graphics, card, false );

			// Played
			if(CurrentLocation == CardLocation.InPlay)
				foreach(var card in spirit.InPlay)
					DrawCard( pe.Graphics, card, false );

			// Discarded
			if(CurrentLocation == CardLocation.Discarded)
				foreach(var card in spirit.DiscardPile)
					DrawCard( pe.Graphics, card, false );

		}

		Rectangle GetButtonRect(CardLocation location){
			switch( location ) {
				case CardLocation.Hand: return handRect;
				case CardLocation.InPlay: return inPlayRect;
				case CardLocation.Discarded: return discardRect;
			}
			throw new ArgumentException("invalid card location");
		}


		void InitIconPositions() {
			handRect = new Rectangle(5,0,40,40);
			inPlayRect = new Rectangle(5,40,40,40);
			discardRect = new Rectangle(5,80,40,40);
		}

		void DrawCard( Graphics graphics, PowerCard card, bool purchased ) {

			InitIconPositions();

			bool isOption = options.Contains( card );

			if(!purchased && pickPowerCard != null && isOption)
				graphics.DrawString(pickPowerCard.Use(card).ToString(),SystemFonts.MessageBoxFont, Brushes.White, x, margin );

			var rect = new Rectangle( x, margin + (purchased ? 0 : topSpacer), cardWidth, cardHeight );
			if(!locations.ContainsKey( card )) // only the first is clickable
				locations.Add( card, rect );

			graphics.DrawImage( images.GetImage( card ), rect );
			if(isOption) {
				using var highlightPen = new Pen( Color.Red, 15 );
				graphics.DrawRectangle( highlightPen, rect );
			}
			x += cardWidth + 20;
		}

		#endregion

		protected override void OnResize( EventArgs e ) {
			base.OnResize( e );

			topSpacer = (int)((Height-margin) * .1f);
			cardHeight = (Height-margin) - topSpacer;
			cardWidth = cardHeight * 350 / 500;
		}
		const int margin = 10;
		int topSpacer;
		int cardHeight;
		int cardWidth;

		#region UI Event Handling

		protected override void OnClick( EventArgs e ) {

			var coords = PointToClient( Control.MousePosition );

			var switchLocation = GetLocationButton( coords );
			if(switchLocation != CardLocation.None) {
				CurrentLocation = switchLocation;
				Invalidate();
				return;
			}

			if(options.Length==0) return;

			var card = GetCardAt( coords );
			if(card != null)
				CardSelected?.Invoke(card);

		}

		CardLocation GetLocationButton(Point coords ) {
			if(handRect.Contains( coords )) return CardLocation.Hand;
			if(inPlayRect.Contains( coords )) return CardLocation.InPlay;
			if(discardRect.Contains( coords )) return CardLocation.Discarded;
			return CardLocation.None;
 		}


		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );
			var coords = PointToClient( Control.MousePosition );
			this.Cursor = (GetCardAt( coords ) == null && GetLocationButton( coords ) == CardLocation.None )
				? Cursors.Default
				: Cursors.Hand;
		}

		PowerCard GetCardAt( Point mp ) {
			return this.options
				.Where( card => locations.ContainsKey( card ) && locations[card].Contains( mp ) )
				.FirstOrDefault();
		}

		public static Bitmap MakeGrayscale3(Image original) {

			// create a blank bitmap the same size as original
			Bitmap newBitmap = new Bitmap(original.Width, original.Height);

			// get a graphics object from the new image
			using Graphics g = Graphics.FromImage(newBitmap);

			// create the grayscale ColorMatrix
			ColorMatrix colorMatrix = new ColorMatrix( new float[][] {
				new float[] {.3f, .3f, .3f, 0, 0},
				new float[] {.59f, .59f, .59f, 0, 0},
				new float[] {.11f, .11f, .11f, 0, 0},
				new float[] {0, 0, 0, 1, 0},
				new float[] {0, 0, 0, 0, 1}
			});

			// create some image attributes
			using ImageAttributes attributes = new ImageAttributes();

			// set the color matrix attribute
			attributes.SetColorMatrix(colorMatrix);

			// draw the original image on the new image using the grayscale color matrix
			g.DrawImage( original,
				new Rectangle(0, 0, original.Width, original.Height),
				0, 0, original.Width, original.Height,
				GraphicsUnit.Pixel, attributes
			);

		   return newBitmap;
		}

		#endregion

		#region private

		int x;
		readonly CardImageManager images = new CardImageManager();

		Decision.PickPowerCard pickPowerCard;
		PowerCard[] options;

		readonly Dictionary<PowerCard, RectangleF> locations = new();
		Spirit spirit;

		#endregion

		HashSet<CardLocation> choiceLocations = new HashSet<CardLocation>();

		enum CardLocation { None, Hand, InPlay, Discarded };
		CardLocation CurrentLocation;
		Rectangle handRect;
		Rectangle inPlayRect;
		Rectangle discardRect;

		Image handImage;
		Image discardImage;
		Image inPlayImage;

	}

}
