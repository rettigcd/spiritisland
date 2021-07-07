using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpiritIsland.Base;
using SpiritIsland.Core;

namespace SpiritIsland.WinForms {

	public partial class CardControl : Control {

		public CardControl() {
			InitializeComponent();
			SetStyle(ControlStyles.AllPaintingInWmPaint 
				| ControlStyles.UserPaint 
				| ControlStyles.OptimizedDoubleBuffer 
				| ControlStyles.ResizeRedraw, true
			);
		}

		public void Init(Spirit spirit){
			this.spirit = spirit;
		}
		Spirit spirit;

		public void HighlightCards(IOption[] options){
			this.optionCards = options.OfType<IActionFactory>() // includes modified powers
				.Select(f=>f.Original) // use original
				.OfType<PowerCard>() // only power cards
				.ToArray();
			this.Invalidate();
		}

		PowerCard[] optionCards;
		readonly Dictionary<PowerCard,Image> images = new();
		readonly Dictionary<PowerCard,RectangleF> locations = new();

		Image GetImage(PowerCard card){

			if(!images.ContainsKey(card)){
				string filename = card.Name.Replace(' ','_').Replace("'","").ToLower();
				//string filename = card.Name switch {
				//	// River
				//	BoonOfVigor.Name => "boon_of_vigor",
				//	FlashFloods.Name => "flash_floods",
				//	RiversBounty.Name => "rivers_bounty",
				//	WashAway.Name => "wash_away",
				//	// Lightning
				//	HarbingerOfLightning.Name => "harbinger_of_lightning",
				//	LightningsBoon.Name => "lightnings_boon",
				//	RagingStorm.Name => "raging_storm",
				//	ShatteredHomesteads.Name => "shattered_homesteads",
				//	// Major
				//	AcceleratedRot.Name => "accelerated_rot",
				//	Tsunami.Name => "tsunami",
				//	// Minor
				//	EncompassingWard.Name => "encompassing_ward",
				//	NaturesResilience.Name => "natures_resilience",
				//	PullBeneathTheHungryEarth.Name => "pull_beneath_the_hungry_earth",
				//	SongOfSanctity.Name => "song_of_sanctity",
				//	UncannyMelting.Name => "uncanny_melting",
				//	_ => throw new Exception("unexpected name:"+card.Name)
				//};
				Image image = Image.FromFile($".\\images\\{filename}.jpg");
				images.Add(card,image);
			}
			return images[card];
		}

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(optionCards == null) return;

			this.locations.Clear();
			this.x = 0;

			if(spirit != null){
				DrawCards( pe.Graphics, spirit.PurchasedCards );
				pe.Graphics.FillRectangle(Brushes.Beige, x, 0, 10, 300);
				x += 20;
				DrawCards( pe.Graphics, spirit.Hand );
			}

		}

		int x;

		void DrawCards( Graphics graphics, List<PowerCard> cards ) {

			foreach(var card in cards){
				var rect = new Rectangle( x, 0, 350, 500 );
				locations.Add( card, rect );
				graphics.DrawImage( GetImage( card ), rect );
				if(optionCards.Contains(card)){
					using var highlightPen = new Pen(Color.Red,15);
					graphics.DrawRectangle(highlightPen,rect);
				}
				x += 375;
			}
		}

		protected override void OnClick( EventArgs e ) {
			if(optionCards==null) return;

			var mp = this.PointToClient(Control.MousePosition);
			foreach(var card in this.optionCards){
				if(locations[card].Contains(mp)){
					CardSelected?.Invoke(card);
					break;
				}
			}

		}

		public event Action<PowerCard> CardSelected;

	}
}
