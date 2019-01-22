﻿using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mugen;

public enum GlobalPlayerLoaderResult
{
	CNSConfigError = -4,
	AirConfigError = -3,
	PlayerConfigError = -2,
	ParamError = -1,
	Ok = 0,
	None = 1,
}

public class GlobalConfigMgr : MonoSingleton<GlobalConfigMgr> {
	private Dictionary<string, GlobalPlayer> m_PlayerDict = new Dictionary<string, GlobalPlayer>();
	private List<SpriteRenderer> m_ClsnSpritePool = new List<SpriteRenderer>();
	private List<BoxCollider2D> m_ClsnColliderPool = new List<BoxCollider2D> ();
	private GameObject m_ClsnSpritePoolRoot = null;
	private GameObject m_ClsnColliderPoolRoot = null;
	private BaseResLoader m_Loader = null;
	private Texture m_ClsnTex = null;

	protected SpriteRenderer GetClsnSpriteFromPool(string name)
	{
		if (m_ClsnSpritePoolRoot == null)
			return null;
		var trans = m_ClsnSpritePoolRoot.transform;
		if (trans.childCount > 0) {
			var child = trans.GetChild (trans.childCount - 1);
			child.gameObject.SetActive (false);
			child.SetParent (null, false);
			child.name = name;
			SpriteRenderer ret = child.GetComponent<SpriteRenderer> ();
			return ret;
		}
		GameObject gameObj = new GameObject(name, typeof(SpriteRenderer));
		SpriteRenderer r = gameObj.GetComponent<SpriteRenderer> ();
		return r;
	}

	protected Clsn GetClsnColliderFromPool(string name)
	{
		if (m_ClsnColliderPoolRoot == null)
			return null;
		var trans = m_ClsnColliderPoolRoot.transform;
		if (trans.childCount > 0) {
			var child = trans.GetChild (trans.childCount - 1);
			child.gameObject.SetActive (false);
			child.SetParent (null, false);
			child.name = name;
            Clsn ret = child.GetComponent<Clsn>();
			return ret;
		}
        GameObject gameObj = new GameObject(name, typeof(Clsn));
        Clsn r = gameObj.GetComponent<Clsn>();
		return r;
	}

    public Clsn CreateClsnBox(InputPlayerType playerType, string name, Transform parent, float x, float y, float w, float h, bool isClsn2 = true)
    {
        Clsn r = GetClsnColliderFromPool(name);
        if (r == null)
            return r;

        var trans = r.transform;
        if (trans.parent != parent)
        {
            trans.SetParent(parent, false);
        }

        ClsnType tt = isClsn2 ? ClsnType.def: ClsnType.attack;
        r.Init(playerType, tt, x, y, w, h);

        if (!r.gameObject.activeSelf)
            r.gameObject.SetActive(true);

        return r;
    }

	/// <summary>
	/// 创建包围盒查看
	/// </summary>
	/// <returns>包围盒节点</returns>
	/// <param name="x">像素位置X</param>
	/// <param name="y">像素位置Y</param>
	/// <param name="w">像素宽度</param>
	/// <param name="h">像素高度</param>
	/// <param name="offsetX">像素偏移X</param>
	/// <param name="offsetY">像素偏移Y</param>
	/// <param name="isClsn2">If set to <c>true</c> 防御盒，否则攻击盒</param>
	public SpriteRenderer CreateClsnSprite(string name, Transform parent, float x, float y, float w, float h, bool isClsn2 = true)
	{
		SpriteRenderer r = GetClsnSpriteFromPool (name);
		if (r == null)
			return null;
		var trans = r.transform;
		if (trans.parent != parent) {
			trans.SetParent (parent, false);
		}

		if (m_ClsnTex == null) {
			m_Loader.LoadTexture (ref m_ClsnTex, "resources/mugen/@clsn/clsn.png");
		}
		Texture2D tex = m_ClsnTex as Texture2D;
		if (tex != null) {
			r.sprite = Sprite.Create (tex, new Rect (0, 0, 
				tex.width, tex.height), new Vector2 (0.5f, 0.5f), 100f);
			trans.localScale = new Vector3(w / (float)tex.width, h/(float)tex.height, 1f);
		} else
			trans.localScale = Vector3.zero;
        if (r.sharedMaterial == null || string.IsNullOrEmpty(r.sharedMaterial.name) || 
            r.sharedMaterial.name.IndexOf("clsn", StringComparison.CurrentCultureIgnoreCase) < 0)
            m_Loader.LoadMaterial(r, "resources/mugen/@clsn/clsn.mat");

		Vector2 pos = new Vector2 ((x + w/2f)/100f, -(y + h/2f)/100f);
		trans.localPosition = pos;
		trans.localRotation = Quaternion.identity;

        if (isClsn2)
        {
            r.color = new Color(0, 0, 1f, 0.2f);
        } else
        {
            r.color = new Color(1f, 0, 0, 0.2f);
        }


		if (!r.gameObject.activeSelf)
			r.gameObject.SetActive (true);
		return r;
	}

	public void DestroyClsn(SpriteRenderer r)
	{
		if (r == null)
			return;
		if (r.transform.parent == m_ClsnSpritePoolRoot.transform)
			return;
		if (m_Loader != null && r.sprite != null) {
			//
		}
		r.sprite = null;
		r.transform.SetParent (m_ClsnSpritePoolRoot.transform, false);
	}

    public void DestroyBoxCollider(Clsn box)
    {
        if (box == null)
            return;
        if (box.transform.parent == m_ClsnColliderPoolRoot.transform)
            return;

        box.transform.SetParent(m_ClsnColliderPoolRoot.transform, false);
    }

	// 默认加载的PlayerName, 可用于测试
	public List<DefaultLoaderPlayer> m_DefaultLoaderPlayers = null;

    public bool HasLoadPlayer(DefaultLoaderPlayer loaderPlayer)
    {
        if (loaderPlayer == null)
            return false;
        string playerName = loaderPlayer.GetPlayerName();
        return m_PlayerDict.ContainsKey(playerName);
    }

	public GlobalPlayer LoadPlayer(DefaultLoaderPlayer loaderPlayer, out GlobalPlayerLoaderResult result)
	{
		if (loaderPlayer == null) {
			result = GlobalPlayerLoaderResult.ParamError;
			return null;
		}
		string playerName = loaderPlayer.GetPlayerName();
		GlobalPlayer ret = LoadPlayer (playerName, out result, loaderPlayer.CnsName);
        return ret;
	}

	private GlobalPlayer LoadPlayer(string playerName, out GlobalPlayerLoaderResult result, string cnsName = "")
	{
		result = GlobalPlayerLoaderResult.Ok;
		GlobalPlayer player;
		if (m_PlayerDict.TryGetValue (playerName, out player) && player != null)
			return player;
		player = GlobalPlayer.CreatePlayer (playerName, out result, cnsName);
		if (player != null) {
			m_PlayerDict [playerName] = player;
		}
		return player;
	}

	public void Clear()
	{
		m_PlayerDict.Clear ();
	}

	private void AddCnsList(DefaultLoaderPlayer loaderPlayer, GlobalPlayer player)
	{
		if (player == null || player.PlayerCfg == null || player.PlayerCfg.Files == null) {
			loaderPlayer.CnsNameList = null;
		} else {
			var files = player.PlayerCfg.Files;
			string name = GetConfigFileNameNoExt (files.cns);
			loaderPlayer.AddCnsName (name);
			name = GetConfigFileNameNoExt (files.st);
			loaderPlayer.AddCnsName (name);
			name = GetConfigFileNameNoExt (files.st2);
			loaderPlayer.AddCnsName (name);
			name = GetConfigFileNameNoExt (files.st3);
			loaderPlayer.AddCnsName (name);
			name = GetConfigFileNameNoExt (files.st4);
			loaderPlayer.AddCnsName (name);
		}
	}

	private void AttachAnim(DefaultLoaderPlayer loaderPlayer, GlobalPlayer player)
	{
		if (player == null || player.PlayerCfg == null || player.PlayerCfg.Files == null) {
			loaderPlayer.LoadAnim = string.Empty;
		} else {
			var files = player.PlayerCfg.Files;
			loaderPlayer.LoadAnim = GetConfigFileNameNoExt(files.anim);
		}
	}

	public static string GetConfigFileNameNoExt(string fileName)
	{
		if (string.IsNullOrEmpty (fileName))
			return string.Empty;
		int idx = fileName.LastIndexOf (".");
		if (idx < 0)
			return fileName;
		string ret = fileName.Substring (0, idx);
		return ret;
	}

	private void AttachPals(DefaultLoaderPlayer loaderPlayer, GlobalPlayer player)
	{
		if (player == null || player.PlayerCfg == null || player.PlayerCfg.Files == null)
			loaderPlayer.PalNameList = null;
		else {
			var files = player.PlayerCfg.Files;
			string name = GetConfigFileNameNoExt(files.pal1);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal2);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal3);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal4);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal5);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal6);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal7);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal8);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal9);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal10);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal11);
			loaderPlayer.AddPalName (name);
			name = GetConfigFileNameNoExt(files.pal12);
			loaderPlayer.AddPalName (name);
		}
	}

	void Start()
	{
		if (m_DefaultLoaderPlayers != null) {
			for (int i = 0; i < m_DefaultLoaderPlayers.Count; ++i) {
				var defaultPlayer = m_DefaultLoaderPlayers [i];
				if (defaultPlayer != null) {
					GlobalPlayerLoaderResult result;
					var player = LoadPlayer (defaultPlayer, out result);
					#if UNITY_EDITOR
					defaultPlayer.LoadResult = result;
					AttachAnim (defaultPlayer, player);
					AddCnsList (defaultPlayer, player);
					AttachPals (defaultPlayer, player);
					#endif
					player.CreatePlayerDisplay (defaultPlayer);
				}
			}
		}
	}

	protected override void  Awake()
	{
		base.Awake ();

		m_Loader = gameObject.AddComponent<BaseResLoader> ();

		m_ClsnSpritePoolRoot = new GameObject ("Pool_SpriteClsn");
		m_ClsnSpritePoolRoot.transform.SetParent (this.transform, false);
		m_ClsnSpritePoolRoot.SetActive (false);

		m_ClsnColliderPoolRoot = new GameObject ("Pool_ColliderClsn");
		m_ClsnColliderPoolRoot.transform.SetParent (this.transform, false);
		m_ClsnColliderPoolRoot.SetActive (false);

        // 注冊所有狀態
        PlayerStateCtl.RegisterPlayerStates();
	}
}
