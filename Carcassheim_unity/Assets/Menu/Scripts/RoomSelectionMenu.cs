﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RoomSelectionMenu : Miscellaneous
{
	public GameObject Pop_up_Options;
	public static bool s_isOpenPanel = false;

	// Start is called before the first frame update
	void Start()
	{
		Pop_up_Options = FindMenu("Panel Options");
	}

	public void HideRoomSelection()
	{
		s_isOpenPanel = false;
		Pop_up_Options.SetActive(s_isOpenPanel);
		ChangeMenu(FindMenu("RoomSelectionMenu"), FindMenu("HomeMenu"));
	}

	public void ShowPopUpOptions()
	{
		s_isOpenPanel = !s_isOpenPanel;
		Pop_up_Options.SetActive(s_isOpenPanel);
	}
}
