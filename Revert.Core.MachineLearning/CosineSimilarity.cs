using System;
using System.Linq;

namespace Revert.Core.MachineLearning
{
    public static class CosineSimilarity
    {
        public static float GetVectorCosSimilarity(int[] vector, int[] secondVector)
        {
            var vectorMagnitude = Math.Sqrt(vector.Sum(a => Math.Pow(a, 2)));
            var vectorProducts = vector.Zip(secondVector, (a, b) => (double)a * b);
            var dotProduct = vectorProducts.Sum();
            var secondVectorMagnitude = Math.Sqrt(secondVector.Sum(b => Math.Pow(b, 2)));
            var magnitudeProduct = vectorMagnitude * secondVectorMagnitude;
            return (float)(dotProduct / magnitudeProduct);
        }
    }
}
