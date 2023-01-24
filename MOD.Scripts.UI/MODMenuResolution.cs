﻿using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MOD.Scripts.UI.MODMenuCommon;

namespace MOD.Scripts.UI
{
	class MODMenuResolution
	{
		private string screenHeightString;

		public MODMenuResolution()
		{
			screenHeightString = "";
		}

		public void OnBeforeMenuVisible()
		{
			screenHeightString = $"{Screen.height}";
		}

		public void OnGUI()
		{
			Label("Resolution Settings");
			{
				GUILayout.BeginHorizontal();
				if (Button(new GUIContent("480p", "Set resolution to 853 x 480"))) { SetAndSaveResolution(480); }
				if (Button(new GUIContent("720p", "Set resolution to 1280 x 720"))) { SetAndSaveResolution(720); }
				if (Button(new GUIContent("1080p", "Set resolution to 1920 x 1080"))) { SetAndSaveResolution(1080); }
				if (Button(new GUIContent("1440p", "Set resolution to 2560 x 1440"))) { SetAndSaveResolution(1440); }
				if (GameSystem.Instance.IsFullscreen)
				{
					if (Button(new GUIContent("Windowed", "Toggle Fullscreen")))
					{
						GameSystem.Instance.DeFullscreen(PlayerPrefs.GetInt("width"), PlayerPrefs.GetInt("height"));
					}
				}
				else
				{
					if (Button(new GUIContent("Fullscreen", "Toggle Fullscreen")))
					{
						GameSystem.Instance.GoFullscreen();
					}
				}

				screenHeightString = GUILayout.TextField(screenHeightString);
				if (Button(new GUIContent("Set", "Sets a custom resolution - mainly for windowed mode.\n\n" +
					"Height set automatically to maintain 16:9 aspect ratio.")))
				{
					if (int.TryParse(screenHeightString, out int new_height))
					{
						if (new_height < 480)
						{
							MODToaster.Show("Height too small - must be at least 480 pixels");
							new_height = 480;
						}
						else if (new_height > 15360)
						{
							MODToaster.Show("Height too big - must be less than 15360 pixels");
							new_height = 15360;
						}
						screenHeightString = $"{new_height}";
						int new_width = Mathf.RoundToInt(new_height * GameSystem.Instance.AspectRatio);
						GameSystem.Instance.SetResolution(new_width, new_height, Screen.fullScreen);
						PlayerPrefs.SetInt("width", new_width);
						PlayerPrefs.SetInt("height", new_height);
					}
				}
				GUILayout.EndHorizontal();
			}
		}

		private void SetAndSaveResolution(int height)
		{
			if (height < 480)
			{
				MODToaster.Show("Height too small - must be at least 480 pixels");
				height = 480;
			}
			else if (height > 15360)
			{
				MODToaster.Show("Height too big - must be less than 15360 pixels");
				height = 15360;
			}
			screenHeightString = $"{height}";
			int width = Mathf.RoundToInt(height * GameSystem.Instance.AspectRatio);
			GameSystem.Instance.SetResolution(width, height, Screen.fullScreen);
			PlayerPrefs.SetInt("width", width);
			PlayerPrefs.SetInt("height", height);
		}
	}
}
