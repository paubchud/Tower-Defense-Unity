#if UNITY_EDITOR
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager;
using UnityEngine;

namespace TowerDefense.Networking
{
    // Official C# SDK uses dynamic bindings in the editor, static P/Invoke in players.
    // No overlay/sample EOSManager is initialized alongside our owned platform handle.
    internal sealed class EosNativeEditor : IDisposable
    {
        private IntPtr library;
        private readonly bool mac = Application.platform == RuntimePlatform.OSXEditor;
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr LoadLibraryW(string path);
        [DllImport("kernel32", CharSet = CharSet.Ansi)] private static extern IntPtr GetProcAddress(IntPtr handle, string name);
        [DllImport("kernel32")] private static extern bool FreeLibrary(IntPtr handle);
        [DllImport("libdl")] private static extern IntPtr dlopen(string path, int flags);
        [DllImport("libdl")] private static extern IntPtr dlsym(IntPtr handle, string name);
        [DllImport("libdl")] private static extern int dlclose(IntPtr handle);
        public EosNativeEditor()
        {
            var info = PackageInfo.FindForAssetPath("Packages/com.towerdefense.eos.sdk/package.json");
            if (info == null) throw new EosOnlineException("Install the pinned EOS SDK with Tools/InstallEosSdk.ps1.");
            string path = Path.Combine(info.resolvedPath, "Runtime/Plugins", mac ? "libEOSSDK-Mac-Shipping.dylib" : "EOSSDK-Win64-Shipping.dll");
            library = mac ? dlopen(path, 2) : LoadLibraryW(path);
            if (library == IntPtr.Zero) throw new EosOnlineException("The EOS native editor library could not be loaded.");
        }
        public IntPtr Symbol(string name)
        {
            if (!mac) return GetProcAddress(library, name);
            IntPtr symbol = dlsym(library, name);
            return symbol == IntPtr.Zero && name.StartsWith("_EOS_", StringComparison.Ordinal) ? dlsym(library, name.Substring(1)) : symbol;
        }
        public void Dispose()
        {
            if (library == IntPtr.Zero) return;
            if (mac) dlclose(library); else FreeLibrary(library);
            library = IntPtr.Zero;
        }
    }
}
#endif
