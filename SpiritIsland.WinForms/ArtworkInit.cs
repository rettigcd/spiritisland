using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class ArtworkInit {

	Label _promptLabel;

	public ArtworkInit( Label label ) {
		_promptLabel = label;
	}

	public void Init() {
		_ = Task.Run( InitArtwork );
		_initTimer = new Timer();
		_initTimer.Interval = 250;
		_initTimer.Tick += _initTimer_Tick;
		_initTimer.Enabled = true;
		_initTimer.Start();
	}
	int _initCurrent = 0;
	int _initTotal;
	Timer _initTimer;

	async Task InitArtwork() {
		var builder = ConfigureGameDialog.GameBuilder;
		List<IFearCard> fearCards = builder.BuildFearCards();
		PowerCard[] powerCards = builder.BuildMajorCards()
			.Union( builder.BuildMinorCards() )
			.Union( builder.BuildSpirits( builder.SpiritNames ).SelectMany( s => s.Hand ) )
			.ToArray();

		_initTotal = fearCards.Count + powerCards.Length;

		BuildFearCards( fearCards );

		foreach(PowerCard powerCard in powerCards) {
			_initCurrent++;
			try {
				using Image img = await ResourceImages.Singleton.GetPowerCard( powerCard );
			}
			catch(Exception ex) {
				MessageBox.Show( ex.ToString() );
			}
		}

		_initCurrent = _initTotal;
	}

	void BuildFearCards( List<IFearCard> fearCards ) {
		foreach(IFearCard fearCard in fearCards) {
			_initCurrent++;
			try {
				ResourceImages.Singleton.InitFearCard( fearCard );
			}catch(Exception ex) {
				MessageBox.Show(ex.ToString());
			}
		}
	}

	void _initTimer_Tick( object sender, EventArgs e ) {
		if(_initCurrent == _initTotal) {
			_initTimer.Enabled = false;
			_promptLabel.Text = $"Initialization Complete";
			return;
		}
		_promptLabel.Text = $"One-time Artwork Initializing {_initCurrent} of {_initTotal}";
	}

}