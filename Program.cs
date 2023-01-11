using System;
using System.Threading;


class BruteForce
{
    private char[] _word;
    private char[] _findWord;
    private int length;

    // Сам брутфорс
    public void Brute()
    {
        while (true)
        {
            for (int i = 1; i < length; i++)
            {
                if (_word[length - i] == '{')
                {
                    _word[length - i] = 'a';
                    _word[length - i - 1]++;
                }
            }
            if (Enumerable.SequenceEqual(_word, _findWord)) return;
            Console.WriteLine(new String(_word));
            _word[length - 1]++;
        }
    }
    
    // Конструктор
    public BruteForce(string word)
    {
        length = word.Length;
        _word = new char[length];
        _findWord = word.ToCharArray();

        for (int i = 0; i < _word.Length; i++)
            _word[i] = 'a';
    }
}
    

class Program
{
    static void Main()
    {
        BruteForce bruteForce = new BruteForce("hello");
        bruteForce.Brute();
        
    }
}