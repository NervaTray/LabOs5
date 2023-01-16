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
    // Переменная нужна для отображения в консоли, что поток остановлен или работает.
    // (имеется в виду тот случай, когда пользователь намеренно его остановил
    // и он больше не добавляется в очередь).
    // false - работает;
    // true - остановлен.
    public bool OnWait = false;
    // Приоритет равен нулю, если поток, в котором совершался перебор уже выполнил свою работу.
    public int Priority = 1;
    
    public string Word => new String(_word);

    // Переменная для приостановки потока.
    private ManualResetEvent MRE = new ManualResetEvent(true);
    

    // Сам брутфорс
    public void Brute()
    {
        //Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Thread.Sleep(0);
        
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

            if (Enumerable.SequenceEqual(_word, _findWord)) return;
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
    private int _timeSlice = 100000;
    private Queue<BruteForce> _bruteObjectsQueue;
    private Queue<Thread> _threadQueue;
    public List<BruteForce> _bruteList;
    public List<Thread> _threadList;
    private Stopwatch? sw = null;
    private int _deadCount = 0;
    
    private BruteForce alpha;
    private BruteForce beta;
    private BruteForce gamma;

    private Thread alphaThread;
    private Thread betaThread;
    private Thread gammaThread;

    // Переменная для приостановки потока.
    private ManualResetEvent MRE = new ManualResetEvent(true);

    // Метод создает очередь и дает потоку проработать в течении кванта.
    public void Start()
    {
        while (true)
        {
            // Останавливает поток для введения команд.
            MRE.WaitOne();

            if (sw == null || sw.ElapsedTicks >= _timeSlice)
            {
                BruteForce temp = _bruteObjectsQueue.Dequeue();
                temp.Stop(true, _threadQueue.Peek().Name);

                if (_threadQueue.Peek().IsAlive)
                {
                    _bruteObjectsQueue.Enqueue(temp);
                    _threadQueue.Enqueue(_threadQueue.Dequeue());
                }
                else
                {
                    Thread threadTemp = _threadQueue.Dequeue();

                    if (temp.Priority != 0)
                    {
                        Console.WriteLine("-------------------------------------------------------");
                        Console.WriteLine("Конец {0}.", threadTemp.Name);
                        Console.WriteLine("Искомое слово: \"{0}\"", temp.Word);
                        Console.WriteLine("-------------------------------------------------------");
                        // Если поток полностью отработал, то повышается значение МЕРТВОГО счетчика.
                        _deadCount++;
                        temp.Priority = 0;
                    }
                }

                // Выходит из метода если все потоки отработали.
                if (_deadCount == 3) return;
                if (_bruteObjectsQueue.Count == 0)
                {
                    MRE.Reset();
                    Console.WriteLine("\nВсе незавершенные потоки находятся в режиме ожидания.");
                    continue;
                }

                _bruteObjectsQueue.Peek().Continue();
                sw = Stopwatch.StartNew();
            }
        }
    }

    // Останавливает все потоки.
    public void StopAll()
    {
        // Меняет флаг InitialState для остановки работы главного потока и метода Start().
        MRE.Reset();

        alpha.Stop(true, alphaThread.Name);
        beta.Stop(true, betaThread.Name);
        gamma.Stop(true, gammaThread.Name);
    }

    // Возобновляет работу БУПа.
    public void ContinueShedular()
    {
        if (_threadQueue.Count == 0)
        {
            Console.WriteLine("\nВсе потоки были поставлены в режим ожидания.");
            return;
        }
        MRE.Set();
    }

    // Ставит в режим ожидания поток.
    // На самом деле он просто удаляет его везде из очереди.
    // В очередь данный поток больше добавляться не будет.
    // Но он будет жив.
    public void SetOnWait(int numOfThread)
    {
        BruteForce bruteForce = _bruteList[numOfThread - 1];
        Thread threadOnWait = _threadList[numOfThread - 1];
        Thread threadTemp = null;
        BruteForce bruteTemp = null;
        bruteForce.OnWait = true;
        
        // Здесь мы проходимся по очереди экземпляров и потов.
        // Экземпляр и поток, которые должны быть поставлены
        // на ожидание не заносятся вновь в очереди.
        int tempLength = _threadQueue.Count;
        for (int i = 0; i < tempLength; i++)
        {
            if (_threadQueue.Count > 0) threadTemp = _threadQueue.Dequeue();
            if (_bruteObjectsQueue.Count > 0) bruteTemp = _bruteObjectsQueue.Dequeue();
            if (threadOnWait != threadTemp) _threadQueue.Enqueue(threadTemp);
            if (bruteForce != bruteTemp) _bruteObjectsQueue.Enqueue(bruteTemp);
        }
    }

    // Ставит поток в режим готовности.
    public void SetOnReady(int numOfThread)
    {
        BruteForce bruteForce = _bruteList[numOfThread - 1];
        Thread threadOnReady = _threadList[numOfThread - 1];
        bruteForce.OnWait = false;

        for (int i = 0; i < bruteForce.Priority; i++)
        {
            _bruteObjectsQueue.Enqueue(bruteForce);
            _threadQueue.Enqueue(threadOnReady);
        }
    }

    // +1 в очередь (увеличивает приоритет).
    public void AddInQueue(int numOfThread)
    {
        BruteForce bruteForce = _bruteList[numOfThread - 1];
        Thread threadOnReady = _threadList[numOfThread - 1];
        
        _bruteObjectsQueue.Enqueue(bruteForce);
        _threadQueue.Enqueue(threadOnReady);
    }

    // Уменьшает приоритет.
    public void DelFromQueue(int numOfThread)
    {
        BruteForce bruteForce = _bruteList[numOfThread - 1];
        Thread threadOnWait = _threadList[numOfThread - 1];
        Thread threadTemp = null;
        BruteForce bruteTemp = null;

        for (int i = 0; i < _threadList.Count; i++)
        {
            threadTemp = _threadQueue.Dequeue();
            bruteTemp = _bruteObjectsQueue.Dequeue();

            if (threadOnWait == threadTemp || bruteForce == bruteTemp) return;
            
            _threadQueue.Enqueue(threadTemp);
            _bruteObjectsQueue.Enqueue(bruteTemp);
        }
    }
    
    // Конструктор БУПа.
    public Sheduler(string alphaWord, string betaWord, string gammaWord)
    {
        // Инициализация экземпляров класса BruteForce.
        alpha = new BruteForce(alphaWord);
        beta = new BruteForce(betaWord);
        gamma = new BruteForce(gammaWord);

        // Инициализация потоков с методом экземпляра класса BruteForce.
        alphaThread = new Thread(alpha.Brute);
        betaThread = new Thread(beta.Brute);
        gammaThread = new Thread(gamma.Brute);

        // Задание имен для каждого потока.
        alphaThread.Name = "Alpha Thread";
        betaThread.Name = "Beta Thread";
        gammaThread.Name = "Gamma Thread";

        // Запуск и остановка каждого потока для дальнейшей работы в очереди.
        alphaThread.Start();
        alpha.Stop();
        betaThread.Start();
        beta.Stop();
        gammaThread.Start();
        gamma.Stop();

        // Инициализация списка экземпляров класса BruteForce.
        _bruteList = new List<BruteForce>();
        _bruteList.Add(alpha);
        _bruteList.Add(beta);
        _bruteList.Add(gamma);

        // Инициализация списка потоков.
        _threadList = new List<Thread>();
        _threadList.Add(alphaThread);
        _threadList.Add(betaThread);
        _threadList.Add(gammaThread);
        
        // Занесение в очередь экземпляров класса BruteForce. 
        _bruteObjectsQueue = new Queue<BruteForce>();
        for (int i = 0; i < 3; i++)
            _bruteObjectsQueue.Enqueue(_bruteList[i]);

        // Занесение в очередь потоков.
        _threadQueue = new Queue<Thread>();
        for (int i = 0; i < 3; i++)
            _threadQueue.Enqueue(_threadList[i]);
    }
}


class Program
{
    static void Main()
    {
        ConsoleKeyInfo cki;
        Sheduler sheduler = new Sheduler("hello", "daily", "ahega");
        //Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Task spy = new Task(() =>
        {
            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            while (true)
            {
                
                cki = Console.ReadKey(true);
                // При нажатии на клавишу олицетворяющую номер потока, все иные потоки останавливаются и вызывается меню
                // по настройке выбранного потока.
                if (cki.Key == ConsoleKey.D1 || cki.Key == ConsoleKey.D2 || cki.Key == ConsoleKey.D3)
                {
                    int temp = Int32.Parse(cki.Key.ToString()[cki.Key.ToString().Length - 1].ToString());
                    if (!sheduler._threadList[temp - 1].IsAlive)
                    {
                        Console.WriteLine("--- Отказано в доступе. Поток {0} уже завершен ---", sheduler._threadList[temp - 1].Name);
                        continue;
                    }
                    
                    // Останавливает все потоки.
                    sheduler.StopAll();

                    Console.WriteLine("\n--- Выбран поток {0} для настройки ({1}) ---\n", 
                        temp, sheduler._threadList[temp - 1].Name);
                    // ПЕРЕДЕЛАТЬ
                    if (sheduler._bruteList[temp - 1].OnWait) Console.WriteLine("> Поток в режиме ОЖИДАНИЯ ");
                    else Console.WriteLine("> Поток ГОТОВ к работе");
                    Console.WriteLine("> Текущий уровень приоритета = {0}\n", sheduler._bruteList[temp - 1].Priority);
                    Console.WriteLine("Список команд:" +
                                      "\n<Q> - для выхода из настройки;" +
                                      "\n<S> - для введения потока в режим ожидания или его возобновление;" +
                                      "\n<+> - для увеличения приоритета потока;" +
                                      "\n<-> - для уменьшения приоритета потока.\n");

                    while (cki.Key != ConsoleKey.Q)
                    {
                        Console.Write("Команда: ");
                        cki = Console.ReadKey();
                        Console.WriteLine();

                        switch (cki.Key)
                        {
                            // Выход из цикла.
                            case ConsoleKey.Q:
                                break;
                            
                            // Постановка на режим ожидания или возобновления.
                            case ConsoleKey.S:
                                
                                if (sheduler._bruteList[temp - 1].OnWait)
                                {
                                    sheduler.SetOnReady(temp);
                                    Console.WriteLine("Поток ГОТОВ к работе.");
                                    
                                }
                                else
                                {
                                    sheduler.SetOnWait(temp);
                                    Console.WriteLine("Поток в режиме ОЖИДАНИЯ.");
                                }
                                break;
                            
                            // Увеличения приоритета.
                            case ConsoleKey.OemPlus:
                                if (sheduler._bruteList[temp - 1].Priority > 2)
                                {
                                    Console.WriteLine("Уровень приоритета не может быть больше 3.");
                                    continue;
                                }
                                sheduler.AddInQueue(temp);
                                sheduler._bruteList[temp - 1].Priority++;
                                Console.WriteLine("Уровень приоритета = {0}", sheduler._bruteList[temp - 1].Priority);
                                break;
                            
                            // Уменьшение приоритета.
                            case ConsoleKey.OemMinus:
                                if (sheduler._bruteList[temp - 1].Priority < 2)
                                {
                                    Console.WriteLine("Уровень приоритета не может быть меньше 1.");
                                    continue;
                                }
                                sheduler.DelFromQueue(temp);
                                sheduler._bruteList[temp - 1].Priority--;
                                Console.WriteLine("Уровень приоритета = {0}", sheduler._bruteList[temp - 1].Priority);
                                break;
                            
                            default:
                                Console.WriteLine("Такой команды нет.");
                                break;
                        }
                    }
                    
                    // Возобновление потока Main().
                    sheduler.ContinueShedular();
                }
            }
        });
        
        Console.WriteLine("!Минимальный уровень приоритета потока = 1; Максимальный уровень приоритета потока = 3!\n");
        Console.WriteLine("Для выбора потока для настройки введите число от 1 до 3." +
                          "\n[1] - Alpha поток." +
                          "\n[2] - Beta поток." +
                          "\n[3] - Gamma поток.\n\n" +
                          "Нажмите <Enter> для начала работы программы.\n");
        Console.ReadLine();
        
        spy.Start();
        sheduler.Start();
    }
}