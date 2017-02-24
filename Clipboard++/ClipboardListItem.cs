using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Clipboard__ {
    class ClipboardListItem {
        private IDataObject obj;
        private ImageSource img;
        private DateTime copytime;

        public ClipboardListItem(IDataObject obj, ImageSource img) {
            this.obj = obj;
            this.img = img;
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

        public String Text {
            get {
                var formats = obj.GetFormats();
                if (formats.Contains(DataFormats.Text)) {
                    return obj.GetData(DataFormats.Text).ToString();
                } else {
                    return "No preview text";
                }
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

    class ClipboardListItemFactory {
        public static ClipboardListItem CreateItem(IDataObject obj) {

        }
        //new BitmapImage(new Uri("pack://application:,,,/images/text.png"))
    }
}
