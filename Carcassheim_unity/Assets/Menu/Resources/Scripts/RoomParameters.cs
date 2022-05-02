using Assets.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Room Parameters menu 
/// </summary>
public class RoomParameters : Miscellaneous
{
	/// <summary>
	///     variables used to get some GameObjects 
	/// </summary>
	private Transform S_container, T_container;
	/// <summary>
	///     variables used to get the time slider
	/// </summary>
	private Slider S_time_round;
	/// <summary>
	///     variables used to get all parameters toggles 
	/// </summary>
	private Toggle T_temps, T_points, T_tuiles, T_public, T_riviere, T_abbaye, T_cathedrale;
	/// <summary>
	///     variables that represents the duration of a round (value of the slider)
	/// </summary>
	private static float duree_round;
	/// <summary>
	///     variables that represents the values of the toggles
	/// </summary>
	private static bool fin_temps = true, fin_points = false, fin_tuiles = false, is_public = true, ext_riviere = true, ext_abbaye = true, ext_cathedrale = true;
	//ATTENTION : cocher une extension la desactive du jeu

	/// <summary>
	///     some initialization 
	/// </summary>
	void Start()
	{
		S_container = GameObject.Find("SubMenus").transform.Find("RoomParametersMenu").transform.Find("Sliders").transform;
		T_container = GameObject.Find("SubMenus").transform.Find("RoomParametersMenu").transform.Find("Toggle Group").transform.Find("ToggleValueChangedRPM");

		S_time_round = S_container.Find("Slider time per round").GetComponent<Slider>();
		S_time_round.onValueChanged.AddListener(TempsParRound);
		duree_round = S_time_round.value;

		T_temps = T_container.Find("EndWithTime").GetComponent<Toggle>();
		T_points = T_container.Find("EndWithPoints").GetComponent<Toggle>();
		T_tuiles = T_container.Find("EndWithTile").GetComponent<Toggle>();

		T_public = T_container.Find("RoomIsPublic").GetComponent<Toggle>();

		T_riviere = T_container.Find("RiverExtension").GetComponent<Toggle>();
		T_abbaye = T_container.Find("AbbayeExtension").GetComponent<Toggle>();
		T_cathedrale = T_container.Find("CathedralExtension").GetComponent<Toggle>();
	}


	// Update is called once per frame
	void Update()
	{
		//Debug.Log("PUBLIC ? : " + is_public + " FIN TEMPS ? : " + fin_temps + " FIN POINTS ? : " + fin_points + " FIN TUILES ? : " + fin_tuiles + " RIVIERE ? : " + ext_riviere + " ABBAYE ? : " + ext_abbaye + " CATHEDRALE ? : " + ext_cathedrale + " DUREE ? : " + duree_round);
	}

	/// <summary>
	///    predefine the values of the slider 
	/// </summary>
	public void TempsParRound(float value)
	{
		if(value >= 20 && value <= 30)
			S_time_round.value = 20;
		else if(value > 30 && value <= 50)
			S_time_round.value = 40;
		else if(value > 50 && value <= 60)
			S_time_round.value = 60;

		duree_round = S_time_round.value;
	}

	/// <summary>
	///     Go to Public room menu 
	/// </summary>
	public void HideRoomParameters()
	{
		HidePopUpOptions();
		ChangeMenu("RoomParametersMenu", "PublicRoomMenu");
	}

	/// <summary>
	///     <param name="curT">The used toggle .</param>
	///     put the right value to bool according to the used toggle 
	/// </summary>
	public void ToggleValueChangedRPM(Toggle curT)
	{
		if (curT.isOn)
			Debug.Log(curT.name);

		if(T_temps.isOn)
		{
			fin_temps = true;
			fin_points = false;
			fin_tuiles = false;
		}
		else if(T_points.isOn)
		{
			fin_points = true;
			fin_temps = false;
			fin_tuiles = false;
		}
		else if(T_tuiles.isOn)
		{
			fin_tuiles = true;
			fin_points = false;
			fin_temps = false;
		}

		if(T_public.isOn)
			is_public = true;
		else is_public = false;

		if(T_riviere.isOn)
			ext_riviere = false;
		else ext_riviere = true;

		if(T_abbaye.isOn)
			ext_abbaye = false;
		else ext_abbaye = true;

		if(T_cathedrale.isOn)
			ext_cathedrale = false;
		else ext_cathedrale = true;
	}
}