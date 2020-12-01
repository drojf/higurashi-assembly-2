using Assets.Scripts.Core.Audio;
using Assets.Scripts.Core.Buriko;
using BGICompiler.Compiler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Core.AssetManagement
{

	/// <summary>
	/// Stores an ordered list of paths for the engine to check when trying to find a cg
	/// </summary>
	public class PathCascadeList {
		public readonly string nameEN;
		public readonly string nameJP;
		public readonly string[] paths;
		public PathCascadeList(string nameEN, string nameJP, string[] paths)
		{
			this.nameEN = nameEN;
			this.nameJP = nameJP;
			this.paths = paths;
		}
	}

	public class AssetManager {
		private static AssetManager _instance;

		public List<PathCascadeList> Artsets = new List<PathCascadeList>();
		public int CurrentArtsetIndex = 0;
		public int ArtsetCount => Artsets.Count == 0 ? 2 : Artsets.Count;
		public PathCascadeList CurrentArtset => GetArtset(CurrentArtsetIndex);
		public bool ShouldSerializeArtsets = false;

		private Texture2D windowTexture;
		private string windowTexturePath = string.Empty;
		private Texture2D dummyTexture;

		private string assetPath = Application.streamingAssetsPath;

		public int CurrentLoading;

		public int MaxLoading;

		public bool AbortLoading;

		private List<string> scriptList = new List<string>();

		public static AssetManager Instance => _instance ?? (_instance = GameSystem.Instance.AssetManager);

		/// <summary>
		/// Get the artset at the given index
		/// </summary>
		/// <param name="index">The index of the artset to get</param>
		/// <returns></returns>
		public PathCascadeList GetArtset(int index)
		{
			// To maintain compatibility with scripts that don't specify artsets, if none have been added act like the base game
			if (Artsets.Count == 0)
			{
				if (index == 0)
				{
					return new PathCascadeList("Console", "ゲーム機", new string[] { "CG" });
				}
				if (index == 1)
				{
					return new PathCascadeList("Remake", "リメーク", new string[] { "CGAlt", "CG" });
				}
			}
			if (index >= 0 && index < Artsets.Count)
			{
				return Artsets[index];
			}
			return new PathCascadeList("Unknown (" + index + ")", "不明(" + index + ")", new string[] { "CG" });
		}

		public void AddArtset(PathCascadeList artset)
		{
			Artsets.Add(artset);
		}

		public void ClearArtsets()
		{
			Artsets.Clear();
		}

		/// <summary>
		/// Gets the path to an asset with the given name in the given artset, or null if none are found
		/// </summary>
		/// <returns>A path to an on-disk asset or null</returns>
		public string PathToAssetWithName(string name, PathCascadeList artset)
		{
			int backgroundSetIndex = BurikoMemory.Instance.GetGlobalFlag("GBackgroundSet").IntValue();

			// If force og backgrounds is enabled, always check OGBackgrounds first.
			if (backgroundSetIndex == 2)
			{
				string filePath = Path.Combine(Path.Combine(assetPath, "OGBackgrounds"), name);
				if (File.Exists(filePath))
				{
					return filePath;
				}
			}

			foreach (var artSetPath in artset.paths)
			{
				// If force console backgrounds is enabled, don't check OGBackgrounds
				if (backgroundSetIndex == 1 && artSetPath == "OGBackgrounds")
				{
					continue;
				}

				string filePath = Path.Combine(Path.Combine(assetPath, artSetPath), name);
				if (File.Exists(filePath))
				{
					return filePath;
				}
			}
			return null;
		}

		public void CompileFolder(string srcDir, string destDir)
		{
			string[] files = Directory.GetFiles(srcDir, "*.txt");
			string[] files2 = Directory.GetFiles(destDir, "*.mg");
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			string[] array = files;
			foreach (string text in array)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				if (fileNameWithoutExtension == null)
				{
					continue;
				}
				list.Add(fileNameWithoutExtension);
				string text2 = text;
				string text3 = Path.Combine(destDir, fileNameWithoutExtension) + ".mg";
				if (File.Exists(text3))
				{
					if (File.GetLastWriteTime(text2) <= File.GetLastWriteTime(text3))
					{
						continue;
					}
					Debug.Log($"Script {text3} last compiled {File.GetLastWriteTime(text3)} (source {text2} updated on {File.GetLastWriteTime(text2)})");
				}
				list2.Add(text2);
				list3.Add(text3);
			}
			MaxLoading = list2.Count;
			for (int j = 0; j < list2.Count; j++)
			{
				CurrentLoading = j + 1;
				string text4 = list2[j];
				string outname = list3[j];
				string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text4);
				Debug.Log("Compiling file " + text4);
				try
				{
					new BGItoMG(text4, outname);
				}
				catch (Exception arg)
				{
					Debug.LogError($"Failed to compile script {fileNameWithoutExtension2}!\r\n{arg}");
				}
				if (AbortLoading)
				{
					return;
				}
			}
			string[] array2 = files2;
			foreach (string path in array2)
			{
				string fileNameWithoutExtension3 = Path.GetFileNameWithoutExtension(path);
				if (!list.Contains(fileNameWithoutExtension3))
				{
					Debug.Log("Compiled script " + fileNameWithoutExtension3 + " has no matching script file. Removing...");
					File.Delete(path);
				}
			}
		}

		public void CompileIfNeeded()
		{
			string path = Path.Combine(assetPath, "Scripts");
			string text = Path.Combine(assetPath, "Update");
			string text2 = Path.Combine(assetPath, "CompiledScripts");
			string destDir = Path.Combine(assetPath, "CompiledUpdateScripts");
			string[] files = Directory.GetFiles(path, "*.txt");
			string[] files2 = Directory.GetFiles(text, "*.txt");
			Debug.Log("Checking update scripts for updates...");
			CompileFolder(text, destDir);
			string[] files3 = Directory.GetFiles(Path.Combine(assetPath, "CompiledScripts"));
			string[] files4 = Directory.GetFiles(Path.Combine(assetPath, "CompiledUpdateScripts"));
			string[] array = files3;
			foreach (string path2 in array)
			{
				if (!(Path.GetExtension(path2) != ".mg"))
				{
					string fileName = Path.GetFileName(path2);
					if (!scriptList.Contains(fileName))
					{
						scriptList.Add(fileName);
					}
				}
			}
			string[] array2 = files4;
			foreach (string path3 in array2)
			{
				if (!(Path.GetExtension(path3) != ".mg"))
				{
					string fileName2 = Path.GetFileName(path3);
					if (!scriptList.Contains(fileName2))
					{
						scriptList.Add(fileName2);
					}
				}
			}

			// If we want to use the game just to compile scripts in an automated manner, we need
			// some way to terminate the game after scripts are compiled.
			// The below code will terminate the game after scripts are compiled if "quitaftercompile"
			// is passed as a command-line argument to the game
			// The code will also try to write a higu_script_compile_status.txt as proof that
			// the scripts really did compile OK
			if (Environment.GetCommandLineArgs().Contains("quitaftercompile"))
			{
				GameSystem.Instance.CanExit = true;
				try
				{
					System.IO.File.WriteAllText("higu_script_compile_status.txt", "Compile OK");
				}
				catch
				{

				}
				Application.Quit();
			}
		}

		private string GetArchiveNameByAudioType(Assets.Scripts.Core.Audio.AudioType audioType)
		{
			switch (audioType)
			{
			case Assets.Scripts.Core.Audio.AudioType.BGM:
				return "BGM";
			case Assets.Scripts.Core.Audio.AudioType.Voice:
				return "voice";
			case Assets.Scripts.Core.Audio.AudioType.SE:
				return "SE";
			case Assets.Scripts.Core.Audio.AudioType.System:
				return "SE";
			default:
				throw new InvalidEnumArgumentException("GetArchiveNameByAudioType: Invalid audiotype " + audioType);
			}
		}

		private static int ReadLittleEndianInt32(byte[] bytes)
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[3 - i] = bytes[i];
			}
			return BitConverter.ToInt32(array, 0);
		}

		public Texture2D LoadScreenshot(string filename)
		{
			string savePath = MGHelper.GetSavePath();
			string path = Path.Combine(savePath, filename.ToLower());
			if (File.Exists(path))
			{
				try
				{
					byte[] array = File.ReadAllBytes(path);
					byte[] array2 = new byte[4];
					Buffer.BlockCopy(array, 16, array2, 0, 4);
					int width = ReadLittleEndianInt32(array2);
					Buffer.BlockCopy(array, 20, array2, 0, 4);
					int height = ReadLittleEndianInt32(array2);
					Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipmap: false);
					texture2D.LoadImage(array);
					texture2D.filterMode = FilterMode.Bilinear;
					texture2D.wrapMode = TextureWrapMode.Clamp;
					return texture2D;
				}
				catch (Exception)
				{
					return LoadTexture("no_data");
				}
			}
			return LoadTexture("no_data");
		}

		public string LoadTextDataString(string dataName)
		{
			string path = Path.Combine(Application.streamingAssetsPath, "Data");
			return File.ReadAllText(Path.Combine(path, dataName));
		}

		public List<string> LoadTextDataLines(string dataName)
		{
			string path = Path.Combine(Application.streamingAssetsPath, "Data");
			return File.ReadAllLines(Path.Combine(path, dataName)).ToList();
		}

		public Texture2D LoadTexture(string textureName)
		{
			return LoadTexture(textureName, out _);
		}

		public Texture2D LoadTexture(string textureName, out string texturePath)
		{
			if (textureName == "windo_filter" && windowTexture != null)
			{
				texturePath = windowTexturePath;
				return windowTexture;
			}
			string path = null;

			// Load path from current artset
			if (path == null && !GameSystem.Instance.UseEnglishText)
			{
				path = PathToAssetWithName(textureName.ToLower() + "_j.png", CurrentArtset);
			}

			if (path == null)
			{
				path = PathToAssetWithName(textureName.ToLower() + ".png", CurrentArtset);
			}

			if (path == null)
			{
				Logger.LogWarning("Could not find texture asset " + textureName.ToLower() + " in " + CurrentArtset.nameEN);
				// When returning null here, most functions won't crash, but this call chain does crash:
				// OperationDrawSpriteWithFiltering() -> DrawSpriteWithFiltering() -> DrawLayerWithMask()
				// Returning a dummy texture instead of null prevents these crashes
				if (dummyTexture == null)
				{
					dummyTexture = new Texture2D(0, 0, TextureFormat.ARGB32, mipmap: true);
				}
				texturePath = "dummy_texture";
				return dummyTexture;
			}
			byte[] array = File.ReadAllBytes(path);
			if (array == null || array.Length == 0)
			{
				throw new Exception("Failed loading texture " + path);
			}
			byte[] array2 = new byte[4];
			Buffer.BlockCopy(array, 16, array2, 0, 4);
			int width = ReadLittleEndianInt32(array2);
			Buffer.BlockCopy(array, 20, array2, 0, 4);
			int height = ReadLittleEndianInt32(array2);
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipmap: true);
			texture2D.mipMapBias = -0.5f;
			texture2D.LoadImage(array);
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.name = textureName;
			if (textureName == "windo_filter")
			{
				windowTexture = texture2D;
				windowTexturePath = path;
			}
			texturePath = path;
			return texture2D;
		}

		public Cubemap LoadCubemap(string path)
		{
			Texture2D texture2D = LoadTexture(path);
			int height = texture2D.height;
			Cubemap cubemap = new Cubemap(texture2D.height, TextureFormat.RGB24, mipmap: false);
			Color[] pixels = texture2D.GetPixels(0, 0, height, height);
			cubemap.SetPixels(pixels, CubemapFace.PositiveX);
			pixels = texture2D.GetPixels(height, 0, height, height);
			cubemap.SetPixels(pixels, CubemapFace.NegativeX);
			pixels = texture2D.GetPixels(height * 2, 0, height, height);
			cubemap.SetPixels(pixels, CubemapFace.PositiveY);
			pixels = texture2D.GetPixels(height * 3, 0, height, height);
			cubemap.SetPixels(pixels, CubemapFace.NegativeY);
			pixels = texture2D.GetPixels(height * 4, 0, height, height);
			cubemap.SetPixels(pixels, CubemapFace.PositiveZ);
			pixels = texture2D.GetPixels(height * 5, 0, height, height);
			cubemap.SetPixels(pixels, CubemapFace.NegativeZ);
			cubemap.Apply();
			UnityEngine.Object.Destroy(texture2D);
			return cubemap;
		}

		public string GetAudioFilePath(string filename, Assets.Scripts.Core.Audio.AudioType type)
		{
			string archiveNameByAudioType = GetArchiveNameByAudioType(type);
			string text = null;
			string text2 = Path.Combine(assetPath, archiveNameByAudioType + "/" + filename.ToLower());
			string text3 = Path.Combine(assetPath, archiveNameByAudioType + "Alt/" + filename.ToLower());
			switch (archiveNameByAudioType)
			{
				case "BGM":
					if (BurikoMemory.Instance.GetGlobalFlag("GAltBGM").IntValue() != 0)
					{
						if (File.Exists(text3))
						{
							return text3;
						}
						break;
					}
					goto default;
				case "SE":
					if (BurikoMemory.Instance.GetGlobalFlag("GAltSE").IntValue() != 0)
					{
						if (File.Exists(text3))
						{
							return text3;
						}
						break;
					}
					goto default;
				case "voice":
					if (BurikoMemory.Instance.GetGlobalFlag("GAltVoice").IntValue() == 0)
					{
						break;
					}
					if (BurikoMemory.Instance.GetGlobalFlag("GAltVoicePriority").IntValue() != 0)
					{
						if (File.Exists(text3))
						{
							return text3;
						}
						break;
					}
					goto default;
				default:
					if (!File.Exists(text2))
					{
						return text3;
					}
					break;
			}
			return text2;
		}

		public byte[] GetAudioFile(string filename, Assets.Scripts.Core.Audio.AudioType type)
		{
			string archiveNameByAudioType = GetArchiveNameByAudioType(type);
			string path = Path.Combine(assetPath, archiveNameByAudioType + "/" + filename.ToLower());
			return File.ReadAllBytes(path);
		}

		public byte[] GetScriptData(string filename)
		{
			string path = Path.Combine(assetPath, "CompiledUpdateScripts/" + filename.ToLower());
			if (File.Exists(path))
			{
				Debug.Log("Loading script " + filename + " from update folder.");
				return File.ReadAllBytes(path);
			}
			path = Path.Combine(assetPath, "CompiledScripts/" + filename.ToLower());
			return File.ReadAllBytes(path);
		}

		public string[] GetAvailableScriptNames()
		{
			return scriptList.ToArray();
		}
	}
}
