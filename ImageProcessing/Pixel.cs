namespace ImageProcessing
{
    internal class Pixel
    {
        private int x;
        private int y;
        private int label;
        private byte color;

        public Pixel(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Pixel(int x, int y, byte color)
            : this(x, y)
        {
            this.color = color;
        }


        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        public int Label
        {
            get { return this.label; }
            set { this.label = value; }
        }

        public byte Color
        {
            get { return this.color; }
            set { this.color = value; }
        }
    }
}