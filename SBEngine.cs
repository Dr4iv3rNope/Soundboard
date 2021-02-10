#pragma warning disable CS1690

using System;

using System.Collections.Generic;

// file
using System.IO;

using System.Threading;

// forms
using System.Linq;
using soundboard.Frames;
using System.Windows.Forms;

// sound
using Un4seen.Bass;

using soundboard.Misc;

namespace soundboard
{
	//
	// engine
	//
	public class SBEngine
	{
		public static string PName { get; } = "Dr4iv3r SoundBoard";
		public static string EName { get; } = "SBEngine";

		//
		// Error work
		//
		public static void ThrowError(string msg)
		{
			MessageBox.Show(msg, $"{EName} error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void ThrowCritical(string msg, int exitCode)
		{
			var result = MessageBox.Show(msg + "\n\nShutdown program?", $"{EName} critical error! N:{exitCode}",
				MessageBoxButtons.YesNo, MessageBoxIcon.Error);

			if (result == DialogResult.Yes)
				Environment.Exit(exitCode);
		}

		public static void ThrowWarning(string msg)
		{
			MessageBox.Show(msg, $"{EName} warning.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		//
		// sound structure
		//
		public struct Sound
		{
			public static Sound NULL() => new Sound(-1, null, -1, -1, -1, -1);

			public bool IsNULL() =>
				ID == -1 ||
				x == -1 || y == -1 ||
				w == -1 || h == -1 ||
				filename == null;

			// ID of element
			public int ID;

			// path
			public string path;

			// position
			public int x, y;

			// size
			public int w, h;

			public string filename;
			public string name() => filename != null ? filename.Replace('.' + filename.Split('.').Last(), "") : null;

			// checks if sound is actually sound
			public bool IsSound() => File.Exists(path) && (path.EndsWith(".mp3") || path.EndsWith(".wav"));

			public static bool IsSound(string path) => File.Exists(path) && (path.EndsWith(".mp3") || path.EndsWith(".wav"));

			// contructor
			//
			// better to use contructor instead self fill struct
			public Sound(int ID, string path, int x = 0, int y = 0, int w = 100, int h = 20)
			{
				this.ID = ID;

				// position
				this.x = x;
				this.y = y;

				// size
				this.w = w;
				this.h = h;

				if ((path != null && !IsSound(path)))
					ThrowWarning($"File {path} is not supported.");

				this.path = path;
				this.filename = path != null ? path.Split('\\').Last() : null;
			}

			// operators
			public static bool operator ==(Sound a, Sound b) => a.path == b.path;
			public static bool operator !=(Sound a, Sound b) => a.path != b.path;

			// tostring
			public override string ToString() => path;
			public override bool Equals(object obj) => base.Equals(obj);
			public override int GetHashCode() => base.GetHashCode();
		}

		// sound list
		public List<SoundElement> soundElements { get; }
		public Thread eventTimer { get; }

		// events
		public delegate void TrackStartFn(Sound sound, bool isDelay);
		public event TrackStartFn OnTrackStart;

		public delegate void TrackFinishFn(Sound sound);
		public event TrackFinishFn OnTrackFinish;

		public delegate void TrackAddFn(SoundElement sound);
		public event TrackAddFn OnTrackAdd;

		//
		// Constructor
		//
		public SBEngine()
		{
			currentSound = Sound.NULL();
			soundElements = new List<SoundElement>();

			// init timer
			eventTimer = new Thread(() =>
		   {
			   while (g.ProgramWorking)
			   {
				   Thread.Sleep(10);

				   if (Stream != 0 && !isOnDelay && Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !currentSound.IsNULL())
				   {
					   StopSound(false);
				   }
			   }
		   });

			eventTimer.Start();
		}

		public Sound currentSound;

		public bool isOnDelay = false;
		public Thread playSoundThread = null;

		public int Stream;

		//
		// Methods
		//
		// play sound
		public void PlaySound(Sound sound)
		{
			if (g.vars.DeviceID == -2 || g.vars.DeviceID > Bass.BASS_GetDeviceCount())
			{
				ThrowWarning($"Please choose playback device in Options");
				return;
			}

			if (!currentSound.IsNULL())
				StopSound(true);

			if (!sound.IsSound())
				ThrowError($"Tried to play not valid sound {sound}");
			else
			{
				playSoundThread = new Thread(() =>
			   {
				   isOnDelay = true;
				   currentSound = sound;

				   OnTrackStart?.Invoke(sound, true);
				   
				   if (Key.IsPressing(g.vars.SoundPlayDelayKey))
					   Thread.Sleep(g.vars.SoundPlayDelay);

				   OnTrackStart?.Invoke(sound, false);

				   Bass.BASS_Init(g.vars.DeviceID, 192000, BASSInit.BASS_DEVICE_STEREO, IntPtr.Zero);
				   Stream = Bass.BASS_StreamCreateFile(sound.path, 0, 0, BASSFlag.BASS_DEFAULT);

				   if (Stream != 0)
				   {
					   isOnDelay = false;

					   Bass.BASS_ChannelSetDevice(Stream, g.vars.DeviceID);
					   Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, g.vars.Volume);
					   Bass.BASS_ChannelPlay(Stream, false);
				   }
			   });

				playSoundThread.Start();
			}
		}

		public void PlaySound(int trackNumber)
		{
			foreach (var el in soundElements)
			{
				if (el.baseSound.ID == trackNumber)
					PlaySound(el.baseSound);
			}
		}

		// deletes all elements
		public void ClearSoundElements()
		{
			foreach (var element in soundElements.ToList())
				element.Remove();

			soundElements.Clear();
		}

		// stop sound
		public void StopSound(bool forcestop = false)
		{
			if (playSoundThread != null && playSoundThread.IsAlive)
			{
				if (forcestop)
					playSoundThread.Abort();
				else
					return;
			}

			OnTrackFinish?.Invoke(currentSound);

			currentSound = Sound.NULL();

			Bass.BASS_ChannelStop(Stream);
			Bass.BASS_StreamFree(Stream);
		}

		public void AddSound(string path, int x = 0, int y = 0, int w = 100, int h = 20)
		{
			if (!Sound.IsSound(path))
				return;

			// check if sound already exists
			soundElements.ForEach((SoundElement element) =>
		   {
			   if (element.baseSound.filename == path)
				   return;
		   });

			var sound = new Sound(soundElements.Count() + 1, path);

			var NewElement = new SoundElement(sound);
			NewElement.Parent = g.MainFrame.global;

			NewElement.OnRemove += (SoundElement self) => soundElements.Remove(self);

			NewElement.SetPos(x, y);
			NewElement.SetSize(w, h);

			OnTrackAdd?.Invoke(NewElement);
		}

		// removes sound
		public void RemoveSound(int ID)
		{
			foreach (var el in soundElements.ToList())
			{
				if(el.baseSound.ID == ID)
				{
					el.Remove();
					break;
				}
			}
		}

		//
		// Config system
		//
		public Config config = new Config();
	}
}
