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
