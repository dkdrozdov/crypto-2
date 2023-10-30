using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace C_2
{
    public class Model
    {
        public static class Encoder
        {
            public static string Encode(string text, IEnumerable<Tuple<string, string>> symbolToCode)
            {
                StringBuilder result = new();
                char[] delimiters = { ',', ' ' };
                var symbolSequence = text.Where((x) => !delimiters.Contains(x)).Select((x) => $"{x}");
                foreach (var symbol in symbolSequence)
                {
                    var symbolEntry = symbolToCode.Where((x) => x.Item1 == symbol);
                    if (!symbolEntry.Any())
                        throw new ArgumentException(nameof(text),
                            "Последовательность содержит отсутствующие в алфавите символы.");
                    result.Append(symbolEntry.First().Item2 + " ");
                }
                return result.ToString();
            }
        };
        public static class Decoder
        {
            public static string Decode(string text, IEnumerable<Tuple<string, string>> symbolToCode)
            {
                StringBuilder result = new();
                char[] delimiters = { ',', ' ' };
                var codeSequence = text.Split(delimiters).Where((x) => x.Contains('1') || x.Contains('0'));
                foreach (var code in codeSequence)
                {
                    var codeEntry = symbolToCode.Where((x) => x.Item2 == code);
                    if (!codeEntry.Any())
                        throw new ArgumentException(nameof(text),
                            "Последовательность содержит отсутствующие в коде алфавита кодовые слова.");
                    result.Append(codeEntry.First().Item1 + " ");
                }
                return result.ToString();
            }
        };

        public static bool IsKraft(IEnumerable<string> codewords)
        {
            var v = codewords.Select((x) => x.Length);
            return v.Sum((l) => Math.Pow(2, -l)) <= 1;
        }
        public static double AverageCodeLength(IEnumerable<string> codewords)
        {
            var v = codewords.Select((x) => x.Length);
            return v.Sum((l) => l) / (double)v.Count();
        }
        public static double CalculateRedundancy(IEnumerable<decimal> probabilities, double avgCodeLength)
        {
            double entropy = 0;
            foreach (var p in probabilities)
                entropy -= (double)p * Math.Log2((double)p);
            return avgCodeLength - entropy;
        }

        public static List<Tuple<string, decimal, string>> CalculateWordsShannonFano(IEnumerable<Tuple<string, decimal>> symbolsProbabilities)
        {
            List<Tuple<string, decimal, string>> result = new();
            Queue<IEnumerable<Tuple<string, decimal, string>>> queue = new();

            // 0. Добавляем в очередь упорядоченный алфавит целиком
            queue.Enqueue(symbolsProbabilities.Select((x) =>
                Tuple.Create(x.Item1, x.Item2, "")).OrderByDescending((l) => l.Item2));

            while (queue.Any())
            {
                // 1. Выбираем группу символов
                var group = queue.Dequeue();
                // 1.1 Группу, в которой остался только один символ добавляем в результат
                if (group.Count() == 1)
                    result.Add(group.First());
                else
                {
                    decimal p = 0;
                    Queue<Tuple<string, decimal, string>> q = new(group);
                    List<Tuple<string, decimal, string>> group0, group1 = new();
                    // 2. Разбиваем на 2 примерно равные по вероятности группы
                    do
                    {
                        var e = q.Dequeue();
                        p += e.Item2;
                        // 3.а) К кодовым словам первой группы конкатенируем 1
                        group1.Add(Tuple.Create(e.Item1, e.Item2, e.Item3 + "1"));
                    }
                    while (p < .5m * group.Sum((x) => x.Item2));
                    // 3.б) К кодовым словам второй группы конкатенируем 0
                    group0 = q.Select((e) => Tuple.Create(e.Item1, e.Item2, e.Item3 + "0")).ToList();
                    // 4. Обе группы добавляем в очередь
                    queue.Enqueue(group1);
                    queue.Enqueue(group0);
                }
            }

            return result;
        }

        public static IEnumerable<Tuple<string, decimal, string>> GetWords(string alphabetFilePath, string probabilitiesFilePath)
        {
            char[] delimiters = { ',', ' ' };
            var eps = 0.001m;
            var alphabet = File.ReadAllText(alphabetFilePath).Split(delimiters);
            var probabilities = (File.ReadAllText(probabilitiesFilePath).Split(delimiters).
                Select((s) =>
                {
                    var i = Convert.ToDecimal(s);
                    if (i <= 0 || i > 1) throw new ArgumentOutOfRangeException(nameof(probabilitiesFilePath),
                        "Все вероятности должны быть больше 0 и не больше 1.");
                    return i;
                }));
            if (!(probabilities.Sum((x) => x) - 1 < eps)) throw new ArgumentOutOfRangeException(nameof(probabilitiesFilePath),
                        "Сумма вероятностей должна быть равна 1.");
            if (alphabet.Length != probabilities.Count()) throw new ArgumentOutOfRangeException(nameof(probabilitiesFilePath),
                         "Количество символов в алфавите и вероятностей не совпадает.");
            if (!alphabet.Any() || !probabilities.Any()) throw new ArgumentOutOfRangeException(nameof(probabilitiesFilePath),
                          "Алфавит пуст.");

            return new List<Tuple<string, decimal, string>>(CalculateWordsShannonFano(
                alphabet.Zip(probabilities, (symbol, probability) => Tuple.Create(symbol, probability))));
        }
    }
}
