using System;
using System.IO;
using System.Text;
using System.Linq;

class Program
{
    private const string OUTPUT_DIR = "../mysql/init";
    private const int TOTAL_RECORDS = 40_000_000;
    private const int RECORDS_PER_FILE = 1_000_000;
    private const int RECORDS_PER_INSERT = 10_000;

    private static readonly string[] FIRST_NAMES = { "John", "Jane", "Michael", "Sarah", "David", "Emma", "James", "Emily", "William", "Olivia" };
    private static readonly string[] LAST_NAMES = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

    private static readonly DateTime START_DATE = new DateTime(1950, 1, 1);
    private static readonly int DATE_RANGE = (new DateTime(2005, 12, 31) - START_DATE).Days;
    private static readonly Random random = new Random();

    static void Main()
    {
        Console.WriteLine("Starting data generation...");

        // Ensure directory exists
        Directory.CreateDirectory(OUTPUT_DIR);

        int totalFiles = (TOTAL_RECORDS + RECORDS_PER_FILE - 1) / RECORDS_PER_FILE;
        int currentRecord = 0;

        for (int fileNum = 0; fileNum < totalFiles; fileNum++)
        {
            string fileName = Path.Combine(OUTPUT_DIR, $"02-data-users-part{fileNum + 1:D2}.sql");
            int recordsThisFile = Math.Min(RECORDS_PER_FILE, TOTAL_RECORDS - currentRecord);

            using (var writer = new StreamWriter(fileName, false, new UTF8Encoding(false)))
            {
                writer.NewLine = "\n";  // Force LF line endings

                // Write header for each file
                writer.WriteLine("SET autocommit=0;");

                var sb = new StringBuilder(RECORDS_PER_INSERT * 100);
                int insertsInFile = (recordsThisFile + RECORDS_PER_INSERT - 1) / RECORDS_PER_INSERT;

                for (int insertBlock = 0; insertBlock < insertsInFile; insertBlock++)
                {
                    sb.Clear();
                    sb.AppendLine("INSERT INTO app_users (date_of_birth, name) VALUES");

                    int recordsThisBlock = Math.Min(RECORDS_PER_INSERT, 
                        recordsThisFile - (insertBlock * RECORDS_PER_INSERT));

                    for (int i = 0; i < recordsThisBlock; i++)
                    {
                        if (i > 0) sb.Append(",\n");

                        var date = START_DATE.AddDays(random.Next(DATE_RANGE)).ToString("yyyy-MM-dd");
                        var firstName = FIRST_NAMES[random.Next(FIRST_NAMES.Length)];
                        var lastName = LAST_NAMES[random.Next(LAST_NAMES.Length)];

                        sb.Append($"('{date}', '{firstName} {lastName}')");
                    }
                    sb.AppendLine(";");

                    if ((insertBlock + 1) % 10 == 0)
                    {
                        sb.AppendLine("COMMIT;");
                        sb.AppendLine("SET autocommit=0;");
                    }

                    writer.Write(sb.ToString());
                }

                // Write footer for each file
                writer.WriteLine("COMMIT;");
                writer.WriteLine("SET autocommit=1;");
            }

            currentRecord += recordsThisFile;
            Console.WriteLine($"Generated file {fileNum + 1}/{totalFiles} ({currentRecord:N0}/{TOTAL_RECORDS:N0} records)");
        }

        Console.WriteLine("Data generation completed!");
    }
} 