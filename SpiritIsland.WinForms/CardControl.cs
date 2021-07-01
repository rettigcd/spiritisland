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
		}

		public void ShowCards(PowerCard[] cards){
			this.cards = cards;
			this.Invalidate();
		}

		PowerCard[] cards;
		readonly Dictionary<PowerCard,Image> images = new Dictionary<PowerCard, Image>();
		readonly Dictionary<PowerCard,RectangleF> locations = new Dictionary<PowerCard, RectangleF>();

		Image GetImage(PowerCard card){
			if(!images.ContainsKey(card)){
				string filename = card.Name switch {
					BoonOfVigor.Name => "boon_of_vigor",
					FlashFloods.Name => "flash_floods",
					AcceleratedRot.Name => "accelerated_rot",
					EncompassingWard.Name => "encompassing_ward",
					NaturesResilience.Name => "natures_resilience",
					PullBeneathTheHungryEarth.Name => "pull_beneath_the_hungry_earth",
					RiversBounty.Name => "rivers_bounty",
					SongOfSanctity.Name => "song_of_sanctity",
					Tsunami.Name => "tsunami",
					UncannyMelting.Name => "uncanny_melting",
					WashAway.Name => "wash_away",
					_ => throw new Exception("unexpected name:"+card.Name)
				};
				Image image = Image.FromFile($".\\images\\{filename}.jpg");
				images.Add(card,image);
			}
			return images[card];
		}

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );
			if(cards==null) return;

			this.locations.Clear();
			for(int i=0;i<cards.Length;++i){
				var card = cards[i];
				var rect = new RectangleF(i*375,0,350,500);
				locations.Add(card,rect);
				pe.Graphics.DrawImage(GetImage(card),rect);
			}
		}

		protected override void OnClick( EventArgs e ) {
			if(cards==null) return;

			var mp = this.PointToClient(Control.MousePosition);
			foreach(var card in this.cards){
				if(locations[card].Contains(mp)){
					CardSelected?.Invoke(card);
					break;
				}
			}

		}

		public event Action<PowerCard> CardSelected;

	}
}
