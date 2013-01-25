using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{

    /**
     * Generates a random name appropriate for a cache in the game Fortitude.
     * 
     * @author Emma 
     * @version 24/01/2013
     */
    public static class CacheNamer
    {
        private static List<String> placeNamePrefix = new List<String>();
        private static List<String> placeNameSuffix = new List<String>();
        private static List<String> cacheNameAsFirstWord = new List<String>();
        private static List<String> cacheNameAsSecondWord = new List<String>();
        private static List<String> cacheTypeAsFirstWord = new List<String>();
        private static List<String> cacheTypeAsSecondWord = new List<String>();
        private static List<String> cacheFullName = new List<String>();

        private static Random randomNumbers = new Random();



        /**
         * Load _all_ the data! All text files stored in subfolder of this folder.
         */
        static CacheNamer()
        {
            String contentDir = ContentManager.ContentDir + "/namegen/";
            LoadFileToArray(contentDir + "placeNamePrefix.txt", placeNamePrefix);
            LoadFileToArray(contentDir + "placeNameSuffix.txt", placeNameSuffix);
            LoadFileToArray(contentDir + "cacheNameAsFirstWord.txt", cacheNameAsFirstWord);
            LoadFileToArray(contentDir + "cacheNameAsSecondWord.txt", cacheNameAsSecondWord);
            LoadFileToArray(contentDir + "cacheTypeAsFirstWord.txt", cacheTypeAsFirstWord);
            LoadFileToArray(contentDir + "cacheTypeAsSecondWord.txt", cacheTypeAsSecondWord);
            LoadFileToArray(contentDir + "cacheFullName.txt", cacheFullName);
        }

        /**
         * Generates a cache name of random format (eg placename-type, or type-cachename etc)
         */

        public static String GenerateRandomName()
        {
            String name = "";

            // Use chooseFormat as not every format has the same likelihood of being generated.
            int nameFormat = ChooseFormat(randomNumbers.Next(101));

            switch (nameFormat) {
                case 0: name = GetNameFromLists(cacheTypeAsFirstWord, cacheNameAsSecondWord);
                    break;
                case 1: name = GetNameFromLists(cacheNameAsFirstWord, cacheTypeAsSecondWord);
                    break;
                case 2: name = GetNameFromPlace();
                    break;
                case 3: name = GetRandom(cacheFullName);
                    break;
            }

            return name;
        }

        public static IEnumerable<String> GenerateRandomNames(int count)
        {
            return Enumerable.Range(0, count).Select(x => GenerateRandomName());
        }

        /**
         * Takes a given number and returns 0, 1, 2 or 3 depending on the value of the input and
         * the likelihood of each name format being generated.
         */

        private static int ChooseFormat(int input)
        {
            int chosenFormat = 3; // Default format, if not changed below, is set to FullName

            int chanceOfTypeName = 19;
            int chanceOfNameType = 40 + chanceOfTypeName; // Must be cumulative for if statements
            int chanceOfPlaceType = 40 + chanceOfNameType;
            // Therefore chance of full name = 1, as far fewer combinations of this available

            if (input < chanceOfTypeName) { chosenFormat = 0; } else if (input < chanceOfNameType) { chosenFormat = 1; } else if (input < chanceOfPlaceType) { chosenFormat = 2; }

            return chosenFormat;
        }

        /**
         * Generates a cache name with the format placename-cacheType
         */

        private static String GetNameFromPlace()
        {
            String name = "";

            name += GetRandom(placeNamePrefix);
            name += GetRandom(placeNameSuffix);
            name += " ";
            name += GetRandom(cacheTypeAsSecondWord);

            return name;
        }

        /**
         * Generates a cache name with words chosen from two given array lists.
         */

        private static String GetNameFromLists(List<String> firstWord, List<String> secondWord)
        {
            String name = "";

            name += GetRandom(firstWord);
            name += " ";
            name += GetRandom(secondWord);

            return name;
        }


        /**
         * Returns a random entry from the given array lists.
         */

        private static String GetRandom(List<String> array)
        {
            int index = randomNumbers.Next(array.Count);
            return array[index];
        }

        /**
         * Take a text file and an array list of strings, and add each line of the text
         * file to the array list as a string.
         */

        private static void LoadFileToArray(String fileName, List<String> array)
        {
            array.AddRange(File.ReadAllLines(fileName).Where(x => x.Trim().Length > 0));
        }


        /**
         * Test the cacheNamer
         */

        public static void Test(int numberOfTests)
        {
            List<String> names = new List<String>();

            for (int i = 0; i < numberOfTests; i++) { names.Add(GenerateRandomName()); }

            PrintArray(names);
        }

        /**
         * Print an array to the console. Used to test methods.
         */

        private static void PrintArray(List<String> array)
        {
            int entryCounter = 0;

            foreach (String entry in array) {
                Console.WriteLine(entry + ", ");
                entryCounter += 1;
                if (entryCounter == 4) {
                    Console.WriteLine();
                    entryCounter = 0;
                }
            }

            Console.WriteLine("\n");
        }
    }
}
