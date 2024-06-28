using System.Security.Cryptography;

namespace VeeaamTestTask
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: FolderSync <sourceFolder> <replicaFolder> <intervalSeconds> <logFilePath>");
                return;
            }

            string sourceFolder = args[0];
            string replicaFolder = args[1];
            int intervalSeconds = int.Parse(args[2]);
            string logFilePath = args[3];

            if (Path.HasExtension(logFilePath))
            {
                try
                {
                    using (File.Create(logFilePath)) { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating log file: {ex.Message}");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Invalid log file path. Please specify a valid file path.");
                return;
            }

            while (true)
            {
                SyncFolders(sourceFolder, replicaFolder, logFilePath);
                Thread.Sleep(intervalSeconds * 1000);
            }
        }

        static void SyncFolders(string source, string replica, string logFilePath)
        {
            try
            {
                Log($"Synchronization started at {DateTime.Now}", logFilePath);

                var sourceFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                var replicaFiles = Directory.GetFiles(replica, "*", SearchOption.AllDirectories);

                foreach (var filePath in sourceFiles)
                {
                    string relativePath = filePath.Substring(source.Length + 1);
                    string replicaFilePath = Path.Combine(replica, relativePath);

                    if (!File.Exists(replicaFilePath) || !FilesAreEqual(filePath, replicaFilePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(replicaFilePath));
                        File.Copy(filePath, replicaFilePath, true);
                        Log($"Copied: {filePath} to {replicaFilePath}", logFilePath);
                    }
                }

                foreach (var filePath in replicaFiles)
                {
                    string relativePath = filePath.Substring(replica.Length + 1);
                    string sourceFilePath = Path.Combine(source, relativePath);

                    if (!File.Exists(sourceFilePath))
                    {
                        File.Delete(filePath);
                        Log($"Deleted: {filePath}", logFilePath);
                    }
                }

                Log($"Synchronization completed at {DateTime.Now}", logFilePath);
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}", logFilePath);
            }
        }

        static bool FilesAreEqual(string filePath1, string filePath2)
        {
            using (var hashAlgorithm = MD5.Create())
            {
                byte[] file1Hash = hashAlgorithm.ComputeHash(File.ReadAllBytes(filePath1));
                byte[] file2Hash = hashAlgorithm.ComputeHash(File.ReadAllBytes(filePath2));
                return file1Hash.SequenceEqual(file2Hash);
            }
        }

        static void Log(string message, string logFilePath)
        {
            try
            {
                Console.WriteLine(message);
                string logDirectory = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message. Error: {ex.Message}");
            }
        }
    }
}