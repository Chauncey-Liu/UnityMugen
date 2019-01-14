﻿using UnityEngine;
using System.Collections;
using Mugen;

public class AppConfig : MonoSingleton<AppConfig> {
	public int FPS = 30;
	public int ScreenSleepTime = 30;
	public int ResolutionWidth = 1280;
	public int ResolutionHeight = 720;
	public string PlayerRootDir = "resources/mugen/char/";
	public string SceneRootDir = "resources/mugen/scene/";
	public string PalleetMatFileName = "resources/mugen/@mat/palleetmaterial.mat";

	public IMugenLoader Loader = null;

    public static bool IsAppQuit = false;

    void OnApplicationQuit()
    {
        IsAppQuit = true;
    }
}
