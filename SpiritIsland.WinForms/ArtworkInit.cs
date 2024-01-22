using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class ArtworkInit( Label label ) {

	readonly Label _promptLabel = label;

	public void Init() {
		_ = Task.Run( InitArtwork );
		_initTimer = new Timer {
			Interval = 250
		};
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
		PowerCard[] majors = builder.BuildMajorCards();
		PowerCard[] minors = builder.BuildMinorCards();

		PowerCard[] uniques = builder.BuildSpirits( builder.SpiritNames ).SelectMany(s=>s.Hand).ToArray();
		List<IBlightCard> blightCards = builder.BuildBlightCards();

		_initTotal = fearCards.Count 
			+ blightCards.Count
			+ majors.Length + minors.Length
			+ uniques.Length;

		BuildFearCards( fearCards );
		BuildBlightCards( blightCards );
		await BuildPowerCards( majors );
		await BuildPowerCards( minors );
		await BuildPowerCards( uniques );

		_initCurrent = _initTotal;
	}

	async Task BuildPowerCards( PowerCard[] powerCards ) {
		foreach(PowerCard powerCard in powerCards) {
			_initCurrent++;
			try {
				using Image img = await ResourceImages.Singleton.GetPowerCard( powerCard );
			}
			catch(Exception ex) {
				MessageBox.Show( ex.ToString() );
			}
		}
	}

	void BuildBlightCards( List<IBlightCard> blightCards ) {
		foreach(IBlightCard blightCard in blightCards) {
			_initCurrent++;
			try {
				using Image img = ResourceImages.Singleton.GetBlightCard( blightCard );
			}catch(Exception ex) {
				MessageBox.Show(ex.ToString());
			}
		}
	}


	void BuildFearCards( List<IFearCard> fearCards ) {
		foreach(IFearCard fearCard in fearCards) {
			_initCurrent++;
			try {
				using Image img = ResourceImages.Singleton.GetFearCard( fearCard );
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