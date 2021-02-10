#pragma warning disable CS1690


using System;

// json
using Newtonsoft.Json;
using System.Collections.Generic;

// file work
using System.IO;
using System.Security.Cryptography;

// forms
using soundboard.Frames;
using System.Text;
using System.Windows.Forms;


using static soundboard.SBEngine;
using static soundboard.Misc.Key;


namespace soundboard
{
	public class Config
	{
		public struct ConfigSound
		{
			public int ID;
			public int x, y;
			public int w, h;
			public string filename;

			public static ConfigSound Generate(Sound sound)
			{
				ConfigSound snd;

				snd.filename = sound.filename;
				snd.x = sound.x;
				snd.y = sound.y;
				snd.w = sound.w;
				snd.h = sound.h;
				snd.ID = sound.ID;

				return snd;
			}
		}

		//
		// config struct
		//
		public struct ConfigAppStructure
		{
			// play options
			public int DeviceID;
			public float Volume;

			// visual options
			public float FontSize;
			public bool EnableInfo;

			public KeyDict hotkeySound;
			public int SoundPlayDelay;
			public KeyDict SoundPlayDelayKey;

			public bool EnableScrolling;

			public int GridSize;

			//
			// constructor
			//
			public static ConfigAppStructure Generate()
			{
				ConfigAppStructure structure;

				structure.DeviceID = g.vars.DeviceID;
				structure.FontSize = g.vars.FontSize;
				structure.Volume = g.vars.Volume;
				structure.EnableInfo = g.vars.EnableInfo;

				structure.hotkeySound = g.vars.hotkeySound;
				structure.SoundPlayDelay = g.vars.SoundPlayDelay;
				structure.SoundPlayDelayKey = g.vars.SoundPlayDelayKey;

				structure.EnableScrolling = g.vars.EnableScrolling;

				structure.GridSize = g.vars.GridSize;

				return structure;
			}
		}

		const string SoundExt = "dsb_cfg";
		const string AppExt = "dsb_pp";
		string SoundFilter { get; } = $"{PName} Sounds (*.{SoundExt})|*.{SoundExt}";
		string AppFilter { get; } = $"{PName} App Config (*.{AppExt})|*.{AppExt}";
		string SoundFileFilter { get; } = $"Sound file (*.wav)|*.wav|Sound file (*.mp3)|*.mp3";

		FolderBrowserDialog file;

		OpenFileDialog openFile;
		OpenFileDialog openFileSnd;

		SaveFileDialog saveFile;

		OpenFileDialog openAppFile;
		SaveFileDialog saveAppFile;

		//
		// constructor
		//
		public Config()
		{
			// save
			file = new FolderBrowserDialog();

			// Open
			openFile = new OpenFileDialog();
			openFile.Filter = SoundFilter;

			// Open
			openFileSnd = new OpenFileDialog();
			openFileSnd.Filter = SoundFileFilter;

			// Save
			saveFile = new SaveFileDialog();
			saveFile.Filter = SoundFilter;

			// App
			// Open
			openAppFile = new OpenFileDialog();
			openAppFile.Filter = AppFilter;

			// Save
			saveAppFile = new SaveFileDialog();
			saveAppFile.Filter = AppFilter;
		}

		//
		// MD5
		//
		public static byte[] GetFileMD5(string path)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(path))
				{
					return md5.ComputeHash(stream);
				}
			}
		}

		//
		// add sound from file manager
		//
		public void AddSoundFromFileBrowser()
		{
			if (openFileSnd.ShowDialog() == DialogResult.OK)
			{
				g.engine.AddSound(openFileSnd.FileName);
			}
		}

		//
		// sound configuration
		//
		public void SaveOld()
		{
			if (saveFile.ShowDialog() == DialogResult.OK)
			{
				try
				{
					List<Sound> soundList = new List<Sound>();

					// get all Sound from SoundElements
					g.engine.soundElements.ForEach((SoundElement element) =>
					{
						soundList.Add(element.baseSound);
					});

					// export to string
					var data = JsonConvert.SerializeObject(soundList);

					// write to file
					using (var file = File.Open(saveFile.FileName, FileMode.OpenOrCreate))
					{
						file.SetLength(0);

						var bytes = Encoding.UTF8.GetBytes(data);

						file.Write(bytes, 0, bytes.Length);
					}
				}
				catch (Exception ex)
				{
					ThrowError($"Fail to save file using old method!\n\nException:\n{ex}");
				}
			}
		}

		public void SaveSound()
		{
			if (file.ShowDialog() == DialogResult.OK)
			{
				List<ConfigSound> soundList = new List<ConfigSound>();

				try
				{
					foreach (var el in g.engine.soundElements)
					{
						soundList.Add(ConfigSound.Generate(el.baseSound));

						var toPath = $"{file.SelectedPath}\\{el.baseSound.filename}";

						if (File.Exists(toPath))
						{
							var existing = GetFileMD5(toPath);
							var toReplace = GetFileMD5(el.baseSound.path);

							if (existing == toReplace)
								continue;

							if (existing != toReplace && MessageBox.Show($"File {toPath} not equal to {el.baseSound.path}\nShould replace?", "Ask, again?",
								MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
								File.Copy(el.baseSound.path, toPath);
							else
								continue;
						}

						File.Copy(el.baseSound.path, toPath);
					}

					// export to string
					var data = JsonConvert.SerializeObject(soundList);

					// write to file
					using (var _file = File.Open($"{file.SelectedPath}\\config.{SoundExt}", FileMode.OpenOrCreate))
					{
						_file.SetLength(0);

						var bytes = Encoding.UTF8.GetBytes(data);

						_file.Write(bytes, 0, bytes.Length);
					}
				}
				catch (Exception ex)
				{
					ThrowError($"Fail to save file!\n\nException:\n{ex}");
				}

				//
				// clean up
				//
				try
				{
					bool cleanask = true;

					List<string> soundListNames = new List<string>();
					foreach (var sound in soundList)
						soundListNames.Add(sound.filename);

					foreach (var _file in Directory.GetFiles(file.SelectedPath))
					{
						var split = _file.Split('\\');
						var filename = split[split.Length - 1];

						if (cleanask && filename != $"config.{SoundExt}" && !soundListNames.Contains(filename))
						{
							var result = MessageBox.Show($"Delete file {_file} because he's not use in this config\nYes - will delete file\nNo - will ignore\nCancel - No to all",
								$"{PName} ask a question.",
								MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

							if (cleanask && result != DialogResult.Yes)
								continue;
							else if (cleanask && result == DialogResult.Cancel)
								cleanask = false;

							File.Delete(_file);
						}
					}
				}
				catch (Exception ex)
				{
					ThrowError($"Fail to cleanup!\n\nException:\n{ex}");
				}
			};
		}

		public void LoadSoundPath(string configPath)
		{
			string json = File.OpenText(configPath).ReadToEnd();

			// deserialize object
			var obj = JsonConvert.DeserializeObject<List<ConfigSound>>(json);

			// Creating new elements
			obj.ForEach((ConfigSound sound) =>
			{
				g.engine.AddSound($"{file.SelectedPath}\\{sound.filename}", sound.x, sound.y, sound.w, sound.h);
			});
		}

		public void LoadOld()
		{
			if (openFile.ShowDialog() == DialogResult.OK)
			{
				g.engine.ClearSoundElements();

				try
				{
					if (!File.Exists(openFile.FileName))
						throw new Exception($"File not found: {openFile.FileName}");

					LoadSoundPath(openFile.FileName);
				}
				catch (Exception ex)
				{
					ThrowError($"Fail to load file using old method!\n\nException:\n{ex}");
				}
			}
		}

		public void LoadSound()
		{
			if (file.ShowDialog() == DialogResult.OK)
			{
				g.engine.ClearSoundElements();

				try
				{
					if (!File.Exists($"{file.SelectedPath}\\config.{SoundExt}"))
						throw new Exception($"File {file.SelectedPath}\\config.{SoundExt} not found");

					LoadSoundPath($"{file.SelectedPath}\\config.{SoundExt}");
				}
				catch (Exception ex)
				{
					ThrowError($"Fail to load file!\n\nException:\n{ex}");
				}
			}
		}

		//
		// app configuration
		//
		public void LoadAppPath(string path)
		{
			if(!File.Exists(path))
			{
				ThrowError($"File {path} not found");
				return;
			}

			try
			{
				// read file
				using (var file = File.OpenText(path))
				{
					string json = file.ReadToEnd();

					// deserialize object
					var obj = JsonConvert.DeserializeObject<ConfigAppStructure>(json);

					g.vars.DeviceID = obj.DeviceID;
					g.vars.FontSize = obj.FontSize;
					g.vars.Volume = obj.Volume;
					g.vars.EnableInfo = obj.EnableInfo;
					g.vars.SoundPlayDelay = obj.SoundPlayDelay;
					g.vars.SoundPlayDelayKey = obj.SoundPlayDelayKey;
					g.vars.hotkeySound = obj.hotkeySound;
					g.vars.EnableScrolling = obj.EnableScrolling;
					g.vars.GridSize = obj.GridSize;
				}
			}
			catch (Exception ex)
			{
				ThrowError($"Fail to load file!\n\nException:\n{ex}");
			}
		}

		public void LoadApp()
		{
			if (openAppFile.ShowDialog() == DialogResult.OK)
			{
				LoadAppPath(openAppFile.FileName);
			}
		}

		public void SaveApp()
		{
			// show save dialog
			//
			// then save if ok
			if (saveAppFile.ShowDialog() == DialogResult.OK)
			{
				try
				{
					// export to string
					var data = JsonConvert.SerializeObject(ConfigAppStructure.Generate());

					// write to file
					using (var file = File.Open(saveAppFile.FileName, FileMode.OpenOrCreate))
					{
						file.SetLength(0);

						var bytes = Encoding.UTF8.GetBytes(data);

						file.Write(bytes, 0, bytes.Length);
					}
				}
				catch (Exception ex)
				{
					ThrowError($"Fail to save file!\n\nException:\n{ex}");
				}
			};
		}
	}
}
