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
		}

		public void Init(Spirit spirit,IHaveOptions iHaveOptions){
			CurrentLocation = Img.Deck_Hand;
			// this.spirit = spirit;
			iHaveOptions.NewDecision += Options_NewDecision;

			decks = spirit.Decks;
			InitIconPositions(decks.Length);

			iconImages = new Image[decks.Length];
			for(int i = 0; i < decks.Length; ++i)
				iconImages[i] = ResourceImages.Singleton.GetImage( decks[i].Icon );
			
		}

		void Options_NewDecision( IDecision decision ) {

			pickPowerCard = decision as Decision.PickPowerCard; // capture so we can display card-action
			this.options = pickPowerCard != null
				? pickPowerCard.CardOptions 
				: decision.Options.OfType<PowerCard>().ToArray();

			choiceLocations.Clear();

			foreach(var deck in decks)
				if( deck.PowerCards.Intersect(options).Any() )
					choiceLocations.Add( deck.Icon );

			// If there are cards to display but we aren't on them
			if( choiceLocations.Any() && !choiceLocations.Contains(CurrentLocation) )
				CurrentLocation = choiceLocations.First(); // switch
			else if( GetCardsForLocation(CurrentLocation).Count == 0 )
				CurrentLocation = Img.Deck_Hand;
			
			this.Invalidate();
		}
		

		public event Action<PowerCard> CardSelected;

		#region Paint / Draw

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(decks == null) return;

			this.locations.Clear();

			InitIconPositions(decks.Length);

			// == Draw Hand / Played / Discarded

			pe.Graphics.FillRectangle( Brushes.Yellow, GetButtonRect(CurrentLocation) );

			for(int i = 0; i< decks.Length; i++)
				pe.Graphics.DrawImageFitBoth(iconImages[i],iconPositions[i]);

			foreach(var loc in choiceLocations)
				pe.Graphics.DrawRectangle(Pens.Red, GetButtonRect(loc) );

			// Draw Counts choiceLocations so # is on top
			for(int i = 0;i<decks.Length;i++)
				pe.Graphics.DrawCountIfHigherThan(iconPositions[i],   decks[i].PowerCards.Count,0);

			this.x = 60; // give room to draw H / P / D

			// Missing Cards
			var cardsWeHave = new HashSet<PowerCard>();
			foreach(var deck in decks) cardsWeHave.UnionWith( deck.PowerCards );
			var missingCards = options.Except(cardsWeHave).ToArray();
			if(missingCards.Length>0)
				foreach(var card in missingCards)
					DrawCard( pe.Graphics, card, false );

			// Current Deck Cards
			foreach(var card in GetCardsForLocation(CurrentLocation))
				DrawCard( pe.Graphics, card, false );

		}

		List<PowerCard> GetCardsForLocation( Img location ) {
			foreach(var deck in decks)
				if(deck.Icon == location)
					return deck.PowerCards;
			throw new ArgumentException("invalid card location");
		}


		Rectangle GetButtonRect(Img location){
			for(int i=0;i<decks.Length;i++)
				if(decks[i].Icon == location)
					return iconPositions[i];
			throw new ArgumentException( "invalid card location" );
		}

		void DrawCard( Graphics graphics, PowerCard card, bool purchased ) {

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
			if(switchLocation != default) {
				CurrentLocation = switchLocation;
				Invalidate();
				return;
			}

			if(options.Length==0) return;

			var card = GetCardAt( coords );
			if(card != null)
				CardSelected?.Invoke(card);

		}

		Img GetLocationButton(Point coords ) {
			if(iconPositions == null) return default; 
			for(int i=0;i<iconPositions.Length;++i)
				if(iconPositions[i].Contains( coords ))
					return decks[i].Icon;
			return default;
		}


		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );
			var coords = PointToClient( Control.MousePosition );
			this.Cursor = (GetCardAt( coords ) == null && GetLocationButton( coords ) == default )
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
		SpiritDeck[] decks;

		#endregion

		void InitIconPositions(int count) {
			iconPositions = new Rectangle[count];
			for(int i = 0;i<count;++i)
				iconPositions[i] = new Rectangle(5,50 * i,40,40); // 50 spreads them out a little.
		}

		readonly HashSet<Img> choiceLocations = new HashSet<Img>();

		Img CurrentLocation;

		Rectangle[] iconPositions;
		Image[] iconImages;

	}

}
