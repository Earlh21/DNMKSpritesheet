using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace DNMKSpritesheet
{
    public abstract class ShotType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string name;
        private int collision;

        public abstract ImageSource ImageSource { get; }
        public Color DelayColor { get; set; }

        public int Collision
        {
            get => collision;
            set
            {
                if (collision < 0)
                {
                    throw new ArgumentException("Collision size cannot be negative.");
                }

                collision = value;
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (value.Any(Char.IsWhiteSpace))
                {
                    throw new ArgumentException("Name cannot contain whitespace.");
                }
                
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name cannot be empty.");
                }
                
                name = value;
                OnPropertyChanged("Name");
            }
        }
        
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public virtual DataString GetDataString(SpriteSheet sheet, int id)
        {
            DataString data_string = new DataString("ShotData");
            
            string color_name = "delay_color=(";
            color_name += DelayColor.R + ", " + DelayColor.G + ", " + DelayColor.B + ")";
            DataString color_data_string = new DataString(color_name);

            string id_name = "id=" + id;
            DataString id_data_string = new DataString(id_name);

            string collision_name = "collision=" + Collision;
            DataString collision_data_string = new DataString(collision_name);
            
            data_string.AddData(color_data_string);
            data_string.AddData(id_data_string);
            data_string.AddData(collision_data_string);

            return data_string;
        }
    }
}