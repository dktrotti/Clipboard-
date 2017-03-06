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
    abstract class ClipboardItem {
        protected IDataObject obj;
        protected ImageSource img;
        protected DateTime copytime;
        protected string text;

        public const string CUSTOM_FORMAT = "Clipboard++DataFormat";

        public ClipboardItem() {
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

        public abstract IDataObject Data {
            get;
        }
    }

    class ImageClipboardItem : ClipboardItem {
        public ImageClipboardItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = (obj.GetData(DataFormats.Bitmap, true) as InteropBitmap);
            this.text = "Image";
        }

        public override IDataObject Data {
            get {
                var rv = new DataObject();

                rv.SetData(DataFormats.Bitmap, this.obj.GetData(DataFormats.Bitmap, true));
                // Format used as marker to indicate that this data comes from Clipboard++
                rv.SetData(CUSTOM_FORMAT, 1);

                return rv;
            }
        }
    }

    class AudioClipboardItem : ClipboardItem {
        public AudioClipboardItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/audio.png"));
            this.text = "Audio";
        }

        public override IDataObject Data {
            get {
                var rv = new DataObject();

                rv.SetData(DataFormats.WaveAudio, this.obj.GetData(DataFormats.WaveAudio, true));
                // Format used as marker to indicate that this data comes from Clipboard++
                rv.SetData(CUSTOM_FORMAT, 1);

                return rv;
            }
        }
    }

    class FileClipboardItem : ClipboardItem {
        public FileClipboardItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/folder.png"));
            var file = (obj.GetData(DataFormats.FileDrop) as string[]);
            this.text = getFileNames(file);
        }

        public override IDataObject Data {
            get {
                var rv = new DataObject();

                rv.SetData(DataFormats.FileDrop, this.obj.GetData(DataFormats.FileDrop, true));
                // Format used as marker to indicate that this data comes from Clipboard++
                rv.SetData(CUSTOM_FORMAT, 1);

                return rv;
            }
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
        public TextClipboardItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/text.png"));
            this.text = obj.GetData(DataFormats.UnicodeText, true).ToString();
        }

        public override IDataObject Data {
            get {
                var rv = new DataObject();
                
                rv.SetData(DataFormats.UnicodeText, this.obj.GetData(DataFormats.UnicodeText, true));
                // Format used as marker to indicate that this data comes from Clipboard++
                rv.SetData(CUSTOM_FORMAT, 1);

                return rv;
            }
        }
    }

    class OtherClipboardItem : ClipboardItem {
        public OtherClipboardItem(IDataObject obj) : base() {
            this.obj = obj;
            this.img = new BitmapImage(new Uri("pack://application:,,,/images/unknown.png"));

            var formats = obj.GetFormats(true);
            if (formats.Contains(DataFormats.UnicodeText)) {
                this.text = obj.GetData(DataFormats.UnicodeText).ToString();
            } else {
                this.text = "No preview text";
            }
        }

        public override IDataObject Data {
            get {
                return null;
            }
        }
    }

    class ClipboardItemFactory {
        public static ClipboardItem CreateItem(IDataObject obj) {
            var formats = obj.GetFormats(true);

            if (formats.Contains(DataFormats.Bitmap)) {
                return new ImageClipboardItem(obj);
            } else if (formats.Contains(DataFormats.WaveAudio)) {
                return new AudioClipboardItem(obj);
            } else if (formats.Contains(DataFormats.FileDrop)) {
                return new FileClipboardItem(obj);
            } else if (formats.Contains(DataFormats.UnicodeText)) {
                return new TextClipboardItem(obj);
            } else {
                Console.WriteLine("Unrecognized clipboard item.");
                return null;
                //return new OtherClipboardItem(obj);
            }
        }
    }
}
