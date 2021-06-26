﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
using System.IO;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
//more stuff
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using StolenYetHelpfulCode;

namespace BBPlusTwitch
{
    //copied from BB+ multiplayer AGAIN i love re-using code
    //note: this code is shit maybe i should completely rewrite the system for this from the ground up
    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Load")]
    class HijackDefaultLoad
    {
        static bool Prefix(NameManager __instance, ref NameButton[] ___buttons, ref string[] ___nameList, ref int ___nameCount) //Initialize initial connect menu
        {
            if (NameMenuManager.CurrentState == NameMenuState.ModeSelector)
            {
                UnityEngine.Debug.Log("loading mode selector");
                ___nameList[0] = "Vanilla";
                ___nameList[1] = "Speedy";
                ___nameList[2] = "Chaos";
                ___nameList[3] = "Offline(WIP)";
                ___nameList[4] = "Chaos(5s)";
                ___nameList[5] = "Chaos(15s)";
                return false;
            }
            else if (NameMenuManager.CurrentState == NameMenuState.Loading)
            {
                ___nameList[0] = "Waiting for";
                ___nameList[1] = "Connection...";
                ___nameList[2] = "";
                ___nameList[3] = "";
                ___nameList[4] = "";
                ___nameList[5] = "";
                return false;
            }
            else
            {
                UnityEngine.Debug.Log("not loading mode selector");
            }
            return NameMenuManager.CurrentState == NameMenuState.SaveSelect;
        }
    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    class HijackNameAwake
    {
        static bool Prefix(NameManager __instance)
        {
            //UnityEngine.Debug.Log(BaldiTwitch.Oath.Value);

            IngameCommands.AddCommands();
            //TwitchManager.AddCommand("test",TestFunction,3);
            return true;
        }

        public static bool TestFunction(string person, string param)
        {
            UnityEngine.Debug.Log(person);
            UnityEngine.Debug.Log(param);
            return true;
        }

        public static void CreateManagers()
        {
            GameObject newgam = new GameObject();
            newgam.name = "TwitchHandlerObject";
            newgam.AddComponent<TwitchConnectionHandler>();
            GameObject newlod = new GameObject();
            newlod.name = "MonoLogicObject";
            newlod.AddComponent<MonoLogicManager>();
        }

        public static void ActivateNormalLoadingCode()
        {
            NameManager instance = GameObject.FindObjectOfType<NameManager>();
            instance.InvokeMethod<NameManager>("Load");
            instance.UpdateState();
        }
    }


    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("NameClicked")]
    class ModifyNameClick
    {
        static bool Prefix(NameManager __instance, int fileNo, ref int ___nameCount, ref string ___newName, ref string[] ___nameList, ref NameButton[] ___buttons, ref bool ___enteringNewName)
        {
            if (NameMenuManager.CurrentState == NameMenuState.ModeSelector)
            {
                if (!(fileNo > 3))
                {
                    //if (fileNo == 0 || fileNo == 1) return false;
                    TwitchManager.CommandCooldown = 5f;
                    NameMenuManager.CurrentState = NameMenuState.Loading;
                    SettingsManager.Mode = (TwitchMode)fileNo;
                    HijackNameAwake.CreateManagers();
                    //THANK YOU STACK OVERFLOW YOU HAVE SAVED MY LIFE
                    __instance.InvokeMethod<NameManager>("Load");
                    return false;
                }
                else
                {
                    NameMenuManager.CurrentState = NameMenuState.Loading;
                    SettingsManager.Mode = TwitchMode.Chaos;
                    HijackNameAwake.CreateManagers();
                    TwitchManager.CooldownEnabled = true;
                    if (fileNo == 4)
                    {
                        TwitchManager.CommandCooldown = 5f;
                    }
                    else if (fileNo == 5)
                    {
                        TwitchManager.CommandCooldown = 15f;
                    }
                    //THANK YOU STACK OVERFLOW YOU HAVE SAVED MY LIFE
                    __instance.InvokeMethod<NameManager>("Load");
                    __instance.UpdateState();
                    return false;
                }
            }

            return (NameMenuManager.CurrentState == NameMenuState.SaveSelect && NameMenuManager.CurrentState != NameMenuState.Loading);
        }

    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Update")]
    class NoNameUpdate
    {
        static bool Prefix(NameManager __instance, ref NameButton[] ___buttons, ref string[] ___nameList, ref bool ___enteringNewName, ref string ___newName, ref int ___nameCount)
        {
            
            return NameMenuManager.CurrentState == NameMenuState.SaveSelect;
        }
    }
}
