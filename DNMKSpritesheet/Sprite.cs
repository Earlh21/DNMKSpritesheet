using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DNMKSpritesheet
{
    public class Sprite : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private ImageSource image_source;
        private Bitmap image;
        private string name;

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name cannot be empty.");
                }
            
                if (value.Any(Char.IsWhiteSpace))
                {
                    throw new ArgumentException("Name cannot contain whitespace.");
                }
                
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public ImageSource ImageSource => image_source;

        public Bitmap Image
        {
            get => image;
            set
            {
                image = value;
                image_source = ImageSourceFromBitmap(image);
                OnPropertyChanged("ImageSource");
            }
        }

        public Sprite(Bitmap image, string name)
        {
            Name = name;
            Image = image;
        }

        private static ImageSource ImageSourceFromBitmap(Bitmap image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public void ReloadImageSource()
        {
            image_source = ImageSourceFromBitmap(image);
        }
        
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public override bool Equals(object obj)
        {
            if (obj is Sprite other)
            {
                return other.Name.Equals(Name);
            }

            return false;
        }
    }
}