using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HartStone
{
    public static class Extensions
    {
        public static bool ToBoolean(this string source)
        {

            int value;
            bool isInt = int.TryParse(source, out value);

            if (isInt)
            {
                if (value != 0 && value != 1)
                {
                    throw new ArgumentException("The source string appears to be an integer. Appropriate values are 'True', 'False', '1', and '0'", "source");
                }

                return Convert.ToBoolean(value);
            }

            bool isBoolString;

            if (!bool.TryParse(source, out isBoolString))
            {
                throw new ArgumentException("The source string could not be converted into a boolean. Appropriate values are 'True', 'False', '1', and '0'", "source");
            }

            return isBoolString;
        }

        private static T Random<T>(this List<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            T[] array = list.Where(w => w != null).Shuffle().ToArray();
            T value = default(T);
            int n = array.Length;
            if (n == 1)
                return array[0];
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return value;
        }

        public static IEnumerable<T> Random<T>(this IEnumerable<T> collection, int count, bool allowDuplicateResults = false)
        {
            if (count <= 0)
                yield break;

            List<T> list = collection.ToList();

            if (count > list.Count)
                count = list.Count;

            for (int i = 0; i < count; i++)
            {
                var result = list.Random();
                if (!allowDuplicateResults)
                    list.Remove(result);
                yield return result;
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            T[] array = list.ToArray();
            int n = array.Length;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return array.ToList();
        }

        private static T DrawOne<T>(ref List<T> list)
        {
            if (!list.Any())
            {
                return default(T);
            }

            T first = list.First();
            list.Remove(first);
            return first;
        }

        public static IEnumerable<T> Draw<T>(ref List<T> source, int count = 1)
        {
            if (count <= 0)
            {
                return null;
            }
;
            if (count > source.Count())
                count = source.Count();

            List<T> draws = new List<T>();
            for (int i = 0; i < count; i++)
            {
                draws.Add(DrawOne(ref source));
            }
            return draws;
        }
    }
}