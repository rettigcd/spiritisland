using System.Drawing;
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
				string msg = $"Turn: {gameState.RoundNumber} ---    Blight Remaining: {gameState.blightOnCard}";
				pe.Graphics.DrawString(msg,SystemFonts.DefaultFont,Brushes.Black,0,0);
			}

		}

	}
}
