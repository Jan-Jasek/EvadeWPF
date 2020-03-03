using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeLogic
{
    public static class HelperMethods
    {

        public static string ParseInputLog(string coordinates)
        {
            coordinates = AppConstants.ColumnValues[coordinates[0].ToString()] +
                          coordinates.Substring(1, coordinates.Length - 1);

            return coordinates;
        }

        public static string ParseOutputLog(string turnLog)
        {
            string outputLog = "";
            foreach (KeyValuePair<string, int> item in AppConstants.ColumnValues)
            {
                if (item.Value == int.Parse(turnLog[0].ToString()))
                {
                    //outputLog = item.Key + turnLog.Substring(1, turnLog.Length - 1);
                    outputLog = item.Key;
                    break;
                }
            } 
            
            foreach (KeyValuePair<string, int> item in AppConstants.ColumnValues)
            {
                if (item.Value == int.Parse(turnLog[3].ToString()))
                {
                    turnLog = outputLog + turnLog[1] + turnLog[2] + item.Key + turnLog[4] + turnLog[5] + turnLog[6];
                    break;
                }
            }

            return turnLog;
        }

        public static int ToInt(char position)
        {
            int.TryParse(position.ToString(), out int intPos);
            return intPos;
        }

        public static bool IsBetween(this double num, double lower, double upper, bool inclusive = false)
        {
            return inclusive
                ? lower <= num && num <= upper
                : lower < num && num < upper;
        }

        public static bool EqualsAny(string name, params string[] names) => names.Any(n => n == name);
        public static bool EqualsAny(int value, params int[] values) => values.Any(v => v == value);

        //public static bool EqualsAny<T>(T name, params T[] names) => names.Any(n => n == name);


        public static int[,] CloneArray(int[,] array)
        {
            int[,] clone = new int[AppConstants.BoardSize + 2, AppConstants.BoardSize + 2];
            for (int i = 0; i < AppConstants.BoardSize + 1; i++)
            {
                for (int j = 0; j < AppConstants.BoardSize + 1; j++)
                {
                    clone[i, j] = array[i, j];
                }
            }

            return clone;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = AppConstants.Rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
