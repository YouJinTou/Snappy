namespace Recognition
{
    internal class Neighbor
    {
        private char classLabel;
        private double distance;

        public Neighbor(char classLabel, double distance)
        {
            this.classLabel = classLabel;
            this.distance = distance;
        }

        public char ClassLabel
        {
            get { return this.classLabel; }
        }

        public double Distance
        {
            get { return this.distance; }
        }
    }
}