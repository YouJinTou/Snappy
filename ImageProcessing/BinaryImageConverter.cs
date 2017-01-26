namespace ImageProcessing
{
    using System.Drawing;

    public class BinaryImageConverter
    {
        private const int NaiveThreshold = 100;
        private const int NaiveBlackThreshold = 0;

        public Bitmap SaveAsBinaryImage(string inPath, string outPath)
        {
            Bitmap original = new Bitmap(inPath);
            Bitmap grayScaleBitmap = this.ConvertColorToGrayScale(original);
            Bitmap binaryBitmap = this.ConvertGrayScaleToBinary(grayScaleBitmap);

            binaryBitmap.Save(outPath);

            return new Bitmap(outPath);
        }

        private Bitmap ConvertColorToGrayScale(Bitmap original)
        {
            Bitmap grayScaleBitmap = new Bitmap(original);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixelColor = original.GetPixel(x, y);
                    int grayScale = (int)((pixelColor.R * 0.3) + (pixelColor.G * 0.59) + (pixelColor.B * 0.11));
                    Color grayScaleColor = Color.FromArgb(grayScale, grayScale, grayScale);

                    grayScaleBitmap.SetPixel(x, y, grayScaleColor);
                }
            }

            return grayScaleBitmap;
        }

        private Bitmap ConvertGrayScaleToBinary(Bitmap grayScale)
        {
            Bitmap binaryBitmap = new Bitmap(grayScale);

            for (int y = 0; y < grayScale.Height; y++)
            {
                for (int x = 0; x < grayScale.Width; x++)
                {
                    byte grayScaleColor = grayScale.GetPixel(x, y).R;
                    Color binaryColor = (grayScaleColor < NaiveBlackThreshold) ? Color.White :
                        (grayScaleColor > NaiveThreshold) ? Color.White : Color.Black;

                    binaryBitmap.SetPixel(x, y, binaryColor);
                }
            }

            return binaryBitmap;
        }
    }
}