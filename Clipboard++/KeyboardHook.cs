using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace Clipboard__
{
    // Based on:
    // https://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp/27309185#27309185
    // https://msdn.microsoft.com/en-us/library/ms752055(v=vs.110).aspx

    public sealed class KeyboardHook : HwndHost, IDisposable
    {
        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        internal const int
            WS_CHILD = 0x40000000,
            WS_VISIBLE = 0x10000000,
            LBS_NOTIFY = 0x00000001,
            HOST_ID = 0x00000002,
            LISTBOX_ID = 0x00000001,
            WS_VSCROLL = 0x00200000,
            WS_BORDER = 0x00800000;

        private const int
            WIN_HEIGHT = 1,
            WIN_WIDTH = 1;

        private static int WM_HOTKEY = 0x0312;
        
        private int _currentId = 0;
        private IntPtr hwndHost;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            hwndHost = IntPtr.Zero;

            hwndHost = NativeMethods.CreateWindowEx(0, "static", "",
                                      WS_CHILD,
                                      0, 0,
                                      WIN_HEIGHT, WIN_WIDTH,
                                      hwndParent.Handle,
                                      (IntPtr)HOST_ID,
                                      IntPtr.Zero,
                                      0);

            return new HandleRef(this, hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            NativeMethods.DestroyWindow(hwnd.Handle);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                Key key = (Key)(((int)lParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)lParam & 0xFFFF);

                KeyPressed(this, new KeyPressedEventArgs(modifier, key));
            }

            handled = false;
            return IntPtr.Zero;
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Key key)
        {
            // increment the counter.
            _currentId++;

            // register the hot key.
            if (!NativeMethods.RegisterHotKey(this.Handle, _currentId, (uint)modifier, (uint)key))
                throw new InvalidOperationException("Couldn’t register the hot key.");
        }

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = _currentId; i > 0; i--)
            {
                NativeMethods.UnregisterHotKey(this.Handle, i);
            }

            // TODO: Should DestroyWindow be called here?
        }

        #endregion

        private class NativeMethods
        {
            // Registers a hot key with Windows.
            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
            // Unregisters the hot key with Windows.
            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
            // Creates a window to be used for receiving messages
            [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                  string lpszClassName,
                                                  string lpszWindowName,
                                                  int style,
                                                  int x, int y,
                                                  int width, int height,
                                                  IntPtr hwndParent,
                                                  IntPtr hMenu,
                                                  IntPtr hInst,
                                                  [MarshalAs(UnmanagedType.AsAny)] object pvParam);
            // Destroys a window created by CreateWindowEx
            [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
            internal static extern bool DestroyWindow(IntPtr hwnd);
        }
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private Key _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, Key key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Key Key
        {
            get { return _key; }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
