namespace ImageProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    public class ComponentExtractor
    {
        private const byte GrayScaleWhite = 255;
        private const int HeightIgnoreThreshold = 2;
        private const int WidthIgnoreThreshold = 1;
        private const int HeightMergeSensitivity = 15;
        private const int WidthMergeSensitivity = 1;
        private const int IntervalThreshold = 5;

        private Bitmap binaryImage;
        private Pixel[,] matrix;
        private HashSet<int> labels;
        private Bitmap interval;

        public ComponentExtractor(Bitmap binaryImage)
        {
            this.binaryImage = binaryImage;
            this.matrix = new Pixel[this.binaryImage.Height, this.binaryImage.Width];
            this.labels = new HashSet<int>();

            this.FillPixelMatrix();
            this.LabelComponents();
        }

        public Bitmap Interval
        {
            get
            {
                if (this.interval.Height == 0)
                {
                    this.interval = new Bitmap(20, 20);

                    for (int y = 0; y < this.interval.Height; y++)
                    {
                        for (int x = 0; x < this.interval.Width; x++)
                        {
                            this.interval.SetPixel(x, y, Color.White);
                        }
                    }
                }

                return this.interval;
            }
        }

        public IList<Bitmap> Extract()
        {
            IList<Rectangle> boundingBoxes = new List<Rectangle>();
            IList<Bitmap> components = new List<Bitmap>();

            foreach (int label in this.labels)
            {
                Rectangle box = this.GetBoundingBox(label);

                boundingBoxes.Add(box);
            }

            boundingBoxes = this.SortComponents(boundingBoxes);

            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                Rectangle box = boundingBoxes[i];
                bool shouldSkip = (box.Height <= HeightIgnoreThreshold || box.Width <= WidthIgnoreThreshold);

                if (shouldSkip)
                {
                    continue;
                }

                box = this.TryMergeBlobs(box, boundingBoxes);
                Bitmap blob = this.CropBlob(box);
                blob = this.ResizeBlob(blob);

                components.Add(blob);

                this.InsertInterval(i, components, boundingBoxes);
            }

            return components;
        }
        
        private Bitmap CropBlob(Rectangle box)
        {
            Bitmap blob = new Bitmap(box.Width, box.Height);
            Graphics graphics = Graphics.FromImage(blob);

            graphics.DrawImage(this.binaryImage, 0, 0, box, GraphicsUnit.Pixel);

            return blob;
        }

        private void FillPixelMatrix()
        {
            for (int y = 0; y < this.binaryImage.Height; y++)
            {
                for (int x = 0; x < this.binaryImage.Width; x++)
                {
                    Color color = this.binaryImage.GetPixel(x, y);
                    this.matrix[y, x] = new Pixel(x, y, color.R);
                }
            }
        }

        private Rectangle GetBoundingBox(int label)
        {
            int minRow = int.MaxValue;
            int maxRow = int.MinValue;
            int minCol = int.MaxValue;
            int maxCol = int.MinValue;

            for (int row = 0; row < this.matrix.GetLength(0); row++)
            {
                for (int col = 0; col < this.matrix.GetLength(1); col++)
                {
                    if (this.matrix[row, col].Label == label)
                    {
                        bool betterMinRowFound = row < minRow;
                        bool betterMaxRowFound = row > maxRow;
                        bool betterMinColFound = col < minCol;
                        bool betterMaxColFound = col > maxCol;
                        minRow = (betterMinRowFound ? row : minRow);
                        maxRow = (betterMaxRowFound ? row : maxRow);
                        minCol = (betterMinColFound ? col : minCol);
                        maxCol = (betterMaxColFound ? col : maxCol);
                    }
                }
            }

            Rectangle box = new Rectangle(minCol, minRow, maxCol - minCol, maxRow - minRow);

            return box;
        }

        private void LabelComponents()
        {
            Queue<Pixel> queue = new Queue<Pixel>();
            int currentLabel = 1;

            for (int y = 0; y < this.binaryImage.Height; y++)
            {
                for (int x = 0; x < this.binaryImage.Width; x++)
                {
                    Pixel pixel = this.matrix[y, x];
                    bool shouldSkip = (pixel.Label != 0 || pixel.Color == GrayScaleWhite);

                    if (!shouldSkip)
                    {
                        queue.Enqueue(pixel);

                        this.LabelComponents(queue, currentLabel);

                        currentLabel++;
                    }
                }
            }
        }

        private void LabelComponents(Queue<Pixel> queue, int currentLabel)
        {
            while (queue.Count > 0)
            {
                Pixel pixel = queue.Dequeue();
                bool shouldSkip = (pixel.Label != 0 || pixel.Color == GrayScaleWhite);

                if (shouldSkip)
                {
                    continue;
                }

                this.matrix[pixel.Y, pixel.X].Label = currentLabel;

                this.labels.Add(currentLabel);

                bool canGoRight = (pixel.X + 1 < this.binaryImage.Width);
                bool canGoLeft = (pixel.X - 1 >= 0);
                bool canGoUp = (pixel.Y - 1 >= 0);
                bool canGoDown = (pixel.Y + 1 < this.binaryImage.Height);
                bool canGoUpRight = (canGoRight & canGoUp);
                bool canGoUpLeft = (canGoLeft & canGoUp);
                bool canGoDownRight = (canGoRight & canGoDown);
                bool canGoDownLeft = (canGoLeft & canGoDown);

                if (canGoUpRight)
                {
                    queue.Enqueue(this.matrix[pixel.Y - 1, pixel.X + 1]);
                }

                if (canGoRight)
                {
                    queue.Enqueue(this.matrix[pixel.Y, pixel.X + 1]);
                }

                if (canGoUpLeft)
                {
                    queue.Enqueue(this.matrix[pixel.Y - 1, pixel.X - 1]);
                }

                if (canGoLeft)
                {
                    queue.Enqueue(this.matrix[pixel.Y, pixel.X - 1]);
                }

                if (canGoUp)
                {
                    queue.Enqueue(this.matrix[pixel.Y - 1, pixel.X]);
                }

                if (canGoDownRight)
                {
                    queue.Enqueue(this.matrix[pixel.Y + 1, pixel.X + 1]);
                }

                if (canGoDown)
                {
                    queue.Enqueue(this.matrix[pixel.Y + 1, pixel.X]);
                }

                if (canGoDownLeft)
                {
                    queue.Enqueue(this.matrix[pixel.Y + 1, pixel.X - 1]);
                }
            }
        }

        private Bitmap ResizeBlob(Bitmap blob)
        {
            return new Bitmap(blob, 20, 20);
        }

        private Rectangle TryMergeBlobs(Rectangle box, IList<Rectangle> boundingBoxes)
        {
            int topMidPoint = (box.X + (box.Width / 2));
            int upperBound = (box.Y - HeightMergeSensitivity - 1);
            upperBound = (upperBound < 0 ? 0 : upperBound);

            for (int row = box.Y - 1; row >= upperBound; row--)
            {
                Rectangle boxToMergeWith = boundingBoxes.FirstOrDefault(b => b.Contains(topMidPoint, row));
                bool foundBox = (boxToMergeWith.Height > 0);

                if (!foundBox)
                {
                    continue;
                }

                boundingBoxes.Remove(boxToMergeWith);

                int mergedX = (box.X <= boxToMergeWith.X ? box.X : boxToMergeWith.X);
                int mergedWidth = (box.Right >= boxToMergeWith.Right ? box.Right : boxToMergeWith.Right) - mergedX;
                int mergedHeight = boxToMergeWith.Height + (box.Top - boxToMergeWith.Bottom) + box.Height;
                Rectangle mergedBox = new Rectangle(mergedX, boxToMergeWith.Y, mergedWidth, mergedHeight);

                return mergedBox;
            }

            //int rightMidPoint = (box.Y + (box.Height / 2));
            //int rightColBound = (box.Right + 1 + WidthMergeSensitivity);

            //for (int x = box.Right + 1; x <= rightColBound; x++)
            //{
            //    Rectangle boxToMergeWith = boundingBoxes.FirstOrDefault(b => b.Contains(x, rightMidPoint));
            //    bool foundBox = (boxToMergeWith.Height > 0);

            //    if (!foundBox)
            //    {
            //        continue;
            //    }

            //    int mergedY = (box.Y <= boxToMergeWith.Y) ? box.Y : boxToMergeWith.Y;
            //    int mergedWidth = (boxToMergeWith.Right - box.Left);
            //    int mergedBottom = (box.Bottom >= boxToMergeWith.Bottom) ? box.Bottom : boxToMergeWith.Bottom;
            //    int mergedHeight = (mergedBottom - mergedY);
            //    Rectangle mergedBox = new Rectangle(box.X, mergedY, mergedWidth, mergedHeight);

            //    return mergedBox;
            //}

            return box;
        }

        private void InsertInterval(int index, IList<Bitmap> components, IList<Rectangle> boundingBoxes)
        {
            if (index == boundingBoxes.Count - 1)
            {
                return;
            }

            bool shouldInsertInterval = ((boundingBoxes[index + 1].Left - boundingBoxes[index].Right) > IntervalThreshold);

            if (shouldInsertInterval)
            {
                components.Add(interval);
            }
        }
        
        private IList<Rectangle> SortComponents(IList<Rectangle> components)
        {
            int naiveBottomDeviation = 20;

            for (int i = 0; i < components.Count - 1; i++)
            {
                int swapIndex = i;

                for (int j = i + 1; j < components.Count; j++)
                {
                    bool shouldSwap = (components[j].Left < components[swapIndex].Left) &&
                        (Math.Abs(components[j].Bottom - components[swapIndex].Bottom) < naiveBottomDeviation);

                    if (shouldSwap)
                    {
                        swapIndex = j;
                    }
                }

                if (swapIndex != i)
                {
                    Rectangle holder = components[i];
                    components[i] = components[swapIndex];
                    components[swapIndex] = holder;
                }
            }

            return components;
        }
    }
}