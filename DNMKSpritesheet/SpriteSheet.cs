using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using Size = System.Drawing.Size;

namespace DNMKSpritesheet
{
    public class SpriteSheet
    {
        public Size Size { get; private set; }
        private List<PositionedSprite> PosSprites { get; set; }

        /// <summary>
        /// Constructs a SpriteSheet.
        /// </summary>
        /// <param name="sprites">Sprites to pack</param>
        public SpriteSheet(List<Sprite> sprites)
        {
            Size = new Size(0, 0);
            PackSprites(sprites);
        }

        private void PackSprites(List<Sprite> sprites)
        {
            PosSprites = new List<PositionedSprite>();
            Size = new Size();

            //This is a bad algorithm, but it should work for now since most sprites will be the same height.
            //It's just packing them all horizontally.
            
            foreach (Sprite sprite in sprites)
            {
                PackSpriteHelper(sprite, new Int32Point(Size.Width, 0));
            }
        }

        private void PackSpriteHelper(Sprite sprite, Int32Point location)
        {
            Size = new Size(location.X + sprite.Image.Width, Size.Height);

            if (location.Y + sprite.Image.Height > Size.Height)
            {
                Size = new Size(Size.Width, location.Y + sprite.Image.Height);
            }

            Int32Rect domain = new Int32Rect(location.X, location.Y, sprite.Image.Width, sprite.Image.Height);
            PosSprites.Add(new PositionedSprite(sprite, domain));
        }
        
        public Bitmap Export()
        {
            Bitmap image = new Bitmap(Size.Width, Size.Height);

            using (Graphics G = Graphics.FromImage(image))
            {
                foreach (PositionedSprite pos in PosSprites)
                {
                    G.DrawImage(pos.Sprite.Image, pos.Domain.X, pos.Domain.Y, pos.Sprite.Image.Width, pos.Sprite.Image.Height);
                }
            }

            return image;
        }

        public Int32Rect GetSpriteDomain(Sprite sprite)
        {
            foreach (PositionedSprite pos_sprite in PosSprites)
            {
                if (pos_sprite.Sprite.Equals(sprite))
                {
                    return pos_sprite.Domain;
                }
            }

            throw new ArgumentException("Sprite not found.");
        }
    }
}