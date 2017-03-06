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

namespace Clipboard__ {
    abstract class ClipboardListItem {
        protected IDataObject obj;
        protected ImageSource img;
        protected DateTime copytime;
        protected string text;

        public ClipboardListItem() {
            this.copytime = DateTime.Now;
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
    }

    class ImageClipboardListItem : ClipboardListItem {
        public ImageClipboardListItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = (obj.GetData(DataFormats.Bitmap, true) as InteropBitmap);
            this.text = "Image";
        }
    }

    class AudioClipboardListItem : ClipboardListItem {
        public AudioClipboardListItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/audio.png"));
            this.text = "Audio";
        }
    }

    class FileClipboardListItem : ClipboardListItem {
        public FileClipboardListItem(IDataObject obj) : base() {
            this.obj = obj;
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

    class TextClipboardListItem : ClipboardListItem {
        public TextClipboardListItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/text.png"));
            this.text = obj.GetData(DataFormats.UnicodeText).ToString();
        }
    }

    class OtherClipboardListItem : ClipboardListItem {
        public OtherClipboardListItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/unknown.png"));

            var formats = obj.GetFormats(true);
            if (formats.Contains(DataFormats.UnicodeText)) {
                this.text = obj.GetData(DataFormats.UnicodeText).ToString();
            } else {
                this.text = "No preview text";
            }
        }
    }

    class ClipboardListItemFactory {
        public static ClipboardListItem CreateItem(IDataObject obj) {
            var formats = obj.GetFormats(true);

            if (formats.Contains(DataFormats.Bitmap)) {
                return new ImageClipboardListItem(obj);
            } else if (formats.Contains(DataFormats.WaveAudio)) {
                return new AudioClipboardListItem(obj);
            } else if (formats.Contains(DataFormats.FileDrop)) {
                return new FileClipboardListItem(obj);
            } else if (formats.Contains(DataFormats.UnicodeText)) {
                return new TextClipboardListItem(obj);
            } else {
                return new OtherClipboardListItem(obj);
            }
        }
    }
}
