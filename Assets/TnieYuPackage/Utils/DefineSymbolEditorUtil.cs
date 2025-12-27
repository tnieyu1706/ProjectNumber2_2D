using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace TnieYuPackage.Utils
{
    public static class DefineSymbolEditorUtil
    {
        public static HashSet<string> GetDefineSymbols(NamedBuildTarget namedBuildTarget)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            return new(defines.Split(';'));
        }
        
        public static bool AddDefineSymbol(string symbol, NamedBuildTarget namedBuildTarget)
        {
            HashSet<string> defineSymbols = GetDefineSymbols(namedBuildTarget);
            if (!defineSymbols.Contains(symbol))
            {
                defineSymbols.Add(symbol);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, string.Join(';', defineSymbols));
                return true;
            }

            return false;
        }

        public static void RemoveDefineSymbol(string symbol, NamedBuildTarget namedBuildTarget)
        {
            HashSet<string> defineSymbols = GetDefineSymbols(namedBuildTarget);
            if (defineSymbols.Contains(symbol))
            {
                defineSymbols.Remove(symbol);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, string.Join(";", defineSymbols));
            }
        }

        public static bool IsSymbolDefined(string symbol, NamedBuildTarget namedBuildTarget)
        {
            HashSet<string> defineSymbols = GetDefineSymbols(namedBuildTarget);
            return defineSymbols.Contains(symbol);
        }
    }
}