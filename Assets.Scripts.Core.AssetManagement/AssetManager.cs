using Assets.Scripts.Core.Audio;
using BGICompiler.Compiler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Core.AssetManagement
{
	public class AssetManager
	{
		private static AssetManager _instance;

		public bool UseNewArt = true;

		private Texture2D windowTexture;

		private string assetPath = Application.streamingAssetsPath;

		public int CurrentLoading;

		public int MaxLoading;

		public bool AbortLoading;

		private List<string> scriptList = new List<string>();

		public static AssetManager Instance => _instance ?? (_instance = GameSystem.Instance.AssetManager);

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
			array = files2;
			foreach (string path in array)
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
			Path.Combine(assetPath, "CompiledScripts");
			string destDir = Path.Combine(assetPath, "CompiledUpdateScripts");
			Directory.GetFiles(path, "*.txt");
			Directory.GetFiles(text, "*.txt");
			Debug.Log("Checking update scripts for updates...");
			CompileFolder(text, destDir);
			string[] files = Directory.GetFiles(Path.Combine(assetPath, "CompiledScripts"));
			string[] files2 = Directory.GetFiles(Path.Combine(assetPath, "CompiledUpdateScripts"));
			string[] array = files;
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
			array = files2;
			foreach (string path3 in array)
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

		public string FixPath(string path)
		{
			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				return path;
			}
			if (!path.Contains("\\"))
			{
				return path;
			}
			return path.Replace("\\", "/");
		}

		public Texture2D LoadScreenshot(string filename)
		{
			string savePath = MGHelper.GetSavePath();
			filename = FixPath(filename);
			string path = Path.Combine(savePath, filename.ToLower());
			if (!File.Exists(path))
			{
				return LoadTexture("no_data");
			}
			try
			{
				byte[] array = File.ReadAllBytes(path);
				byte[] array2 = new byte[4];
				Buffer.BlockCopy(array, 16, array2, 0, 4);
				int width = ReadLittleEndianInt32(array2);
				Buffer.BlockCopy(array, 20, array2, 0, 4);
				int height = ReadLittleEndianInt32(array2);
				Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
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

		public string LoadTextDataString(string dataName)
		{
			dataName = FixPath(dataName);
			return File.ReadAllText(Path.Combine(Path.Combine(Application.streamingAssetsPath, "Data"), dataName));
		}

		public List<string> LoadTextDataLines(string dataName)
		{
			dataName = FixPath(dataName);
			return File.ReadAllLines(Path.Combine(Path.Combine(Application.streamingAssetsPath, "Data"), dataName)).ToList();
		}

		public Texture2D LoadTexture(string textureName)
		{
			textureName = FixPath(textureName);
			if (textureName == "windo_filter" && windowTexture != null)
			{
				return windowTexture;
			}
			string path = Path.Combine(assetPath, "CG/" + textureName.ToLower() + "_j.png");
			string path2 = Path.Combine(assetPath, "CGAlt/" + textureName.ToLower() + "_j.png");
			string text = Path.Combine(assetPath, "CG/" + textureName.ToLower() + ".png");
			string path3 = Path.Combine(assetPath, "CGAlt/" + textureName.ToLower() + ".png");
			byte[] array = new byte[0];
			bool flag = false;
			if (!GameSystem.Instance.UseEnglishText)
			{
				if (UseNewArt && File.Exists(path2))
				{
					array = File.ReadAllBytes(path2);
					flag = true;
				}
				else if (File.Exists(path))
				{
					array = File.ReadAllBytes(path);
					flag = true;
				}
			}
			if (!flag)
			{
				if (UseNewArt && File.Exists(path3))
				{
					array = File.ReadAllBytes(path3);
				}
				else
				{
					if (!File.Exists(text))
					{
						Logger.LogWarning("Could not find texture asset " + text);
						return null;
					}
					array = File.ReadAllBytes(text);
				}
			}
			if (array == null || array.Length == 0)
			{
				throw new Exception("Failed loading texture " + textureName.ToLower());
			}
			byte[] array2 = new byte[4];
			Buffer.BlockCopy(array, 16, array2, 0, 4);
			int width = ReadLittleEndianInt32(array2);
			Buffer.BlockCopy(array, 20, array2, 0, 4);
			int height = ReadLittleEndianInt32(array2);
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: true);
			texture2D.mipMapBias = -0.5f;
			texture2D.LoadImage(array);
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.name = textureName;
			if (textureName == "windo_filter")
			{
				windowTexture = texture2D;
			}
			return texture2D;
		}

		public Cubemap LoadCubemap(string path)
		{
			Texture2D texture2D = LoadTexture(path);
			int height = texture2D.height;
			Cubemap cubemap = new Cubemap(texture2D.height, TextureFormat.RGB24, mipChain: false);
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
			filename = FixPath(filename);
			string archiveNameByAudioType = GetArchiveNameByAudioType(type);
			return Path.Combine(assetPath, archiveNameByAudioType + "/" + filename.ToLower());
		}

		public byte[] GetAudioFile(string filename, Assets.Scripts.Core.Audio.AudioType type)
		{
			filename = FixPath(filename);
			string archiveNameByAudioType = GetArchiveNameByAudioType(type);
			return File.ReadAllBytes(Path.Combine(assetPath, archiveNameByAudioType + "/" + filename.ToLower()));
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
