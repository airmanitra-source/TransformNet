using System.Numerics;

namespace MachineLearning.ApiService.Models
{
    public static class SIMD
    {
        public static double Dot(double[] a, double[] b)
        {
            int count = Vector<double>.Count;
            Vector<double> resV = Vector<double>.Zero;
            int i = 0;
            for (; i <= a.Length - count; i += count)
                resV += new Vector<double>(a, i) * new Vector<double>(b, i);
            double res = Vector.Dot(resV, Vector<double>.One);
            for (; i < a.Length; i++) res += a[i] * b[i];
            return res;
        }
    }
}
