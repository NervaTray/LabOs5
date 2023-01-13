using System;
using System.Diagnostics;
using System.Threading;


class BruteForce
{
    private char[] _word;
    private char[] _findWord;
    private int length;
    // Показывает initialState флаг экземпляра класса ManualResetEvent.
    private bool _initialState = true;
    private int _count = 0;

    public bool InitialState => _initialState;

    // Переменная для приостановки потока.
    private ManualResetEvent MRE = new ManualResetEvent(true);
    

    // Сам брутфорс
    public void Brute()
    {
        
        while (true)
        {
            // Останавливает поток.
            MRE.WaitOne();
            
            
            for (int i = 1; i < length; i++)
            {
                if (_word[length - i] == '{')
                {
                    _word[length - i] = 'a';
                    _word[length - i - 1]++;
                }
            }

            if (Enumerable.SequenceEqual(_word, _findWord))
            {
                Console.WriteLine(new String(_word));
                return;
            }
            //Console.WriteLine(new String(_word));
            _word[length - 1]++;
            _count++;

        }
    }

    // Останавливает поток.
    public void Stop(bool printAllow = false, string name = "")
    {
        MRE.Reset();
        _initialState = false;
        if (printAllow) Console.WriteLine("Combinations of {0}: {1}", name, _count);
        _count = 0;
    }

    // Продолжает поток.
    public void Continue()
    {
        MRE.Set();
        _initialState = true;
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


class Sheduler
{
    // Квант.
    private int _timeSlice = 1000000;
    private Queue<BruteForce> _bruteObjectsQueue;
    private Queue<Thread> _threadQueue;
    private Stopwatch sw = null;
    

    // Метод создает очередь и дает потоку проработать в течении кванта.
    public void Start()
    {
        while (true)
        {
            if (sw == null || sw.ElapsedTicks >= _timeSlice)
            {
                
                BruteForce temp = _bruteObjectsQueue.Dequeue();
                temp.Stop(true, _threadQueue.Peek().Name);
                
                
                if (_threadQueue.Peek().IsAlive)
                {
                    _bruteObjectsQueue.Enqueue(temp);
                    _threadQueue.Enqueue(_threadQueue.Dequeue());
                }
                else _threadQueue.Dequeue();

                if (_bruteObjectsQueue.Count == 0) return;
                
                
                _bruteObjectsQueue.Peek().Continue();
                sw = Stopwatch.StartNew();
            }
        }
    }
    
    // Конструктор БУПа.
    public Sheduler()
    {
        BruteForce alpha = new BruteForce("ahega");
        BruteForce beta = new BruteForce("daily");
        BruteForce gamma = new BruteForce("yammy");

        Thread alphaThread = new Thread(alpha.Brute);
        Thread betaThread = new Thread(beta.Brute);
        Thread gammaThread = new Thread(gamma.Brute);

        alphaThread.Name = "Alpha Thread";
        betaThread.Name = "Beta Thread";
        gammaThread.Name = "Gamma Thread";

        alphaThread.Start();
        alpha.Stop();
        betaThread.Start();
        beta.Stop();
        gammaThread.Start();
        gamma.Stop();

        _bruteObjectsQueue = new Queue<BruteForce>();
        _bruteObjectsQueue.Enqueue(alpha);
        _bruteObjectsQueue.Enqueue(beta);
        _bruteObjectsQueue.Enqueue(gamma);

        _threadQueue = new Queue<Thread>();
        _threadQueue.Enqueue(alphaThread);
        _threadQueue.Enqueue(betaThread);
        _threadQueue.Enqueue(gammaThread);
    }
}
    

class Program
{
    static void Main()
    {
        ConsoleKeyInfo cki;
        //BruteForce test = new BruteForce("hellojkih");
        Sheduler sheduler = new Sheduler();
        
        
        sheduler.Start();

        //Sheduler sheduler = new Sheduler();
        // Stopwatch sw = null;
        // int _timeSlice = 1000000;
        //
        // bool trigger = true;
        //
        // Thread th = new Thread(test.Brute);
        // th.Start();

        // while (true)
        // {
        //     if (sw == null || sw.ElapsedTicks >= _timeSlice)
        //     { 
        //         if (trigger)
        //         {
        //             test.Stop();
        //             trigger = false;
        //         }
        //         else
        //         {
        //             test.Continue();
        //             trigger = true;
        //         }
        //         sw = Stopwatch.StartNew();
        //     }
        //
        //     // cki = Console.ReadKey();
        //     // if (cki.Key == ConsoleKey.D2)
        //     // {
        //     //     if (test.InitialState) test.Stop();
        //     //     else test.Continue();
        //     // }
        //     
        //     // Console.ReadLine();
        //     // if (trigger)
        //     // {
        //     //     test.Stop();
        //     //     trigger = false;
        //     // }
        //     // else
        //     // {
        //     //     test.Continue();
        //     //     trigger = true;
        //     // }
        // }
    }
}