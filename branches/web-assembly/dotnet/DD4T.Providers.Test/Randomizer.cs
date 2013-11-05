using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel;

namespace DD4T.Providers.Test
{
    static class Randomizer
    {
        static Random random = new Random(DateTime.Now.Second * DateTime.Now.Minute);
        static string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789,..     ";
        static string alphabetSafe = "abcdefghijklmnopqrstuvwxyz0123456789";
        public static string AnyString(int length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(alphabet.Length);
                result += alphabet.Substring(index, 1);
            }
            return result;
        }
        public static string AnySafeString(int length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(alphabetSafe.Length);
                result += alphabetSafe.Substring(index, 1);
            }
            return result;
        }

        public static string AnyUri(int itemType)
        {
            string result = "tcm:";
            int length = random.Next(1, 2);
            for (int i = 0; i < length; i++)
            {
                int nr = random.Next(9);
                result += Convert.ToString(nr);

            }
            result += "-";
            length = random.Next(3, 5);
            for (int i = 0; i < length; i++)
            {
                int nr = random.Next(9);
                result += Convert.ToString(nr);
            }
            return itemType == 16 ? result : result + "-" + itemType;
        }


        public static Field AnyTextField(int nameLength, int valueLength, bool isMultiLine)
        {
            Field f = new Field();
            f.Name = AnyString(nameLength);
            if (isMultiLine)
            {
                f.Values.Add(AnyString(valueLength) + "\r\n" + AnyString(valueLength));
            }
            else
            {
                f.Values.Add(AnyString(valueLength));
            }
            return f;
        }
    }
}
