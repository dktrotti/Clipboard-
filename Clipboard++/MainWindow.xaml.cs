using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Clipboard__ {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private HwndSource _source;
        private Dictionary<int, Hotkey> hotkeys;
        private ObservableCollection<ClipboardItem> clipboardItems;
        private bool updatingSelection = false;
        private const int MAX_CB_ITEMS = 30;

        private class NativeMethods {
            // Registers a hot key with Windows.
            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
            // Unregisters the hot key with Windows.
            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
            // See http://msdn.microsoft.com/en-us/library/ms632599%28VS.85%29.aspx#message_only
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);
        }

        public MainWindow() {
            InitializeComponent();

            hotkeys = new Dictionary<int, Hotkey>();
            clipboardItems = new ObservableCollection<ClipboardItem>();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            // TODO: Make this a single instance application.
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);

            const uint VK_V = 0x56;
            RegisterHotKey(new Hotkey(ModifierKeys.Alt | ModifierKeys.Control, VK_V, focusHotkeyPressed));

            AddClipboardListener();

            cbItemsListBox.ItemsSource = clipboardItems;
            addCurrentClipboard();
        }

        protected override void OnClosed(EventArgs e) {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKeys();
            base.OnClosed(e);
        }

        private void RegisterHotKey(Hotkey hotkey) {
            var helper = new WindowInteropHelper(this);
            if (!NativeMethods.RegisterHotKey(helper.Handle, hotkey.Id, hotkey.Modifier, hotkey.Key)) {
                throw new InvalidOperationException("Couldn’t register the hot key.");
            }

            hotkeys.Add(hotkey.Id, hotkey);
        }

        private void UnregisterHotKeys() {
            var helper = new WindowInteropHelper(this);
            foreach (Hotkey hotkey in hotkeys.Values) {
                NativeMethods.UnregisterHotKey(helper.Handle, hotkey.Id);
            }
        }

        private void AddClipboardListener() {
            var helper = new WindowInteropHelper(this);
            NativeMethods.AddClipboardFormatListener(helper.Handle);
        }

        private void addCurrentClipboard() {
            var obj = Clipboard.GetDataObject();

            if (obj.GetDataPresent(ClipboardItem.APP_CUSTOM_FORMAT)) {
                // This data has been pasted by the application
                return;
            }

            try {
                updatingSelection = true;

                if (clipboardItems.Count >= MAX_CB_ITEMS) {
                    clipboardItems.RemoveAt(clipboardItems.Count - 1);
                }

                var cbItem = ClipboardItemFactory.CreateItem(obj);
                clipboardItems.Insert(0, cbItem);

                cbItemsListBox.SelectedIndex = 0;
                cbItemsListBox.ScrollIntoView(cbItem);
                updatingSelection = false;
            } catch (EmptyClipboardException) {
                // Nothing needs to be done if the clipboard is empty
                return;
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            const int WM_HOTKEY = 0x0312;
            const int WM_CLIPBOARDUPDATE = 0x031D;
            switch (msg) {
                case WM_HOTKEY:
                    int id = wParam.ToInt32();
                    if (hotkeys.ContainsKey(id)) {
                        hotkeys[id].Action();
                    } else {
                        // TODO: Do something, although this case may never occur
                    }
                    break;
                case WM_CLIPBOARDUPDATE:
                    addCurrentClipboard();
                    break;
            }
            return IntPtr.Zero;
        }
    }

    private void focusHotkeyPressed() {
        if (IsKeyboardFocused) {
            cbItemsListBox.SelectedIndex = (cbItemsListBox.SelectedIndex + 1) % cbItemsListBox.Items.Count;
        } else {
            makeVisible();
        }
    }
}
