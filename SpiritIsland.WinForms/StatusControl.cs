﻿using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class StatusControl : Control {
		public StatusControl() {
			InitializeComponent();
		}

		public void Init(GameState gameState, IHaveOptions optionProvider ) {
			this.gameState = gameState;
			// !! This is the wrong event.  But it fires all of the time so it will do for now.
			optionProvider.NewDecision += (obj) => this.Invalidate();
		}

		GameState gameState;


		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );

			if(gameState != null) {
				string ravage = gameState.InvaderDeck.Ravage.FirstOrDefault()?.Text ?? "-";
				string build = gameState.InvaderDeck.Build.FirstOrDefault()?.Text ?? "-";
				string msg = $"Energy: {gameState.Spirits[0].Energy}      ---      "
					+ $"Turn: {gameState.Round} --- Ravage: {ravage}  Build: {build}      ---      "
					+ $"Fear Pool: {gameState.Fear.Pool}  Activated: {gameState.Fear.ActivatedCards.Count}  Terror Level: {gameState.Fear.TerrorLevel}     ----       "
					+$"Blight Remaining: {gameState.blightOnCard}";
				pe.Graphics.DrawString(msg,SystemFonts.DefaultFont,Brushes.Black,0,0);
			}

		}

	}
}
