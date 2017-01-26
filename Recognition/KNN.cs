namespace Recognition
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    public class KNN
    {
        private const int K = 3;

        private IDictionary<char, IList<Bitmap>> trainingData;

        public KNN()
        {
            this.LoadTrainingData();
        }

        public string Classify(Bitmap blob)
        {
            IDictionary<char, int> knn = new Dictionary<char, int>();

            foreach (ICollection<Bitmap> classValues in this.trainingData.Values)
            {
                foreach (Bitmap trainingBlob in classValues)
                {
                    for (int row = 0; row < trainingBlob.Height; row++)
                    {
                        for (int col = 0; col < trainingBlob.Width; col++)
                        {

                        }
                    }
                }                
            }

            return null;
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