import java.util.ArrayList;
import java.util.Random;
import java.io.*;

/**
 * Generates a random name appropriate for a cache in the game Fortitude.
 * 
 * @author Emma 
 * @version 24/01/2013
 */
public class CacheNamer
{
    private ArrayList<String> placeNamePrefix = new ArrayList<String>();
    private ArrayList<String> placeNameSuffix = new ArrayList<String>();
    private ArrayList<String> cacheNameAsFirstWord = new ArrayList<String>();
    private ArrayList<String> cacheNameAsSecondWord = new ArrayList<String>();
    private ArrayList<String> cacheTypeAsFirstWord = new ArrayList<String>();
    private ArrayList<String> cacheTypeAsSecondWord = new ArrayList<String>();
    private ArrayList<String> cacheFullName = new ArrayList<String>();
    
    Random randomNumbers = new Random();
    
    
    
    /**
     * Load _all_ the data! All text files stored in subfolder of this folder.
     */
    public CacheNamer()
    {
        loadFileToArray("Data\\placeNamePrefix.txt", placeNamePrefix);
        loadFileToArray("Data\\placeNameSuffix.txt", placeNameSuffix);
        loadFileToArray("Data\\cacheNameAsFirstWord.txt", cacheNameAsFirstWord);
        loadFileToArray("Data\\cacheNameAsSecondWord.txt", cacheNameAsSecondWord);
        loadFileToArray("Data\\cacheTypeAsFirstWord.txt", cacheTypeAsFirstWord);
        loadFileToArray("Data\\cacheTypeAsSecondWord.txt", cacheTypeAsSecondWord);
        loadFileToArray("Data\\cacheFullName.txt", cacheFullName);        
    }

    /**
     * Generates a cache name of random format (eg placename-type, or type-cachename etc)
     */
    
    public String generateRandomName ()
    {
        String name = "";
        
        // Use chooseFormat as not every format has the same likelihood of being generated.
        int nameFormat = chooseFormat(randomNumbers.nextInt(101));
        
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
    
    private int chooseFormat (int input)
    {
        int chosenFormat = 3; // Default format, if not changed below, is set to FullName
        
        int chanceOfTypeName = 19;
        int chanceOfNameType = 40 + chanceOfTypeName; // Must be cumulative for if statements
        int chanceOfPlaceType = 40 + chanceOfNameType;
        // Therefore chance of full name = 1, as far fewer combinations of this available
        
        if (input < chanceOfTypeName) { chosenFormat = 0; }
        else if (input < chanceOfNameType) { chosenFormat = 1; }
        else if (input < chanceOfPlaceType) { chosenFormat = 2; }
        
        return chosenFormat;
    }
    
    /**
     * Generates a cache name with the format placename-cacheType
     */
    
    private String getNameFromPlace ()
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
    
    private String getNameFromLists (ArrayList<String> firstWord, ArrayList<String> secondWord)
    {
        String name = "";
        
        name += getRandom (firstWord);
        name += " ";
        name += getRandom (secondWord);
        
        return name;
    }
    
    
    /**
     * Returns a random entry from the given array lists.
     */
    
    private String getRandom (ArrayList<String> array)
    {
        int index = randomNumbers.nextInt(array.size());
        return array.get(index);
    }
    
    /**
     * Take a text file and an array list of strings, and add each line of the text
     * file to the array list as a string.
     */
    
    private void loadFileToArray(String fileName, ArrayList<String> array)
    {
       // Use try and catch in case of invalid file name
        
        try {
            // Load file to program
            FileInputStream fstream = new FileInputStream(fileName);
            DataInputStream in = new DataInputStream(fstream);
            BufferedReader br = new BufferedReader(new InputStreamReader(in));
  
            String line;
            //Read File Line By Line
            while ((line = br.readLine()) != null)   {
               // Print the content on the console
               array.add(line);
            }
  
            //Close the input stream
            in.close();
        }
        catch (Exception e) {
            //Catch exception if any
            System.err.println("Error: " + e.getMessage());
        }
    }
    
    
    /**
     * Test the cacheNamer
     */
    
    public void test (int numberOfTests)
    {
        ArrayList<String> names = new ArrayList<String>();
        
        for (int i = 0; i < numberOfTests; i++) { names.add(generateRandomName()); }
        
        printArray (names);
    }
    
    /**
     * Print an array to the console. Used to test methods.
     */
    
    private void printArray (ArrayList<String> array)
    {
        int entryCounter = 0;
        
        for (String entry : array) {   
            System.out.println(entry + ", ");
            entryCounter += 1;
            if (entryCounter == 4) {
                System.out.print ("\n");
                entryCounter = 0;
            }
        } 
        
        System.out.println("\n");
    }
}
