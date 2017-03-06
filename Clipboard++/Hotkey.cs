using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clipboard__ {
    class Hotkey {
        private static int currentId = 0;

        private uint modifier;
        private uint key;
        private int id;
        private Action action;

        public Hotkey(ModifierKeys modifier, uint virtualKey, Action hotkeyAction) {
            this.modifier = (uint)modifier;
            this.key = virtualKey;
            this.id = currentId;
            this.action = hotkeyAction;

            currentId++;
        }

        public uint Key {
            get {
                return key;
            }

            set {
                key = value;
            }
        }

        public uint Modifier {
            get {
                return modifier;
            }

            set {
                modifier = value;
            }
        }

        public int Id {
            get {
                return id;
            }

            set {
                id = value;
            }
        }

        public Action Action {
            get {
                return action;
            }

            set {
                action = value;
            }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
