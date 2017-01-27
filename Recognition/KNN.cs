namespace Recognition
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    public class KNN
    {
        private const int K = 3;

        private IDictionary<char, IList<Bitmap>> trainingData;

        public KNN()
        {
            this.LoadTrainingData();
        }

        public char Classify(Bitmap blob)
        {
            IList<Neighbor> knn = new List<Neighbor>();

            foreach (var entry in this.trainingData)
            {
                foreach (Bitmap trainingBlob in entry.Value)
                {
                    double distance = 0.0;

                    for (int y = 0; y < trainingBlob.Height; y++)
                    {
                        for (int x = 0; x < trainingBlob.Width; x++)
                        {
                            int blobPixelColor = blob.GetPixel(x, y).R;
                            int trainingPixelColor = trainingBlob.GetPixel(x, y).R;
                            distance += Math.Pow(blobPixelColor - trainingPixelColor, 2);
                        }
                    }

                    distance = Math.Sqrt(distance);

                    knn.Add(new Neighbor(entry.Key, distance));
                }
            }
                       
            var neighborGroups = knn.OrderBy(n => n.Distance).Take(K).GroupBy(n => n.ClassLabel);
            int highestCount = 0;
            char bestLabel = ' ';

            foreach (var group in neighborGroups)
            {
                if (highestCount < group.Count())
                {
                    bestLabel = group.Key;
                    highestCount = group.Count();
                }   
            }

            return (bestLabel == '.') ? ' ' : bestLabel;
        }

        private void LoadTrainingData()
        {
            string trainingDataPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", "Recognition", "TrainingData"));
            DirectoryInfo dir = new DirectoryInfo(trainingDataPath);
            this.trainingData = new Dictionary<char, IList<Bitmap>>();

            foreach (FileInfo file in dir.GetFiles("*.png"))
            {
                char classLabel = file.Name[0];

                if (!this.trainingData.ContainsKey(classLabel))
                {
                    this.trainingData[classLabel] = new List<Bitmap>();
                }

                this.trainingData[classLabel].Add(new Bitmap(file.FullName));
            }
        }
    }
}