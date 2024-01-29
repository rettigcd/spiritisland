using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class AdversaryRect(AdversaryConfig _adversary) : IPaintableRect, IClickableLocation {

	public float? WidthRatio => 234f/148f; // image dimensions

	/// <summary> Records where it was painted, so we can click on it. </summary>
	public Rectangle Bounds {get; private set;}

	public void Click(){
		var adv = ConfigureGameDialog.GameBuilder.BuildAdversary( _adversary );
		var adjustments = adv.Levels;
		var rows = new List<string> {
			$"==== {_adversary.Name} - Level:{_adversary.Level} - Difficulty:{adjustments[_adversary.Level].Difficulty} ===="
		};
		// Loss
		var lossCond = adv.LossCondition;
		if(lossCond is not null){
			rows.Add( $"\r\n-- Additional Loss Condition --" );
			rows.Add( lossCond.Description );
		}
		// Levels
		for(int i = 0; i <= _adversary.Level; ++i) {
			var a = adjustments[i];
			string label = i == 0 ? "Escalation: " : "Level:" + i;
			rows.Add( $"\r\n-- {label} {a.Title} --" );
			rows.Add( $"{a.Description}" );
		}
		MessageBox.Show( rows.Join( "\r\n" ) );
	}
	public bool Contains( Point point ) => Bounds.Contains(point);

	public void Paint(Graphics graphics, Rectangle bounds) {
		using Bitmap flag = ResourceImages.Singleton.AdversaryFlag( _adversary.Name );
		Bounds = bounds.FitBoth(flag.Size);
		graphics.DrawImage( flag, Bounds );
	}
}
