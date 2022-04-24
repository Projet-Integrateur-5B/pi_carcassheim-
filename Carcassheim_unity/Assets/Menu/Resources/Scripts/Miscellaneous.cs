using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/* Convention de nommage : 
 * 
 * Méthodes : Méthode(int a...)
 * Attributs : public int Variable, private int _variable, static s_variable
 * Variables locales : int variable;
 * Classes : Classe1
 * 
 */
public abstract class Miscellaneous : MonoBehaviour
{
    [SerializeField] public GameObject absolute_parent;
    static protected GameObject absolute_parent_ref;

    public static event Action<string> OnMenuChange;

    private static bool s_state = false;
    private static bool s_menuHasChanged = false;
    private static GameObject previousMenu = null;
    private static GameObject nextMenu = null;
    private Color colState;
    public GameObject Pop_up_Options;
    public static bool s_isOpenPanel = false;
    void Awake()
    {
        if (absolute_parent == null)
            absolute_parent = absolute_parent_ref;
        Pop_up_Options = Miscellaneous.FindObject(absolute_parent, "SubMenus").transform.Find("Panel Options").gameObject;
        nextMenu = Miscellaneous.FindObject(absolute_parent, "HomeMenu"); // Menu courant au lancement du jeu
    }

    public static GameObject FindObject(GameObject parent, string name)
    {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    // PATCH : 
    /*public void GetScripts()
	{
		var scripts = Resources.LoadAll<MonoScript>("Scripts");
		int len = scripts.Length;
		foreach (var script in scripts)
		{
			// GetClass method returns the type of the script
			Debug.Log("Script : " + script.GetClass());
		}
	}*/

    public void HidePopUpOptions()
    {
        SetPanelOpen(false);
        Pop_up_Options.SetActive(GetPanelOpen());
    }

    public void ShowPopUpOptions()
    {
        if (GetPanelOpen())
        {
            Miscellaneous.FindObject(absolute_parent, "WheelPlayer").GetComponent<UnityEngine.Video.VideoPlayer>().Stop();
            Miscellaneous.FindObject(absolute_parent, "WheelPlayer").GetComponent<UnityEngine.Video.VideoPlayer>().isLooping = false;
        }
        else
        {
            Miscellaneous.FindObject(absolute_parent, "WheelPlayer").GetComponent<UnityEngine.Video.VideoPlayer>().Play();
            Miscellaneous.FindObject(absolute_parent, "WheelPlayer").GetComponent<UnityEngine.Video.VideoPlayer>().isLooping = true;
        }

        SetPanelOpen(!GetPanelOpen());
        Pop_up_Options.SetActive(GetPanelOpen());
    }

    public void SetPanelOpen(bool b)
    {
        s_isOpenPanel = b;
    }

    public bool GetPanelOpen()
    {
        return s_isOpenPanel;
    }

    public bool GetState()
    {
        return s_state;
    }

    public void SetState(bool b)
    {
        s_state = b;
    }

    public void Connected()
    {
        ColorUtility.TryParseHtmlString("#90EE90", out colState);
        Button tmpStat = Miscellaneous.FindObject(absolute_parent, "ShowStat").GetComponent<Button>();
        Button tmpJouer = Miscellaneous.FindObject(absolute_parent, "ShowRoomSelection").GetComponent<Button>();
        Miscellaneous.FindObject(absolute_parent, "Etat de connexion").GetComponent<Text>().color = colState;
        Miscellaneous.FindObject(absolute_parent, "Etat de connexion").GetComponent<Text>().text = "Connecte";
        Miscellaneous.FindObject(absolute_parent, "ShowConnection").SetActive(false);
        tmpJouer.interactable = tmpStat.interactable = true;
        tmpJouer.GetComponentInChildren<Text>().color = tmpStat.GetComponentInChildren<Text>().color = Color.white;

        // Remonte les boutons après la connexion 
        Transform buttons = Miscellaneous.FindObject(absolute_parent, "Buttons").transform;
        // buttons.GetChild(1).transform.position = buttons.GetChild(1).transform.position + new Vector3(0, 150, 0);
        // buttons.GetChild(2).transform.position = buttons.GetChild(2).transform.position + new Vector3(0, 150, 0);
        // buttons.GetChild(3).transform.position = buttons.GetChild(3).transform.position + new Vector3(0, 150, 0);
    }

    public void SetMenuChanged(bool b)
    {
        s_menuHasChanged = b;
    }

    public bool HasMenuChanged()
    {
        return s_menuHasChanged;
    }

    public GameObject GetPreviousMenu()
    {
        return previousMenu;
    }

    public GameObject GetCurrentMenu()
    {
        return nextMenu;
    }

    public GameObject FirstActiveChild(GameObject FAGO)
    {
        foreach (Transform child in FAGO.transform)
            if (child.gameObject.activeSelf)
                return child.gameObject;
        return null;
    }

    public void ChangeMenu(string close, string goTo)
    {
        Debug.Log("Close " + close + " to " + goTo);
        s_menuHasChanged = true;


        previousMenu = Miscellaneous.FindObject(absolute_parent, close).gameObject;
        nextMenu = Miscellaneous.FindObject(absolute_parent, goTo).gameObject;

        OnMenuChange?.Invoke(goTo);

        previousMenu.SetActive(false);
        nextMenu.SetActive(true);
    }

    //Not for passwords -> "" = char
    public string RemoveLastSpace(string mot) // Inputfield
    {
        string modif = mot.TrimEnd();
        return (mot.Length > 1) ? modif : mot;
    }
}