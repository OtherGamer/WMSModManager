using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WMSModManager
{
    public class ModUpdateChecker
    {
        public static bool CompareVersion(string installed, string internet)
        {
            string[] spl = installed.Split('.');
            string[] inter = internet.Split(".");
            int mn = spl.Length;
            if (mn > inter.Length)
            {
                mn = inter.Length;
            }
            for (int i = 0; i < mn; i++)
            {
                if (spl[i] != inter[i])
                {
                    try
                    {
                        int f = Int32.Parse(spl[i]);
                        int s = Int32.Parse(inter[i]);
                        if (f < s)
                        {
                            return true;
                        }
                        return false;
                    }
                    catch
                    {
                        return (spl[i].CompareTo(inter[i])) < 0;
                    }
                }
            }
            return spl.Length < inter.Length;
        }
    }
}
