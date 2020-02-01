namespace DNMKSpritesheet
{
    public struct Int32Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Int32Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}