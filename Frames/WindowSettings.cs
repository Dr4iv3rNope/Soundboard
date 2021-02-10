using System;

using System.Collections.Generic;

// forms
using System.Drawing;
using System.Windows.Forms;

// sound
using Un4seen.Bass;

using static soundboard.Misc.Key;
using soundboard.Misc;

namespace soundboard.Frames
{
	public class WindowSettings : Dr4iv3rForm
	{
		public TabControl gTab { get; }

		//
		// constructor
		//
		public WindowSettings()
		{
			// main frame settings
			BackColor = Color.White;
			Text = "Settings";
			MinimumSize = new Size(300, 200);
			MaximumSize = new Size(300, 500);
			MinimizeBox = false;
			MaximizeBox = false;

			// init tabs
			gTab = new TabControl();
			gTab.Parent = this;
			gTab.Dock = DockStyle.Fill;

			gTab.TabPages.Add(ConfigTab());
			gTab.TabPages.Add(DevicesTab());
			gTab.TabPages.Add(GUITab());

			g.MainFrame.FormClosed += (object obj, FormClosedEventArgs args) => Close();
		}

		//
		// DeviceButton
		//
		public class DeviceButton : RadioButton
		{
			public static List<DeviceButton> buttons = new List<DeviceButton>();

			public BASS_DEVICEINFO info;
			public int DeviceID;

			public DeviceButton(int DeviceID) : base()
			{
				this.DeviceID = DeviceID;
				this.info = Bass.BASS_GetDeviceInfo(DeviceID);
				this.Text = $"[{DeviceID}] {info.name}";

				buttons.Add(this);

				// events
				Click += (object obj, EventArgs args) =>
				{
					g.vars.DeviceID = DeviceID;

					ReloadDeviceButtons();
				};
			}

			public static void ReloadDeviceButtons()
			{
				foreach (var button in buttons)
					button.Checked = button.DeviceID == g.vars.DeviceID;
			}
		}

		//
		// Device tab page
		//
		public TabPage DevicesTab()
		{
			TabPage page = new TabPage("Device");
			page.AutoScroll = true;

			for (int DeviceID = 0; DeviceID < Bass.BASS_GetDeviceCount(); DeviceID++)
			{
				DeviceButton button = new DeviceButton(DeviceID);
				button.Parent = page;

				button.Dock = DockStyle.Top;
			}

			return page;
		}

		//
		// Config settings
		//
		public TabPage ConfigTab()
		{
			TabPage page = new TabPage("Config");
			page.AutoScroll = true;

			// load app cfg button
			Button loadApp = new Button();
			loadApp.Parent = page;
			loadApp.Dock = DockStyle.Bottom;
			loadApp.Text = "Load app configuration";
			loadApp.Click += (object obj, EventArgs args) => g.engine.config.LoadApp();

			// save app cfg button
			Button saveApp = new Button();
			saveApp.Parent = page;
			saveApp.Dock = DockStyle.Bottom;
			saveApp.Text = "Save app configuration";
			saveApp.Click += (object obj, EventArgs args) => g.engine.config.SaveApp();

			return page;
		}

		//
		// GUI Settings
		//
		public struct GUI
		{
			public static NumericUpDown fontsize = new NumericUpDown();
			public static NumericUpDown volume = new NumericUpDown();
			public static CheckBox info = new CheckBox();
			public static KeyDictPanel hotkeySound = new KeyDictPanel("Play sound by KEY + Track number");
			public static GroupBox lSoundDelay = new GroupBox();
			public static NumericUpDown soundDelay = new NumericUpDown();
			public static KeyDictPanel soundDelayKey = new KeyDictPanel("Delay play sound while holding key");
			public static CheckBox EnableScrolling = new CheckBox();
			public static NumericUpDown gridSize = new NumericUpDown();

			public static void SetupAllValues()
			{
				// Font size
				fontsize.Value = Convert.ToDecimal(g.vars.FontSize);

				// Volume
				volume.Value = Convert.ToDecimal(g.vars.Volume * 100f);

				// Is enable info panel
				info.Checked = g.vars.EnableInfo;

				// Update hot key sound element
				hotkeySound.Update(g.vars.hotkeySound);

				// Sound play delay
				soundDelay.Value = Convert.ToDecimal(g.vars.SoundPlayDelay);

				// update sound delay key
				soundDelayKey.Update(g.vars.SoundPlayDelayKey);

				// hide lSoundDelay element and update it
				// if global SoundPlayKeyDelay is Disabled
				//
				// and update soundDelayKey size
				lSoundDelay.Visible = g.vars.SoundPlayDelayKey != KeyDict.Disable;

				if (g.vars.SoundPlayDelayKey != KeyDict.Disable)
				{
					lSoundDelay.Top = soundDelayKey.DefaultHeight;
					lSoundDelay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
					lSoundDelay.Height = 50;
				}

				soundDelayKey.Height = g.vars.SoundPlayDelayKey != KeyDict.Disable ? soundDelayKey.DefaultHeight + 50 : soundDelayKey.DefaultHeight;


				// update element Enable scrolling
				EnableScrolling.Checked = g.vars.EnableScrolling;

				// grid size
				gridSize.Value = Convert.ToDecimal(g.vars.GridSize);

				g.MainFrame.UpdateElements();
			}
		}

		public TabPage GUITab()
		{
			TabPage page = new TabPage("GUI");
			page.AutoScroll = true;


			//
			// grid size
			//
			GroupBox lGridSize = new GroupBox();
			lGridSize.Parent = page;
			lGridSize.Text = "Grid Size";

			GUI.gridSize.Parent = lGridSize;
			GUI.gridSize.Minimum = 2;
			GUI.gridSize.Maximum = 50;
			GUI.gridSize.Dock = DockStyle.Top;
			GUI.gridSize.ValueChanged += (object obj, EventArgs args) =>
			{
				g.vars.GridSize = Convert.ToInt32(((NumericUpDown)obj).Value);

				GUI.SetupAllValues();
			};
			lGridSize.Dock = DockStyle.Top;
			lGridSize.Height = 50;


			//
			// autoscroll
			//
			GUI.EnableScrolling.Parent = page;
			GUI.EnableScrolling.Dock = DockStyle.Top;
			GUI.EnableScrolling.Text = "Enable scrollbars for main window";
			GUI.EnableScrolling.Click += (object obj, EventArgs args) =>
			{
				g.vars.EnableScrolling = ((CheckBox)obj).Checked;

				GUI.SetupAllValues();
			};


			//
			// sound delay track
			//
			GUI.lSoundDelay.Parent = GUI.soundDelayKey;
			GUI.lSoundDelay.Text = "Sound play delay while pressing binded keys";

			GUI.soundDelay.Parent = GUI.lSoundDelay;
			GUI.soundDelay.Minimum = 1;
			GUI.soundDelay.Maximum = 2000;
			GUI.soundDelay.Dock = DockStyle.Top;
			GUI.soundDelay.ValueChanged += (object obj, EventArgs args) =>
			{
				g.vars.SoundPlayDelay = Convert.ToInt32(((NumericUpDown)obj).Value);

				GUI.SetupAllValues();
			};


			//
			// sound delay bind
			//
			GUI.soundDelayKey.Parent = page;
			GUI.soundDelayKey.Dock = DockStyle.Top;
			GUI.soundDelayKey.BindChanged += (object obj, bool isCtrl, bool isAlt) =>
			{
				g.vars.SoundPlayDelayKey = BoolToKeyDict(isCtrl, isAlt);

				GUI.SetupAllValues();
			};


			//
			// enable play sound by hotkey
			//
			GUI.hotkeySound.Parent = page;
			GUI.hotkeySound.Dock = DockStyle.Top;
			GUI.hotkeySound.BindChanged += (object obj, bool isCtrl, bool isAlt) =>
			{
				g.vars.hotkeySound = BoolToKeyDict(isCtrl, isAlt);

				GUI.SetupAllValues();
			};


			//
			// enable info
			//
			GUI.info.Parent = page;
			GUI.info.Dock = DockStyle.Top;
			GUI.info.Text = "Enable info panel";
			GUI.info.Click += (object obj, EventArgs args) =>
			{
				g.vars.EnableInfo = ((CheckBox)obj).Checked;

				GUI.SetupAllValues();
			};


			//
			// font size
			//
			GroupBox lFontsize = new GroupBox();
			lFontsize.Parent = page;
			lFontsize.Text = "Font size";

			GUI.fontsize.Parent = lFontsize;
			GUI.fontsize.Minimum = 6;
			GUI.fontsize.Maximum = 48;
			GUI.fontsize.Dock = DockStyle.Fill;
			GUI.fontsize.ValueChanged += (object obj, EventArgs args) =>
			{
				g.vars.FontSize = Convert.ToInt32(GUI.fontsize.Value);

				GUI.SetupAllValues();

				SoundElement.RefontAllElements();
			};
			lFontsize.Dock = DockStyle.Top;
			lFontsize.Height = 50;


			//
			// volume
			//
			GroupBox lVolume = new GroupBox();
			lVolume.Parent = page;
			lVolume.Text = "Volume";

			GUI.volume.Parent = lVolume;
			GUI.volume.Minimum = 1;
			GUI.volume.Maximum = 100;
			GUI.volume.Dock = DockStyle.Fill;
			GUI.volume.ValueChanged += (object obj, EventArgs args) =>
			{
				g.vars.Volume = Convert.ToSingle(GUI.volume.Value) / 100f;

				GUI.SetupAllValues();
			};
			lVolume.Dock = DockStyle.Top;
			lVolume.Height = 50;


			// setup all values
			GUI.SetupAllValues();

			return page;
		}
	}
}
