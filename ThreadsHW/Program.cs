using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ThreadsHW
{
    internal class Program
    {
        class RandomThreadProcessor
        {
            private readonly Random[] _randoms;
            protected readonly Thread[] _threads;
            protected readonly int[] _array;

            public RandomThreadProcessor(int[] array, int threadCount)
            {
                var random = new Random();
                _randoms = new Random[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    _randoms[i] = new Random(random.Next());
                }

                _threads = new Thread[threadCount];
                _array = array;
            }

            public void Run()
            {
                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i] = new Thread(Process);
                }

                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Start(i);
                }

                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Join();
                }
            }

            protected virtual void Process(object? threadNumber)
            {
                var num = (int)threadNumber;

                var span = Span(threadNumber);

                var random = _randoms[num];

                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = random.Next();
                }
            }

            protected virtual Span<int> Span(object? threadNumber)
            {
                var itemsByThread = _array.Length / _threads.Length;

                var num = (int)threadNumber;

                var span =
                    num == _threads.Length - 1
                       ? _array[(num * itemsByThread)..]
                       : _array.AsSpan(num * itemsByThread, itemsByThread);

                return span;
            }
        }

        class SearchMinNumberThread : RandomThreadProcessor
        {
            private int _minValue = int.MaxValue;
            private readonly object _lockObject = new object();

            public SearchMinNumberThread(int[] array, int threadCount)
                : base(array, threadCount)
            {

            }

            public int GetMinValue()
            {
                Run();
                return _minValue;
            }

            protected override void Process(object? threadNumber)
            {
                base.Process(threadNumber);

                var localMin = int.MaxValue;

                var span = Span(threadNumber);

                for (var i = 0; i < span.Length; i++)
                {
                    if (span[i] < localMin)
                        localMin = span[i];
                }

                lock (_lockObject)
                {
                    if (localMin < _minValue)
                        _minValue = localMin;
                }
            }
        }

        class SearchMaxNumberThread : RandomThreadProcessor
        {
            private int _maxValue = int.MinValue;
            private readonly object _lockObject = new object();

            public SearchMaxNumberThread(int[] array, int threadCount)
                : base(array, threadCount)
            {

            }

            public int GetMaxValue()
            {
                Run();
                return _maxValue;
            }

            protected override void Process(object? threadNumber)
            {
                base.Process(threadNumber);

                var localMax = int.MinValue;

                var span = Span(threadNumber);

                for (var i = 0; i < span.Length; i++)
                {
                    if (span[i] > localMax)
                        localMax = span[i];
                }

                lock (_lockObject)
                {
                    if (localMax > _maxValue)
                        _maxValue = localMax;
                }
            }
        }

        class SearchSumAllElementsThread : RandomThreadProcessor
        {
            private long _sumValue = 0;
            private readonly object _lockObject = new object();

            public SearchSumAllElementsThread(int[] array, int threadCount)
                : base(array, threadCount)
            {

            }

            public long GetSumOfValues()
            {
                Run();
                return _sumValue;
            }

            protected override void Process(object? threadNumber)
            {
                base.Process(threadNumber);

                long localSum = 0;

                var span = Span(threadNumber);

                for (var i = 0; i < span.Length; i++)
                {
                    localSum += span[i];
                }

                lock (_lockObject)
                {
                    _sumValue += localSum;
                }
            }
        }

        class SearchAverageValueThread : RandomThreadProcessor
        {
            private long _averageValue = 0;
            private readonly object _lockObject = new object();

            public SearchAverageValueThread(int[] array, int threadCount)
                : base(array, threadCount)
            {

            }

            public long GetAverageValue()
            {
                Run();
                return _averageValue / _array.Length;
            }

            protected override void Process(object? threadNumber)
            {
                base.Process(threadNumber);

                long sumElements = 0;

                var span = Span(threadNumber);

                for (var i = 0; i < span.Length; i++)
                {
                    sumElements += span[i];
                }

                lock (_lockObject)
                {
                    _averageValue += sumElements;
                }
            }
        }

        class CopyHalfPartOfValuesThread : RandomThreadProcessor
        {
            private readonly int[] _arr;
            private readonly int _expectedLengthArray;
            private readonly int _chunkSize;
            private readonly object _lockObject = new object();

            public CopyHalfPartOfValuesThread(int[] array, int threadCount, int expectedLengthArray)
                : base(array, threadCount)
            {
                _expectedLengthArray = expectedLengthArray;
                _arr = new int[expectedLengthArray];
                _chunkSize = (expectedLengthArray + threadCount - 1) / threadCount;
            }

            public int[] GetHalfPartOfValues()
            {
                Run();
                return _arr;
            }

            protected override Span<int> Span(object? threadNumber)
            {
                var num = (int)threadNumber;

                var start = num * _chunkSize;
                var length = (num == _threads.Length - 1)
                    ? Math.Min(_expectedLengthArray - start, _chunkSize)
                    : _chunkSize;

                return _array.AsSpan(start, length);
            }

            protected override void Process(object? threadNumber)
            {
                base.Process(threadNumber);

                var num = (int)threadNumber;
                var span = Span(threadNumber);

                lock (_lockObject)
                {
                    var start = num * _chunkSize;
                    var end = Math.Min(start + _chunkSize, _expectedLengthArray);

                    for (var i = start; i < end; i++)
                    {
                        if (i - start < span.Length)
                        {
                            _arr[i] = span[i - start];
                        }
                    }
                }
            }
        }

        class CharacterFrequencyAnalyzerThread
        {
            protected readonly Thread[] _threads;
            protected Dictionary<string, int> _characterFrequency = new Dictionary<string, int>();
            protected readonly string _book;
            protected readonly object _lockObject = new object();

            public CharacterFrequencyAnalyzerThread(string book, int threadCount)
            {
                _book = book;
                _threads = new Thread[threadCount];
            }

            public void Run()
            {
                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i] = new Thread(Process);
                }

                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Start(i);
                }

                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Join();
                }
            }

            protected virtual void Process(object? threadNumber)
            {
                var num = (int)threadNumber;

                var itemsByThread = _book.Length / _threads.Length;

                var start = num * itemsByThread;

                var length = (num == _threads.Length - 1)
                    ? _book.Length - start
                    : itemsByThread;

                var span = _book.AsSpan(start, length);

                for (int i = 0; i < span.Length; i++)
                {
                    var currentChar = span[i];
                    var displayChar = currentChar switch
                    {
                        ' ' => "space",
                        '\n' => "\\n",
                        '\r' => "\\r",
                        '-' => "hyphen",
                        '–' => "en-dash",
                        '—' => "em-dash",
                        '…' => "ellipsis",
                        '“' => "left-quote",
                        '”' => "right-quote",
                        '‘' => "left-single-quote",
                        '’' => "right-single-quote",
                        _ => currentChar.ToString()
                    };

                    lock (_lockObject)
                    {
                        if (_characterFrequency.ContainsKey(displayChar))
                        {
                            _characterFrequency[displayChar]++;
                        }
                        else
                        {
                            _characterFrequency[displayChar] = 1;
                        }
                    }
                }
            }

            public void PrintDictionary()
            {
                foreach (var dict in _characterFrequency)
                    Console.WriteLine($"{dict.Key} - {dict.Value}");
            }
        }

        class WordFrequencyAnalyzerThread : CharacterFrequencyAnalyzerThread
        {
            private string[] _words;
            public WordFrequencyAnalyzerThread(string book, int treadCount)
                : base(book, treadCount)
            {
                _words = SplitTextIntoWords(book);
            }

            private string[] SplitTextIntoWords(string text)
            {
                var cleanedText = Regex.Replace(text, @"[^a-zA-Z0-9\s]", "");

                var words = Regex.Split(cleanedText, @"\s+");

                return Array.FindAll(words, word => !string.IsNullOrEmpty(word));
            }

            protected override void Process(object? threadNumber)
            {
                var num = (int)threadNumber;

                var itemsByThread = _words.Length / _threads.Length;

                var start = num * itemsByThread;

                var length = (num == _threads.Length - 1)
                    ? _words.Length - start
                    : itemsByThread;

                var span = _words.AsSpan(start, length);

                foreach (var word in span)
                {
                    lock (_lockObject)
                    {
                        if (_characterFrequency.ContainsKey(word))
                        {
                            _characterFrequency[word]++;
                        }
                        else
                        {
                            _characterFrequency[word] = 1;
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            var arr = new int[100_000_000];

            //var randomProc = new RandomThreadProcessor(arr, 8);
            //randomProc.Run();

            //-------------------------------------------------------

            //var getMinValue = new SearchMinNumberThread(arr, 128); // 128 - 00:00:00.2470732; 00:00:00.2292985; 00:00:00.2649346; 00:00:00.2509660; 00:00:00.2348639; 00:00:00.2248602;
            //var min = getMinValue.GetMinValue();

            //-------------------------------------------------------

            //var getMaxValue = new SearchMaxNumberThread(arr, 256); // 256 - 00:00:00.2465283; 00:00:00.2243551; 00:00:00.2291567; 00:00:00.2344024; 00:00:00.2436079; 00:00:00.2353001;
            //var max = getMaxValue.GetMaxValue();

            //-------------------------------------------------------

            //var getSumOfValues = new SearchSumAllElementsThread(arr, 256); // 256 - 00:00:00.2313988; 00:00:00.2524478; 00:00:00.2613115; 00:00:00.2304081; 00:00:00.2325839; 00:00:00.2391098;
            //var sum = getSumOfValues.GetSumOfValues();

            //-------------------------------------------------------

            //var getAverageValue = new SearchAverageValueThread(arr, 256); // 256 - 00:00:00.2699076; 00:00:00.2439301; 00:00:00.2388924; 00:00:00.2226464; 00:00:00.2150967; 00:00:00.2393904; 00:00:00.2373529;
            //var average = getAverageValue.GetAverageValue();

            //-------------------------------------------------------

            //var getHalfPartOfArray = new CopyHalfPartOfValuesThread(arr, 32, 50_000_000); // 32 - 00:00:00.3535007; 00:00:00.3519599; 00:00:00.3578338; 00:00:00.3572585;
            //var halfArray = getHalfPartOfArray.GetHalfPartOfValues();

            //-------------------------------------------------------

            //var characterFrequency = new CharacterFrequencyAnalyzerThread(book, 4); // 4 - 00:00:00.0032816; 00:00:00.0026837; 00:00:00.0030798; 00:00:00.0030414;
            //characterFrequency.Run();

            //-------------------------------------------------------

            //var wordsFrequency = new WordFrequencyAnalyzerThread(book, 4);  // 4 - 00:00:00.0128623; 00:00:00.0100595; 00:00:00.0105974;
            //wordsFrequency.Run();

            sw.Stop();
            Console.WriteLine(sw.Elapsed.ToString());

            //Console.WriteLine($"Min = {min}");
            //Console.WriteLine($"Max = {max}");
            //Console.WriteLine($"Sum = {sum}");
            //Console.WriteLine($"Average = {average}");
            //characterFrequency.PrintDictionary();
            //wordsFrequency.PrintDictionary();

        }

        const string book = "The Chronicles of Arlandia\r\n\r\nIn the heart of the vast kingdom of Arlandia, " +
            "nestled between towering mountains and lush forests, there existed a city known for its beauty and grandeur. " +
            "The capital, Eldoria, was a place where ancient traditions intertwined with the marvels of modern magic. " +
            "Stone buildings, decorated with ivy and glowing runes, lined the cobbled streets, " +
            "and towering spires stretched toward the sky as if in pursuit of the heavens themselves." +
            "\r\n\r\nThe citizens of Eldoria were as varied as the lands beyond the city's borders. " +
            "Merchants from the desert city of Kharim traded their exotic spices and silk; scholars from " +
            "the northern academy of Laris brought with them scrolls of forgotten lore; and travelers from the isles " +
            "of Verindar spoke of creatures unknown to the mainland. The diversity of people and cultures made Eldoria " +
            "not only the heart of Arlandia but the very soul of the world.\r\n\r\nHowever, beneath the surface of this " +
            "harmonious kingdom, shadows lingered. The King, Valen the Wise, had grown old and weary. His once-sharp mind, " +
            "which had guided the kingdom through years of prosperity, now faltered. Rumors whispered in the taverns and alleyways — " +
            "rumors of a dark force gathering strength beyond the borders, of a sorcerer exiled long ago who sought revenge, " +
            "and of a forgotten prophecy that foretold the fall of Arlandia.\r\n\r\nAmid this unrest, " +
            "a young knight named Elara found herself caught in the throes of fate. She was not born of noble blood, " +
            "but her courage and skill with a blade had earned her the King’s favor. Raised in a small village on the outskirts of the kingdom, " +
            "Elara had always dreamed of adventure, but she had never imagined that she would be called upon to save the very realm she had once only heard of in legends." +
            "\r\n\r\nOne fateful night, as a blood-red moon hung ominously in the sky, Elara was summoned to the King’s chambers. " +
            "The halls of the palace, usually bustling with courtiers and guards, were eerily silent. " +
            "Only the soft flicker of torchlight and the distant echo of her footsteps accompanied her as she approached the grand door." +
            "\r\n\r\nKing Valen, frail but still regal, sat on his throne. His silver hair fell loosely around his shoulders, " +
            "and his once-vibrant eyes now seemed dim with worry. Beside him stood the royal advisor, Alistair, " +
            "a tall man with piercing green eyes and a voice that could command armies.\r\n\r\n“Elara,” the King said softly, " +
            "his voice cracking with age, “I have called upon you because the time has come to fulfill your destiny.”" +
            "\r\n\r\nThe young knight knelt before the King, her heart pounding in her chest. “Your Majesty, " +
            "I am at your service.”\r\n\r\nValen motioned for her to rise. “Years ago, before you were even born, there was a war. " +
            "A war unlike any the world had ever seen. A sorcerer, Malakar, sought to claim the throne, not for power, but for destruction. " +
            "He believed that by plunging the world into chaos, he could reshape it in his image.”" +
            "\r\n\r\nElara’s hand instinctively moved to the hilt of her sword, her mind racing with the stories she had heard as a child. " +
            "Malakar — the name itself was enough to strike fear into the hearts of even the bravest of warriors.\r\n\r\n“He was defeated,” " +
            "Valen continued, “but not destroyed. He was exiled to the far reaches of the world, bound by powerful magic. " +
            "But now, the bindings are weakening. The signs are all around us — the red moon, the strange occurrences in the forests, " +
            "the whispers in the wind.”\r\n\r\nThe King’s voice trembled as he spoke the next words, “You are the key, Elara. " +
            "You are the one who must stop him.”\r\n\r\nElara’s breath caught in her throat. “Me? But how?”\r\n\r\nAlistair stepped forward, " +
            "his eyes never leaving hers. “You carry the blood of the ancient guardians, those who once protected the realm from forces like Malakar. " +
            "It is in your lineage, your very soul, to stand against him. But you will not stand alone. A group of warriors, scholars, " +
            "and magicians have been chosen to accompany you on this journey.”\r\n\r\nElara’s mind swirled with the weight of the task before her. " +
            "She had trained her whole life to serve the kingdom, but this… this was beyond anything she had ever imagined. “I will do it,” she said, " +
            "her voice steady despite the storm of emotions within her.\r\n\r\n“Then go,” Valen said, his eyes filled with both hope and sorrow. " +
            "“May the light of the guardians guide your path.”\r\n\r\nAnd so, Elara’s journey began. The road ahead was fraught with danger, " +
            "but she was not alone. Alongside her were companions as diverse and determined as the kingdom itself — warriors of unmatched strength, " +
            "magicians with the knowledge of the ancients, and scholars who held the key to unlocking the secrets of the past." +
            "\r\n\r\nTogether, they would face trials that tested their strength, courage, and loyalty. " +
            "They would encounter creatures born of nightmares, landscapes both beautiful and treacherous, " +
            "and enemies who sought to turn them against one another.\r\n\r\nBut through it all, Elara held onto one truth: " +
            "that the fate of Arlandia rested in her hands. And though the path was dark, " +
            "she knew that even the darkest of nights would eventually give way to the dawn.";
    }
}
