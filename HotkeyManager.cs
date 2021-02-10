using System;

using System.Threading;

using static soundboard.Misc.WinAPI;
using static soundboard.Misc.Key;

namespace soundboard
{
	public class HotkeyManager
	{
		public Thread thread;

		public static int Tick = 0;

		//
		// constructor
		//
		public HotkeyManager()
		{
			thread = new Thread(() => Think());

			thread.Start();
		}

		//
		// Think function
		//
		public static VK last = VK.NONE;
		public static int lastT = 0;

		public static bool IsKeyDown(VK vk)
		{
			if (GetAsyncKeyState(vk))
			{
				last = vk;

				return true;
			}

			return false;
		}

		public string trackNumber = null;

		public void Think()
		{
			while (g.ProgramWorking)
			{
				Thread.Sleep(1);

				if (g.vars.hotkeySound == KeyDict.Disable)
					continue;

				Tick++;

				if (IsPressing(g.vars.hotkeySound))
				{
					if (trackNumber == null)
						trackNumber = "";

					for (int num = 0; num <= 9; num++)
					{
						int key = (int)VK.NUMPAD0 + num;

						if (IsKeyDown((VK)key))
						{
							if ((VK)key == last && lastT > Tick)
							{
								lastT++;

								continue;
							}

							lastT = Tick + 2;

							trackNumber += num.ToString()[0];
						}
					}
				}
				else
				{
					if (trackNumber != null && trackNumber.Length > 0)
					{
						int track = Convert.ToInt32(trackNumber);

						Console.WriteLine($"Track number: {trackNumber}");

						g.engine.PlaySound(track);
					}

					trackNumber = null;
				}
			}
		}
	}
}
