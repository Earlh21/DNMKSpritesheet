using System.Runtime.Serialization;
using System.Windows;

namespace DNMKSpritesheet
{
    public struct PositionedSprite
    {
        public Sprite Sprite { get; private set; }
        public Int32Rect Domain { get; private set; }

        public PositionedSprite(Sprite sprite, Int32Rect domain)
        {
            Sprite = sprite;
            Domain = domain;
        }
    }
}