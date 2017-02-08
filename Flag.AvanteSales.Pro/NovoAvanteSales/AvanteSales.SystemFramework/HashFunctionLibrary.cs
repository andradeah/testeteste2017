#region Using directives

using System;

#endregion

namespace AvanteSales.SystemFramework
{
    /// <summary>
    /// Summary description for HashFunctionLibrary.
    /// </summary>
    public class HashFunctionLibrary
    {
        public static int RSHash(string str)
        {
            int b = 378551;
            int a = 63689;
            int hash = 0;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = hash * a + array[i];
                a = a * b;
            }

            return hash;
        }

        public static int JSHash(string str)
        {
            int hash = 1315423911;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash ^= ((hash << 5) + array[i] + (hash >> 2));
            }

            return hash;
        }

        public static int PJWHash(string str)
        {
            long BitsInUnsignedInt = (long)(4 * 8);
            long ThreeQuarters = (long)((BitsInUnsignedInt * 3) / 4);
            long OneEighth = (long)(BitsInUnsignedInt / 8);
            long HighBits = (long)(0xFFFFFFFF) << (int)(BitsInUnsignedInt - OneEighth);
            long hash = 0;
            long test = 0;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = (hash << (int)OneEighth) + array[i];

                if ((test = hash & HighBits) != 0)
                {
                    hash = ((hash ^ (test >> (int)ThreeQuarters)) & (~HighBits));
                }
            }

            return (int)hash;
        }

        public static int ELFHash(string str)
        {
            long hash = 0;
            long x = 0;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = (hash << 4) + array[i];

                if ((x = hash & 0xF0000000L) != 0)
                {
                    hash ^= (x >> 24);
                    hash &= ~x;
                }
            }

            return (int)hash;
        }

        public static int BKDRHash(string str)
        {
            long seed = 131313; // 31 131 1313 13131 131313 etc..
            long hash = 0;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = (hash * seed) + array[i];
            }

            return (int)hash;
        }

        public static int SDBMHash(string str)
        {
            int hash = 0;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = array[i] + (hash << 6) + (hash << 16) - hash;
            }

            return hash;
        }

        public static int DJBHash(string str)
        {
            int hash = 5381;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = ((hash << 5) + hash) + array[i];
            }

            return hash;
        }

        public static int DEKHash(string str)
        {
            int hash = str.Length;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = ((hash << 5) ^ (hash >> 27)) ^ array[i];
            }

            return hash;
        }

        public static int APHash(string str)
        {
            int hash = 0;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                if ((i & 1) == 0)
                {
                    hash ^= ((hash << 7) ^ array[i] ^ (hash >> 3));
                }
                else
                {
                    hash ^= (~((hash << 11) ^ array[i] ^ (hash >> 5)));
                }
            }

            return hash;
        }

        public static int CBUhash(string str)
        {
            int hash = 0;
            char[] array = str.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                hash = hash << 2 + array[i];
            }

            return hash;
        }
    }
}