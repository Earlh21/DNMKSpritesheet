using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media.Animation;

namespace DNMKSpritesheet
{
    public class Animation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<AnimationFrame> AnimationFrames { get; private set; }

        public int Length => AnimationFrames.Count;

        public Sprite GetSpriteAtIndex(int index)
        {
            return AnimationFrames[index].Sprite;
        }

        public int GetDelayAtIndex(int index)
        {
            return AnimationFrames[index].Delay;
        }
        
        public void RemoveSprite(Sprite sprite)
        {
            for (int i = 0; i < AnimationFrames.Count; i++)
            {
                AnimationFrame anim_frame = AnimationFrames[i];

                if (anim_frame.Sprite == sprite)
                {
                    AnimationFrames.RemoveAt(i);

                    if (i == 0)
                    {
                        OnPropertyChanged("ImageSource");
                    }
                    
                    i--;
                }
            }
        }
        
        public void AddAnimationFrame(int delay, Sprite sprite)
        { 
            AnimationFrames.Add(new AnimationFrame(delay, sprite));

            if (Length == 1)
            {
                OnPropertyChanged("ImageSource");
            }
        }

        public void InsertAnimationFrame(int index, int delay, Sprite sprite)
        {
            AnimationFrames.Insert(index, new AnimationFrame(delay, sprite));

            if (Length == 1)
            {
                OnPropertyChanged("ImageSource");
            }
        }

        public void RemoveAnimationFrame(int index)
        {
            AnimationFrames.RemoveAt(index);

            if (Length == 0)
            {
                OnPropertyChanged("ImageSource");
            }
        }

        public Animation()
        {
            AnimationFrames = new ObservableCollection<AnimationFrame>();
        }
        
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public DataString GetDataString(SpriteSheet sheet)
        {
            DataString data_string = new DataString("AnimationData");
            foreach (AnimationFrame frame in AnimationFrames)
            {
                data_string.AddData(frame.GetDataString(sheet));
            }

            return data_string;
        }
    }
}