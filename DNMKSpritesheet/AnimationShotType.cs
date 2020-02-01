using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace DNMKSpritesheet
{
    public class AnimationShotType : ShotType
    {
        public override ImageSource ImageSource
        {
            get
            {
                if (Animation.Length == 0)
                {
                    return null;
                }

                return Animation.GetSpriteAtIndex(0).ImageSource;
            }
        }
        
        public Animation Animation { get; set; }

        public void RemoveSprite(Sprite sprite)
        {
            Animation.RemoveSprite(sprite);
        }

        public AnimationShotType(string name, int collision, Color delay_color)
        {
            Name = name;
            Collision = collision;
            DelayColor = delay_color;
            Animation = new Animation();
        }

        public override DataString GetDataString(SpriteSheet sheet, int id)
        {
            DataString data_string = base.GetDataString(sheet, id);
            
            data_string.AddData(Animation.GetDataString(sheet));

            return data_string;
        }
        
        public int MoveAnimationFrameUp(int index)
        {
            if (index < 0 || index >= Animation.Length)
            {
                throw new ArgumentException("Animation index is out of bounds.");
            }

            if (index == 0)
            {
                return index;
            }

            AnimationFrame current = Animation.AnimationFrames[index];
            Animation.AnimationFrames.RemoveAt(index);
            Animation.AnimationFrames.Insert(index - 1, current);
            
            OnPropertyChanged("ImageSource");

            return index - 1;
        }
        
        public int MoveAnimationFrameDown(int index)
        {
            if (index < 0 || index >= Animation.Length)
            {
                throw new ArgumentException("Animation index is out of bounds.");
            }
            
            if (index == Animation.Length - 1)
            {
                return index;
            }
            
            AnimationFrame current = Animation.AnimationFrames[index];
            Animation.AnimationFrames.RemoveAt(index);
            Animation.AnimationFrames.Insert(index + 1, current);

            OnPropertyChanged("ImageSource");
            
            return index + 1;
        }
    }
}