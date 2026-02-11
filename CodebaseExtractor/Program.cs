using System.Text;

internal class Program
{
    [STAThread]
    static void Main()
    {
        Console.WriteLine("Select .NET Project Folder");

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select your .NET project folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            Console.WriteLine("No folder selected.");
            return;
        }

        string selectedPath = dialog.SelectedPath;

        Console.WriteLine("\nExport Mode:");
        Console.WriteLine("1 - Preserve folder structure");
        Console.WriteLine("2 - Flat folder (All files in one folder)");
        Console.Write("Choose option (1 or 2): ");

        var choice = Console.ReadLine();

        bool flatMode = choice == "2";

        try
        {
            ExportCsFiles(selectedPath, flatMode);
            Console.WriteLine("\nExport completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }

    private static void ExportCsFiles(string rootPath, bool flatMode)
    {
        string outputRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            flatMode ? "CodebaseExport_Flat" : "CodebaseExport"
        );

        if (Directory.Exists(outputRoot))
            Directory.Delete(outputRoot, true);

        Directory.CreateDirectory(outputRoot);

        var csFiles = Directory
            .GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(IsValidSourceFile)
            .ToList();

        foreach (var sourceFile in csFiles)
        {
            string destinationPath;

            if (flatMode)
            {
                string relativePath = Path.GetRelativePath(rootPath, sourceFile);

                string safeFileName = relativePath
                    .Replace(Path.DirectorySeparatorChar, '_')
                    .Replace("/", "_");

                destinationPath = Path.Combine(outputRoot, safeFileName);
            }
            else
            {
                string relativePath = Path.GetRelativePath(rootPath, sourceFile);
                destinationPath = Path.Combine(outputRoot, relativePath);

                string? dir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir!);
            }

            string fileContent = File.ReadAllText(sourceFile);

            var sb = new StringBuilder();
            sb.AppendLine("/* =============================================");
            sb.AppendLine($"   FILE NAME : {Path.GetFileName(sourceFile)}");
            sb.AppendLine($"   PATH  : {sourceFile}");
            sb.AppendLine("   ============================================= */");
            sb.AppendLine();
            sb.AppendLine(fileContent);

            File.WriteAllText(destinationPath, sb.ToString(), Encoding.UTF8);
        }

        Console.WriteLine($"\nTotal exported files: {csFiles.Count}");
        Console.WriteLine($"Output location: {outputRoot}");
    }

    private static bool IsValidSourceFile(string path)
    {
        if (path.Contains(@"\bin\") || path.Contains(@"\obj\"))
            return false;

        if (path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            return false;

        if (path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            return false;

        if (path.EndsWith("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
