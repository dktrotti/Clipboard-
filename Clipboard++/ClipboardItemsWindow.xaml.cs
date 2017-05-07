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

using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsDesktop;

// Hotkeys based on solution here: http://stackoverflow.com/a/11378213
// Clipboard based on solution here: http://stackoverflow.com/a/11901709

namespace Clipboard__ {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClipboardItemsWindow : Window {
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

        public ClipboardItemsWindow() {
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

            this.cbItemsListBox.ItemsSource = clipboardItems;
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

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            const int WM_HOTKEY = 0x0312;
            const int WM_CLIPBOARDUPDATE = 0x031D;
            switch (msg) {
                case WM_HOTKEY:
                    int id = wParam.ToInt32();
                    if (hotkeys.ContainsKey(id)) {
                        hotkeys[id].Action();
                    } else {
                        // TODO: Do something
                    }
                    break;
                case WM_CLIPBOARDUPDATE:
                    addCurrentClipboard();
                    break;
            }
            return IntPtr.Zero;
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

        private void focusHotkeyPressed() {
            if (this.IsKeyboardFocused) {
                this.cbItemsListBox.SelectedIndex = (this.cbItemsListBox.SelectedIndex + 1) % this.cbItemsListBox.Items.Count;
            } else {
                makeVisible();
            }
        }

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void cbItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!updatingSelection) {
                var cbItem = cbItemsListBox.SelectedItem as ClipboardItem;
                Clipboard.SetDataObject(cbItem.Data);
                cbItemsListBox.ScrollIntoView(cbItem);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e) {
            makeVisible();
        }

        private void Quit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            var res = MessageBox.Show(this, "Are you sure you want to quit Clipboard++? All stored items will be lost.", "Clipboard++", MessageBoxButton.OKCancel);

            if (res != MessageBoxResult.OK) {
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Found here: http://stackoverflow.com/a/9495851
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                this.cbItemsListBox.SelectedIndex = (this.cbItemsListBox.SelectedIndex + 1) % this.cbItemsListBox.Items.Count;
            } else if (e.Key == Key.Up) {
                if (this.cbItemsListBox.SelectedIndex == 0) {
                    this.cbItemsListBox.SelectedIndex = this.cbItemsListBox.Items.Count - 1;
                } else {
                    this.cbItemsListBox.SelectedIndex = this.cbItemsListBox.SelectedIndex - 1;
                }
            }
        }

        private void makeVisible() {
            if (this.WindowState == WindowState.Minimized) {
                this.WindowState = WindowState.Normal;
            }

            var vdm = new VirtualDesktopManager();
            var helper = new WindowInteropHelper(this);

            //vdm.MoveWindowToDesktop(helper.Handle, vdm.GetCurrentDesktopId());
            var desktops = VirtualDesktop.GetDesktops();
            //this.MoveToDesktop(VirtualDesktop.Current);

            this.Activate();
        }

        private void Clear_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void Copy_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void Remove_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}

