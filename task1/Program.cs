using System;
using System.Text;

internal static class Compressor
{
    static public string Compress(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= 1) return input;

        StringBuilder result = new StringBuilder();
        int counter = 1;

        result.Append(input[0]);
        for (int i = 1; i < input.Length; i++)
        {
            if (input[i] == result[result.Length - 1])
            {
                counter++;
            }
            else
            {
                if (counter > 1)
                {
                    result.Append(counter);
                }
                counter = 1;
                result.Append(input[i]);
            }
        }
        if (counter > 1) result.Append(counter.ToString());

        return result.ToString();
    }

    static public string Decompress(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= 1) return input;
        
        StringBuilder result = new StringBuilder();

        int i = 1;
        int amount = 0;
        char currSymbol = input[0];
        bool symbolIsUniq = true;
        while (i < input.Length)
        {
            if (char.IsDigit(input[i]))
            {
                amount = amount * 10 + input[i] - '0';
                symbolIsUniq &= (input[i] != '0');
            }
            else
            {
                result.Append(currSymbol, (amount == 0 && symbolIsUniq) ? 1 : amount);
                amount = 0;
                currSymbol = input[i];
                symbolIsUniq = true;
            }
            i++;
        }
        result.Append(currSymbol, (amount == 0 && symbolIsUniq) ? 1 : amount);

        return result.ToString();
    }

    static public int Test()
    {
        //from the assignment
        if (Compress("aaabbcccdde") != "a3b2c3d2e") return 1;
        if (Compressor.Decompress("a3b2c3d2e") != "aaabbcccdde") return 1;

        //boundary cases
        if (Compressor.Compress(null) != null) return 1;
        if (Compressor.Compress("") != "") return 1;
        if (Compressor.Compress("a") != "a") return 1;
        if (Compressor.Compress("abcaabbccyyyyyyyyyyyy") != "abca2b2c2y12") return 1;

        //boundary cases
        if (Compressor.Decompress(null) != null) return 1;
        if (Compressor.Decompress("") != "") return 1;
        if (Compressor.Decompress("a") != "a") return 1;
        if (Compressor.Decompress("a2a2bbb2a2ac3w11d") != "aaaabbbbaaacccwwwwwwwwwwwd") return 1;

        //empty unclenching
        if (Compressor.Decompress("a0") != "") return 1;
        if (Compressor.Decompress("a00000") != "") return 1;
        if (Compressor.Decompress("a000003") != "aaa") return 1;
        if (Compressor.Decompress("a0b1c0c0c0t2t0") != "btt") return 1;

        //unhandled exception expected
        bool exceptionThrown = false;
        try
        {
            Compressor.Decompress("a999999999999");
        }
        catch (ArgumentOutOfRangeException) { exceptionThrown = true; }
        catch (OutOfMemoryException) { exceptionThrown = true; }
        if (!exceptionThrown) return 1;


        return 0;
    }
};

internal class Program
{
    static void Main()
    {
        string a = "aaabbcccdde", b = "a3b2c3d2e";
        Console.WriteLine(a + " -> " + Compressor.Compress(a));
        Console.WriteLine(b + " -> " + Compressor.Decompress(b) + "\n");
        
        if (Compressor.Test() != 0) Console.WriteLine("The Compressor class is incorrect.");
        else Console.WriteLine("Test passed");
    }
}
