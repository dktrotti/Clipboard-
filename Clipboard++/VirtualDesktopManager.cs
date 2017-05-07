//Copyright(C) 2017  Dominic Trottier

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Clipboard__ {
    // Found here: https://blogs.msdn.microsoft.com/winsdk/2015/09/10/virtual-desktop-switching-in-windows-10/

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    [System.Security.SuppressUnmanagedCodeSecurity]
    public interface IVirtualDesktopManager {
        [PreserveSig]
        int IsWindowOnCurrentVirtualDesktop(
            [In] IntPtr TopLevelWindow,
            [Out] out int OnCurrentDesktop
            );
        [PreserveSig]
        int GetWindowDesktopId(
            [In] IntPtr TopLevelWindow,
            [Out] out Guid CurrentDesktop
            );

        [PreserveSig]
        int MoveWindowToDesktop(
            [In] IntPtr TopLevelWindow,
            [MarshalAs(UnmanagedType.LPStruct)]
            [In]Guid CurrentDesktop
            );
    }

    public class NewWindow : Window, IDisposable {
        public NewWindow() {
            this.Height = 1;
            this.Width = 1;
            this.Opacity = 0;
        }

        public void Dispose() {
            this.Close();
            this.Dispose();
        }
    }

    [ComImport, Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
    public class CVirtualDesktopManager {

    }
    public class VirtualDesktopManager {
        public VirtualDesktopManager() {
            cmanager = new CVirtualDesktopManager();
            manager = (IVirtualDesktopManager)cmanager;
        }
        ~VirtualDesktopManager() {
            manager = null;
            cmanager = null;
        }
        private CVirtualDesktopManager cmanager = null;
        private IVirtualDesktopManager manager;

        public bool IsWindowOnCurrentVirtualDesktop(IntPtr TopLevelWindow) {
            int result;
            int hr;
            if ((hr = manager.IsWindowOnCurrentVirtualDesktop(TopLevelWindow, out result)) != 0) {
                Marshal.ThrowExceptionForHR(hr);
            }
            return result != 0;
        }

        public Guid GetWindowDesktopId(IntPtr TopLevelWindow) {
            Guid result;
            int hr;
            if ((hr = manager.GetWindowDesktopId(TopLevelWindow, out result)) != 0) {
                Marshal.ThrowExceptionForHR(hr);
            }
            return result;
        }

        public void MoveWindowToDesktop(IntPtr TopLevelWindow, Guid CurrentDesktop) {
            int hr;
            if ((hr = manager.MoveWindowToDesktop(TopLevelWindow, CurrentDesktop)) != 0) {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public Guid GetCurrentDesktopId() {
            // GetWindowDesktopId doesn't work inside of a "using" block for some reason, "try finally" is used instead.
            NewWindow nw = new NewWindow();
            try {
                nw.Show();
                var helper = new WindowInteropHelper(nw);
                return this.GetWindowDesktopId(helper.Handle);
            } finally {
                nw.Close();
            }
        }
    }
}
