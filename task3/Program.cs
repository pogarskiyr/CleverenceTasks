/*В словесном описании задания формат даты DD-MM-YYYY (день-месяц-год) - считаю это верным
  В примерах выходных данных дата "2025-03-10" - считаю это опечаткой

  Первый параметр программы - имя выходного лог файл
  Второй и последующие - имя входных лог файлов
  Имя файла с проблемными строками "problems.txt" - зашито в коде
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

internal class Program
{

    readonly public struct DesiredLogFormat
    {
        public enum LevelsLog
        {
            INFO,
            WARN,
            ERROR,
            DEBUG
        }

        private static readonly string separator = "\t";
        readonly DateOnly  date;
        readonly TimeOnly time;
        readonly string timeOnString;
        readonly LevelsLog levelLog;
        readonly string callingMethod;
        readonly string message;

        //The function attempts to parse a log of the first format. If unsuccessful, return false;
        static private bool ParseFormatFirst(ref DateOnly date, ref TimeOnly time, ref string timeOnString, ref LevelsLog levelLog, ref string callingMethod, ref string message, string LogString) {

            string[] parts = LogString.Split(new[] { ' ', '\t', '\u00A0' }, 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return false;
            for (int i = 0; i < 3; i++) parts[i] = parts[i].Trim();

            if (!DateOnly.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return false;
            }

            if (!TimeOnly.TryParseExact(parts[1], "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                return false;
            }
            timeOnString = parts[1];

            switch (parts[2].ToUpper())
            {
                case "INFORMATION":
                case "INFO":
                    levelLog = LevelsLog.INFO;
                    break;
                case "WARNING":
                case "WARN":
                    levelLog = LevelsLog.WARN;
                    break;
                case "ERROR":
                    levelLog = LevelsLog.ERROR;
                    break;
                case "DEBUG":
                    levelLog = LevelsLog.DEBUG;
                    break;
                default:
                    return false;
            }

            callingMethod = "DEFAULT";

            if (parts.Length == 4) message = parts[3];
            else message = "";

            return true;
        }

        //The function attempts to parse a log of the swcond format. If unsuccessful, return false;
        static private bool ParseFormatSecond(ref DateOnly date, ref TimeOnly time, ref string timeOnString, ref LevelsLog levelLog, ref string callingMethod, ref string message, string LogString)
        {
            string[] parts = LogString.Split(new[] { ' ', '\t', '|', '\u00A0' }, 6, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return false;
            for (int i = 0; i < 5; i++) parts[i] = parts[i].Trim();

            if (!DateOnly.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return false;
            }

            if (!TimeOnly.TryParseExact(parts[1], "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                return false;
            }
            timeOnString = parts[1];

            switch (parts[2].ToUpper())
            {
                case "INFORMATION":
                case "INFO":
                    levelLog = LevelsLog.INFO;
                    break;
                case "WARNING":
                case "WARN":
                    levelLog = LevelsLog.WARN;
                    break;
                case "ERROR":
                    levelLog = LevelsLog.ERROR;
                    break;
                case "DEBUG":
                    levelLog = LevelsLog.DEBUG;
                    break;
                default:
                    return false;
            }

            //parts[3] unused?

            callingMethod = parts[4].Length == 0 ? "DEFAULT": parts[4];

            if (parts.Length == 6) message = parts[5];
            else message = "";

            return true;
        }

        public DesiredLogFormat(string LogString)
        {
            DateOnly date = DateOnly.MinValue; TimeOnly time = TimeOnly.MinValue; string timeOnString = ""; LevelsLog levelLog = LevelsLog.INFO; string callingMethod = ""; string message = "";
            if (ParseFormatFirst(ref date, ref time, ref timeOnString, ref levelLog, ref callingMethod, ref message, LogString)) { }
            else if (ParseFormatSecond(ref date, ref time, ref timeOnString, ref levelLog, ref callingMethod, ref message, LogString)) { }
            else {
                throw new ArgumentException(nameof(LogString), "Log format is not supported.");
            }

            this.date = date;
            this.time = time;
            this.timeOnString = timeOnString;
            this.levelLog = levelLog;
            this.callingMethod = callingMethod;
            this.message = message;
        }


        public override readonly string ToString()
        {
            string dateString = date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

            return $"{dateString}{separator}{timeOnString}{separator}{levelLog}{separator}{callingMethod}{separator}{message}";
        }

        public static bool operator <(DesiredLogFormat left, DesiredLogFormat right)
        {
            if (left.date != right.date)
            {
                return left.date < right.date;
            }
            return left.time < right.time;
        }

        public static bool operator >(DesiredLogFormat left, DesiredLogFormat right)
        {
            if (left.date != right.date)
            {
                return left.date > right.date;
            }
            return left.time > right.time;
        }
    }

    static void MergeLogs(string problemsFile, string outFile, string[] inFiles) 
    {
        using var problemsWriter = new StreamWriter(problemsFile, append: false);
        using var outWriter = new StreamWriter(outFile, append: false);

        var logsReaders = new List<StreamReader>();
        var logs = new List<DesiredLogFormat>();
        string? _tmp;

        foreach (string file in inFiles)
        {
            var stream = new StreamReader(file);
            if (stream != null) logsReaders.Add(stream);

        }

        for (int i = 0; i < logsReaders.Count; i++)
        {
            StreamReader reader = logsReaders[i];
        
            while (true)
            {
                _tmp = reader.ReadLine();

                if (_tmp == null)
                {
                    reader.Close();
                    logsReaders.Remove(reader);
                    i--;
                    break;
                }

                if (string.IsNullOrWhiteSpace(_tmp)) continue;

                try
                {
                    DesiredLogFormat log = new DesiredLogFormat(_tmp);
                    logs.Add(log);
                    break;
                }
                catch
                {
                    problemsWriter.WriteLine(_tmp);
                    continue;
                }
            }
        }

        while (logsReaders.Count > 0)
        {
            int minIndex = 0;
            for (int i = 1; i < logs.Count; i++)
            {
                if (logs[i] < logs[minIndex]) minIndex = i; 
            }

            outWriter.WriteLine(logs[minIndex]);

            logs.RemoveAt(minIndex);
            while (true)
            {
                _tmp = logsReaders[minIndex].ReadLine();

                if (_tmp == null)
                {
                    logsReaders[minIndex].Close();
                    logsReaders.Remove(logsReaders[minIndex]);
                    break;
                }

                if (string.IsNullOrWhiteSpace(_tmp)) continue;

                try
                {
                    DesiredLogFormat log = new DesiredLogFormat(_tmp);
                    logs.Insert(minIndex, log);
                    break;
                }
                catch
                {
                    problemsWriter.WriteLine(_tmp);
                    continue;
                }
            }
        }        
    }

    private static void Main(string[] Logs)
    {
        const string problemFile = "problems.txt";

        try
        {
            if (Logs.Length > 1) MergeLogs(problemFile, Logs[0], Logs[1..]);
        }
        catch
        {
            Console.WriteLine("Failed to open file");
        }
    }
}
