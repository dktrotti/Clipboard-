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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.IO;
using System.Reflection;

namespace Clipboard__ {
    abstract class ClipboardItem {
        protected Dictionary<string, object> data;
        protected ImageSource img;
        protected DateTime copytime;
        protected string text;

        // Limiting the allowed formats to the defaults helps prevent performance issues due to delayed rendering
        // See here: https://msdn.microsoft.com/en-us/library/windows/desktop/ms649014(v=vs.85).aspx#_win32_Delayed_Rendering
        // Also: http://stackoverflow.com/a/2579846
        protected static readonly string[] ALLOWED_FORMATS = typeof(DataFormats).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                                .Where(f => f.FieldType == typeof(string))
                                                                .Select(f => f.GetValue(null) as string)
                                                                .ToArray();

        public const string APP_CUSTOM_FORMAT = "Clipboard++DataFormat";

        public ClipboardItem(IDataObject obj) {
            data = new Dictionary<string, object>();
            this.copytime = DateTime.Now;

            var formats = obj.GetFormats(false);
            foreach (var format in formats.Intersect(ALLOWED_FORMATS)) {
                data[format] = obj.GetData(format);
            }
        }

        public DateTime Copytime {
            get {
                return copytime;
            }

            set {
                copytime = value;
            }
        }

        public string Text {
            get {
                return text;
            }
        }

        public ImageSource Image {
            get {
                return img;
            }

            set {
                img = value;
            }
        }

        public IDataObject Data {
            get {
                var rv = new DataObject();

                foreach (var entry in data) {
                    rv.SetData(entry.Key, entry.Value);
                }
                rv.SetData(APP_CUSTOM_FORMAT, 1);

                return rv;
            }
        }
    }

    class ImageClipboardItem : ClipboardItem {
        public ImageClipboardItem(IDataObject obj) : base(obj) {
            this.img = (obj.GetData(DataFormats.Bitmap, true) as InteropBitmap);
            this.text = "Image";
        }
    }

    class AudioClipboardItem : ClipboardItem {
        public AudioClipboardItem(IDataObject obj) : base(obj) {
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/audio.png"));
            this.text = "Audio";
        }
    }

    class FileClipboardItem : ClipboardItem {
        public FileClipboardItem(IDataObject obj) : base(obj) {
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/folder.png"));
            var file = (obj.GetData(DataFormats.FileDrop) as string[]);
            this.text = getFileNames(file);
        }

        private static string getFileNames(string[] files) {
            if (files.Length == 0) {
                throw new ArgumentException("No file names provided.");
            }

            var names = files.Select((file) => Path.GetFileName(file));
            return names.Aggregate((s1, s2) => s1 + ", " + s2);
        }
    }

    class TextClipboardItem : ClipboardItem {
        public TextClipboardItem(IDataObject obj) : base(obj) {
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/text.png"));
            this.text = obj.GetData(DataFormats.UnicodeText, true).ToString();
        }
    }

    class OtherClipboardItem : ClipboardItem {
        public OtherClipboardItem(IDataObject obj) : base(obj) {
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/unknown.png"));

            var formats = obj.GetFormats(true);
            if (formats.Contains(DataFormats.UnicodeText)) {
                this.text = obj.GetData(DataFormats.UnicodeText).ToString();
            } else {
                this.text = "No preview text";
            }
        }
    }

    class ClipboardItemFactory {
        public static ClipboardItem CreateItem(IDataObject obj) {
            var formats = obj.GetFormats(true);

            if (formats.Length == 0) {
                throw new EmptyClipboardException();
            }

            if (formats.Contains(DataFormats.Bitmap)) {
                return new ImageClipboardItem(obj);
            } else if (formats.Contains(DataFormats.WaveAudio)) {
                return new AudioClipboardItem(obj);
            } else if (formats.Contains(DataFormats.FileDrop)) {
                return new FileClipboardItem(obj);
            } else if (formats.Contains(DataFormats.UnicodeText)) {
                return new TextClipboardItem(obj);
            } else {
                return new OtherClipboardItem(obj);
            }
        }
    }

    class EmptyClipboardException : Exception {

    }
}
