using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.PromoPack1;
using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class ConfigureGameDialog : Form {
		public ConfigureGameDialog() {
			InitializeComponent();
		}

		void ConfigureGame_Load( object sender, EventArgs e ) {
			Init_SpiritList();

			// boards
			boardListBox.Items.Add( "[Random]" );
			boardListBox.Items.Add( "A" );
			boardListBox.Items.Add( "B" );
			boardListBox.Items.Add( "C" );
			boardListBox.Items.Add( "D" );
			boardListBox.SelectedIndex = 0;

			// color
			colorListBox.Items.Add( "[Automatic]" );
			colorListBox.Items.Add( "red" );
			colorListBox.Items.Add( "orange" );
			colorListBox.Items.Add( "yellow" );
			colorListBox.Items.Add( "green" );
			colorListBox.Items.Add( "blue" );
			colorListBox.Items.Add( "dkblue" );
			colorListBox.Items.Add( "purple" );
			colorListBox.Items.Add( "pink" );
			colorListBox.Items.Add( "greenorangeswirl" );
			colorListBox.SelectedIndex = 0;


			CheckOkStatus( null, null );
		}

		class SpiritBox {
			public Type SpiritType;
			public string Name => (string)SpiritType.GetField("Name").GetValue(null);
		}

		static readonly SpiritBox[] spiritTypes;

		static ConfigureGameDialog() {
			spiritTypes = GetAllSpiritTypes()
				.Select( type => new SpiritBox { SpiritType = type } )
				.OrderBy( t => t.Name )
				.ToArray();

		}

		static IEnumerable<Type> GetAllSpiritTypes() {
			return new IGameComponentProvider[]{ 
				new Basegame.GameComponentProvider(),
				new BranchAndClaw.GameComponentProvider(),
				new PromoPack1.GameComponentProvider(),
				new JaggedEarth.GameComponentProvider()
			}.SelectMany(p=>p.Spirits);

			//return ScanForAssemblies()
			//	.SelectMany( assembly => assembly.GetTypes() )
			//	.Where(type => type.IsSubclassOf( typeof( Spirit ) ));
			//
		}

		//static IEnumerable<System.Reflection.Assembly> ScanForAssemblies() {
		//	foreach(string assemblyPath in Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories))
		//		yield return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
		//}

		void Init_SpiritList() {

			spiritListBox.Items.Clear();
			spiritListBox.Items.Add( "[Random]" );

			foreach(var spirit in spiritTypes)
				spiritListBox.Items.Add( spirit );

			spiritListBox.SelectedIndex = 0;

		}

		private void CheckOkStatus( object sender, EventArgs e ) {
			okButton.Enabled = true;
		}

		private void OkButton_Click( object sender, EventArgs e ) {
			var gameSettings = new GameConfiguration {
				SpiritType = SelectedSpiritType(),
				UsePowerProgression = powerProgressionCheckBox.Checked,
				Board = SelectedBoard(),
				ShuffleNumber = ShuffleNumber(),
				UseBranchAndClaw = true,
				UseJaggedEarth = true,
			};
			gameSettings.Color = (colorListBox.SelectedIndex == 0)
				? GetColorForSpirit( gameSettings.SpiritType )
				: colorListBox.SelectedItem as string;
			GameConfig = gameSettings;

		}

		int ShuffleNumber() {
			string txt = shuffleNumberTextBox.Text.Trim();
			return int.TryParse(txt, out int shuffleNumber) ? shuffleNumber
				: new Random().Next( 0, 999_999_999 );
		}

		static string GetColorForSpirit( Type spiritType ) {
			string spiritName = ((Spirit)Activator.CreateInstance( spiritType )).Text;
			return spiritName switch {
				RiverSurges.Name           => "blue",
				LightningsSwiftStrike.Name => "yellow",
				VitalStrength.Name         => "orange",
				Shadows.Name               => "purple",
				Thunderspeaker.Name        => "red",
				ASpreadOfRampantGreen.Name => "green",
				Bringer.Name               => "pink",
				Ocean.Name                 => "dkblue",
				Keeper.Name                => "greenorangeswirl",
				SharpFangs.Name            => "red",
				HeartOfTheWildfire.Name    => "green",
				SerpentSlumbering.Name     => "orange",
				LureOfTheDeepWilderness.Name => "green",
				StonesUnyieldingDefiance.Name => "orange",
				_                          => "blue"
			};
		}

		string SelectedBoard() {
			return (boardListBox.SelectedIndex == 0)
				? boardListBox.Items[1 + (int)(DateTime.Now.Ticks % 4)] as string
				: boardListBox.SelectedItem as string;
		}

		Type SelectedSpiritType() {
			int numberOfSpirits = spiritListBox.Items.Count-1;

			var box = (spiritListBox.SelectedIndex == 0)
				? spiritListBox.Items[1 + (int)(DateTime.Now.Ticks % numberOfSpirits)] as SpiritBox
				: spiritListBox.SelectedItem as SpiritBox;
			return box.SpiritType;
		}

		public GameConfiguration GameConfig { get; private set; }

		private void CheckBox1_CheckedChanged( object sender, EventArgs e ) {
			Init_SpiritList();
		}
	}

}
