using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SolidLab3
{
    public class ImportedRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public override string ToString() => $"[ID: {Id}, Name: {Name}, Value: {Value}]";
    }

    public class ValidationReport
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }

    public class ProcessingSummary
    {
        public int Total { get; set; }
        public int Success { get; set; }
        public List<string> Details { get; set; } = new List<string>();
    }


    public interface IDataImporter // Принцип OCP/LSP
    {
        IEnumerable<ImportedRecord> Import(string filePath);
    }

    public interface IDataValidator // Принцип SRP
    {
        ValidationReport Validate(ImportedRecord record);
    }

    public interface IOutputService // Принцип ISP/DIP
    {
        void ShowMessage(string message);
        void ShowSummary(ProcessingSummary summary);
    }

    public class CsvDataImporter : IDataImporter
    {
        public IEnumerable<ImportedRecord> Import(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            var lines = File.ReadAllLines(filePath);
            return lines.Skip(1)
                .Select(line => line.Split(';'))
                .Where(p => p.Length >= 3)
                .Select(p => new ImportedRecord { Id = p[0], Name = p[1], Value = p[2] });
        }
    }
    public class JsonDataImporter : IDataImporter
    {
        public IEnumerable<ImportedRecord> Import(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<ImportedRecord>>(jsonString);
        }
    }

    public class DataValidator : IDataValidator
    {
        public ValidationReport Validate(ImportedRecord record)
        {
            if (!int.TryParse(record.Id, out _))
                return new ValidationReport { IsValid = false, Message = "ID не является числом" };

            if (!double.TryParse(record.Value, out double val) || val <= 0)
                return new ValidationReport { IsValid = false, Message = "Значение должно быть > 0" };

            return new ValidationReport { IsValid = true };
        }
    }

    public class ConsoleOutputService : IOutputService
    {
        public void ShowMessage(string message) => Console.WriteLine(message);
        public void ShowSummary(ProcessingSummary s)
        {
            Console.WriteLine("\n--- ОТЧЕТ ---");
            Console.WriteLine($"Всего: {s.Total}, Успешно: {s.Success}, Ошибок: {s.Total - s.Success}");
            s.Details.ForEach(d => Console.WriteLine(d));
        }
    }

    public class DataManager
    {
        private readonly IDataImporter _importer;
        private readonly IDataValidator _validator;
        private readonly IOutputService _output;

        // Зависимости передаются через конструктор (DIP)
        public DataManager(IDataImporter importer, IDataValidator validator, IOutputService output)
        {
            _importer = importer;
            _validator = validator;
            _output = output;
        }

        public void ProcessFile(string path)
        {
            _output.ShowMessage($"\nНачало обработки: {path}");
            var summary = new ProcessingSummary();

            try
            {
                var records = _importer.Import(path);
                foreach (var record in records)
                {
                    summary.Total++;
                    var report = _validator.Validate(record);

                    if (report.IsValid)
                    {
                        summary.Success++;
                        _output.ShowMessage($"[OK] {record}");
                    }
                    else
                    {
                        summary.Details.Add($"Ошибка в записи {record.Name}: {report.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.ShowMessage($"[Критическая ошибка]: {ex.Message}");
            }

            _output.ShowSummary(summary);
        }
    }

    class Program
    {
        static void Main()
        {
            var validator = new DataValidator();
            var output = new ConsoleOutputService();

            var csvManager = new DataManager(new CsvDataImporter(), validator, output);
            csvManager.ProcessFile("data.csv");

            var jsonManager = new DataManager(new JsonDataImporter(), validator, output);
            jsonManager.ProcessFile("data.json");
        }
    }
}