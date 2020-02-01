using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DNMKSpritesheet
{
    public class AnimationFrame : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int delay;
        private Sprite sprite;

        public int Delay
        {
            get => delay;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("Delay cannot be less than one.");
                }

                delay = value;
                OnPropertyChanged("Delay");
            }
        }

        public Sprite Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                OnPropertyChanged("Sprite");
            }
        }
        public ImageSource ImageSource => Sprite.ImageSource;
        
        public AnimationFrame(int delay, Sprite sprite)
        {
            Delay = delay;
            Sprite = sprite;
        }

        public DataString GetDataString(SpriteSheet sheet)
        {
            Int32Rect domain = sheet.GetSpriteDomain(Sprite);
            String name = "animation_data=(";
            name += Delay + ",";
            name += domain.X + "," + domain.Y + ",";
            name += (domain.X + domain.Width).ToString() + "," + (domain.Y + domain.Height).ToString() + ")";
            return new DataString(name);
        }
        
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}