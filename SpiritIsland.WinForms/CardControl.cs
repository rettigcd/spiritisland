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
		}


		public void Init(Spirit spirit,IHaveOptions iHaveOptions){
			this.spirit = spirit;
			iHaveOptions.NewDecision += Options_NewDecision;
		}

		void Options_NewDecision( IDecision decision ) {

			pickPowerCard = decision as Decision.PickPowerCard;
			if(pickPowerCard != null) {
				this.options = pickPowerCard.CardOptions;
			} else {
				// when selecting power to resolve, is not PickPowerCard but PickActionFactory
				this.options = decision.Options.OfType<PowerCard>().ToArray();
			}

			this.Invalidate();
		}

		public event Action<PowerCard> CardSelected;

		#region Paint / Draw

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );

			this.locations.Clear();
			this.x = 0;

			if(spirit != null){

				// Cards that are not in our hand - new cards from the deck
				var missingCards = options.Except( spirit.PurchasedCards ).Except(spirit.Hand).ToArray();
				if(missingCards.Length>0)
					foreach(var card in missingCards)
						DrawCard( pe.Graphics, card, false );

				// Draw Purchased
				foreach(var card in spirit.PurchasedCards)
					DrawCard( pe.Graphics, card, true );

				x += 20;

				// Draw In Hand
				foreach(var card in spirit.Hand)
					DrawCard( pe.Graphics, card, false );

			}

		}

		void DrawCard( Graphics graphics, PowerCard card, bool purchased ) {

			bool isOption = options.Contains( card );

			if(!purchased && pickPowerCard != null && isOption)
				graphics.DrawString(pickPowerCard.Use.ToString(),SystemFonts.MessageBoxFont, Brushes.White, x, margin );

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

		#endregion

		#region UI Event Handling

		protected override void OnClick( EventArgs e ) {
			if(options.Length==0) return;

			var card = GetCardAt( PointToClient( Control.MousePosition ) );
			if(card != null)
				CardSelected?.Invoke(card);

		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );
			if(options.Length==0) return;
			this.Cursor = GetCardAt( PointToClient( Control.MousePosition ) ) == null
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

	}

}
