using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace DNMKSpritesheet
{
    public class StillShotType : ShotType
    {
        private Sprite sprite;
        
        public override ImageSource ImageSource => Sprite.ImageSource;

        public Sprite Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                OnPropertyChanged("ImageSource");
            }
        }

        public StillShotType(string name, Sprite sprite, int collision, Color delay_color)
        {
            
            Name = name;
            Sprite = sprite;
            Collision = collision;
            DelayColor = delay_color;
        }

        public override DataString GetDataString(SpriteSheet sheet, int id)
        {
            DataString data_string = base.GetDataString(sheet, id);

            string rect_name = "rect=(";
            Int32Rect domain = sheet.GetSpriteDomain(Sprite);
            rect_name += domain.X + "," + domain.Y + ",";
            rect_name += (domain.X + domain.Width).ToString() + "," + (domain.Y + domain.Height).ToString() + ")";
            
            DataString rect_string = new DataString(rect_name);
            
            data_string.AddData(rect_string);

            return data_string;
        }
    }
}