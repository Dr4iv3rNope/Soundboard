using System.Runtime.InteropServices;

namespace soundboard.Misc
{
	public static class WinAPI
	{
		public enum VK
		{
			NONE = -1,
			SHIFT = 0x10,
			CONTROL,
			ALT,
			NUMPAD0 = 0x60,
			NUMPAD1,
			NUMPAD2,
			NUMPAD3,
			NUMPAD4,
			NUMPAD5,
			NUMPAD6,
			NUMPAD7,
			NUMPAD8,
			NUMPAD9
		}


		[DllImport("user32.dll")]
		public static extern bool GetAsyncKeyState(VK key);

		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();
	}
}
