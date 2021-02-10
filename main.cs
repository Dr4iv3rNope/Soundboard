using System;

// forms
using System.Windows.Forms;
using soundboard.Frames;

using static soundboard.Misc.Key;

using soundboard.Misc;

namespace soundboard
{
	public static class g
	{
		public static class vars
		{
			public static int DeviceID { get; set; } = -2;
			public static float FontSize { get; set; } = 10f;
			public static float Volume { get; set; } = 1f;
			public static bool EnableInfo { get; set; } = true;
			public static KeyDict hotkeySound { get; set; } = KeyDict.Disable;
			public static int SoundPlayDelay { get; set; } = 1;
			public static KeyDict SoundPlayDelayKey { get; set; } = KeyDict.Ctrl;
			public static bool EnableScrolling { get; set; } = true;
			public static int GridSize { get; set; } = 4;
		}

		// const
		public static bool Debug { get; set; } = false;
		public static string CurrentPath { get; set; }
		public static bool ProgramWorking { get; set; } = true;

		// workers
		public static SBEngine engine { get; set; }
		public static Frame MainFrame { get; set; }
		public static WindowSettings Settings { get; set; }
		public static HotkeyManager hotkeyManager { get; set; }
		public static TracksFrame tracksFrame { get; set; }

		// methods
		public static void ReloadAll()
		{
			engine.StopSound(true);
			WindowSettings.GUI.SetupAllValues();
			SoundElement.RefontAllElements();
			SoundElement.RetextAllElements();
			WindowSettings.DeviceButton.ReloadDeviceButtons();
			MainFrame.UpdateElements();
		}
	}

	public static class main
	{
		[STAThread]
		static void Main(string[] args)
		{
			// g.debug true if defined DEBUG
#if DEBUG
			g.Debug = true;
#endif

			var split = Application.ExecutablePath.Split('\\');
			g.CurrentPath = Application.ExecutablePath.Replace(split[split.Length - 1], "");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// init sound engine
			g.engine = new SBEngine();

			// init frame
			g.MainFrame = new Frame();

			g.Settings = new WindowSettings();
			g.tracksFrame = new TracksFrame();

			g.hotkeyManager = new HotkeyManager();

			//
			// arg controller
			//
			ArgsController argsController = new ArgsController('-', args);
			var enableConsole = new ArgsController.Arg("enableConsole", ref argsController, null, () => { if (!g.Debug) WinAPI.FreeConsole(); });
			var appSettings = new ArgsController.Arg("appSettings", ref argsController, (ArgStruct arg) => g.engine.config.LoadAppPath(arg.value), () => g.engine.config.LoadApp());
			argsController.CheckArgs();

			// setup font
			SoundElement.paintFont = SoundElement.CreateFont();

			g.ReloadAll();
			Application.Run(g.MainFrame);
		}
	}
}
