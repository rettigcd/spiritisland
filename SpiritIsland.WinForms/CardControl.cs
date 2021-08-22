using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
		}


		public void Init(Spirit spirit,IHaveOptions iHaveOptions){
			this.spirit = spirit;
			iHaveOptions.OptionsChanged += Options_OptionsChanged;
		}

		void Options_OptionsChanged( IOption[] options ) {
			this.optionCards = options.OfType<IActionFactory>() // includes modified powers
				.Select( f => f.Original ) // use original
				.OfType<PowerCard>() // only power cards
				.ToArray();

			fearCard = options.OfType<DisplayFearCard>().FirstOrDefault();

			this.Invalidate();
		}

		DisplayFearCard fearCard;

		public event Action<PowerCard> CardSelected;

		#region Paint / Draw

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(optionCards == null) return;

			this.locations.Clear();
			this.x = 0;

			if(spirit != null){

				// fear
				if(fearCard != null) {
					var img = fearCardImages.GetImage(fearCard.Text);
					var rect = new Rectangle( x, 0, cardWidth, cardHeight );
					pe.Graphics.DrawImage( img, rect );
					x+=rect.Width+20;
				}


				// Missing (drawing)
				var missingCards = optionCards.Except(spirit.PurchasedCards).Except(spirit.Hand).ToArray();
				if(missingCards.Length>0)
					DrawCards( pe.Graphics, missingCards);

				// Draw Purchased
				DrawCards( pe.Graphics, spirit.PurchasedCards );

				//				pe.Graphics.FillRectangle(Brushes.Beige, x, 0, 10, 300);
				x += 20;

				// Draw In Hand
				DrawCards( pe.Graphics, spirit.Hand );
			}

		}

		readonly FearCardImageManager fearCardImages = new FearCardImageManager();

		void DrawCards( Graphics graphics, IList<PowerCard> cards ) {

			foreach(var card in cards){
				var rect = new Rectangle( x, 0, cardWidth, cardHeight );
				if(!locations.ContainsKey(card)) // only the first is clickable
					locations.Add( card, rect );

				graphics.DrawImage( images.GetImage( card ), rect );
				if(optionCards.Contains(card)){
					using var highlightPen = new Pen(Color.Red,15);
					graphics.DrawRectangle(highlightPen,rect);
				}
				x += cardWidth + 20;
			}

		}

		protected override void OnResize( EventArgs e ) {
			base.OnResize( e );
			cardHeight = Height - 10;
			cardWidth = cardHeight * 350 / 500;
		}
		int cardHeight;
		int cardWidth;

		#endregion

		#region UI Event Handling

		protected override void OnClick( EventArgs e ) {
			if(optionCards==null) return;

			var card = GetCardAt( PointToClient( Control.MousePosition ) );
			if(card != null)
				CardSelected?.Invoke(card);

		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );
			this.Cursor = GetCardAt( PointToClient( Control.MousePosition ) ) == null
				? Cursors.Default
				: Cursors.Hand;
		}

		PowerCard GetCardAt( Point mp ) {
			return this.optionCards
				.Where( card => locations.ContainsKey( card ) && locations[card].Contains( mp ) )
				.FirstOrDefault();
		}

		#endregion

		#region private

		int x;
		readonly CardImageManager images = new CardImageManager();

		PowerCard[] optionCards;
		readonly Dictionary<PowerCard, RectangleF> locations = new();
		Spirit spirit;

		#endregion

	}

}
