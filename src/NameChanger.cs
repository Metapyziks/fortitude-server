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
    public class CacheNamer
    {
        private List<String> placeNamePrefix = new List<String>();
        private List<String> placeNameSuffix = new List<String>();
        private List<String> cacheNameAsFirstWord = new List<String>();
        private List<String> cacheNameAsSecondWord = new List<String>();
        private List<String> cacheTypeAsFirstWord = new List<String>();
        private List<String> cacheTypeAsSecondWord = new List<String>();
        private List<String> cacheFullName = new List<String>();

        Random randomNumbers = new Random();



        /**
         * Load _all_ the data! All text files stored in subfolder of this folder.
         */
        public CacheNamer()
        {
            String contentDir = ContentManager.ContentDir + "/namegen/";
            loadFileToArray(contentDir + "placeNamePrefix.txt", placeNamePrefix);
            loadFileToArray(contentDir + "placeNameSuffix.txt", placeNameSuffix);
            loadFileToArray(contentDir + "cacheNameAsFirstWord.txt", cacheNameAsFirstWord);
            loadFileToArray(contentDir + "cacheNameAsSecondWord.txt", cacheNameAsSecondWord);
            loadFileToArray(contentDir + "cacheTypeAsFirstWord.txt", cacheTypeAsFirstWord);
            loadFileToArray(contentDir + "cacheTypeAsSecondWord.txt", cacheTypeAsSecondWord);
            loadFileToArray(contentDir + "cacheFullName.txt", cacheFullName);
        }

        /**
         * Generates a cache name of random format (eg placename-type, or type-cachename etc)
         */

        public String generateRandomName()
        {
            String name = "";

            // Use chooseFormat as not every format has the same likelihood of being generated.
            int nameFormat = chooseFormat(randomNumbers.Next(101));

            switch (nameFormat) {
                case 0: name = getNameFromLists(cacheTypeAsFirstWord, cacheNameAsSecondWord);
                    break;
                case 1: name = getNameFromLists(cacheNameAsFirstWord, cacheTypeAsSecondWord);
                    break;
                case 2: name = getNameFromPlace();
                    break;
                case 3: name = getRandom(cacheFullName);
                    break;
            }

            return name;
        }

        /**
         * Takes a given number and returns 0, 1, 2 or 3 depending on the value of the input and
         * the likelihood of each name format being generated.
         */

        private int chooseFormat(int input)
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

        private String getNameFromPlace()
        {
            String name = "";

            name += getRandom(placeNamePrefix);
            name += getRandom(placeNameSuffix);
            name += " ";
            name += getRandom(cacheTypeAsSecondWord);

            return name;
        }

        /**
         * Generates a cache name with words chosen from two given array lists.
         */

        private String getNameFromLists(List<String> firstWord, List<String> secondWord)
        {
            String name = "";

            name += getRandom(firstWord);
            name += " ";
            name += getRandom(secondWord);

            return name;
        }


        /**
         * Returns a random entry from the given array lists.
         */

        private String getRandom(List<String> array)
        {
            int index = randomNumbers.Next(array.Count);
            return array[index];
        }

        /**
         * Take a text file and an array list of strings, and add each line of the text
         * file to the array list as a string.
         */

        private void loadFileToArray(String fileName, List<String> array)
        {
            array.AddRange(File.ReadAllLines(fileName).Where(x => x.Trim().Length > 0));
        }


        /**
         * Test the cacheNamer
         */

        public void test(int numberOfTests)
        {
            List<String> names = new List<String>();

            for (int i = 0; i < numberOfTests; i++) { names.Add(generateRandomName()); }

            printArray(names);
        }

        /**
         * Print an array to the console. Used to test methods.
         */

        private void printArray(List<String> array)
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
