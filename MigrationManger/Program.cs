// See https://aka.ms/new-console-template for more information

using MigrationManger;
using MigrationManager;
using MigrationManger;
using System;
using System.IO;
using static MigrationManager.IfStatementInfo;
using System.Xml;

class Program
{
    static void Main()
    {

        Console.Write("Enter the path to your SQL file: ");
        string filePath = Console.ReadLine();
        Console.Clear();
        
        Console.Write("What do you wish to do with this file?");
        Console.WriteLine();

        while (true)
        {
            CommandLineSelector commandLineSelector = new CommandLineSelector(new List<string> { "Export Analysis to Excel", "Import Modified Excel", "Process file for Synapse", "Exit" });
            int selectedOption = commandLineSelector.PrintOptions();

            switch (selectedOption)
            {
                case 1:
                    //ExportAnalysisToExcel(filePath);
                    Console.Clear();
                    Console.Write("File Exported...\n");
                    break;
                case 2:
                    //ImportModifiedExcel(filePath);
                    Console.Clear();
                    Console.Write("File Imported...\n");
                    break;
                case 3:
                    //ProcessFileForSynapse(filePath);
                    Console.Clear();
                    Console.Write("Files Processed...\n");
                    break;
                case 4:
                    Environment.Exit(0);
                    break;
            }
        }
    }

    public ProcedureParser ExportAnalysisToExcel(string filePath)
    {
        Console.Write("Enter the path to your excel file: ");
        string excelFileLoc = Console.ReadLine();
        string sqlText = "";
        try
        {
            sqlText = File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }

        ProcedureParser parser = new ProcedureParser();
        parser.Parse(sqlText);
        ExcelLoader.CreateIfNotExists(excelFileLoc);

        ExcelLoader.LoadParamtersToExcel(parser.Parameters, excelFileLoc);
        ExcelLoader.SaveIfStatementInfoToExcel(parser.IfStatement, excelFileLoc);
        ExcelLoader.SaveTableInfoToExcel(parser.TableInfo, excelFileLoc);

        return parser;
    }

    public void ImportModifiedExcel(string filePath, ProcedureParser? parser)
    {
        Console.Write("Enter the path to your excel file: ");
        string excelFileLoc = Console.ReadLine();
        string sqlText = "";
        if (parser == null)
        {
            parser = new ProcedureParser();
            
            try
            {
                sqlText = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            parser.Parse(sqlText);

        }
        parser.IfStatement = ExcelLoader.ReadIfStatementInfoFromExcel(excelFileLoc);
        parser.TableInfo = ExcelLoader.ReadTableInfoFromExcel(excelFileLoc);
        parser.Parameters = ExcelLoader.ReadParametersFromExcel(excelFileLoc);
    }

    public void ProcessFileForSynapse(string filePath, ProcedureParser? parser)
    {
        string sqlText = "";
        if (parser == null)
        {
            parser = new ProcedureParser();

            try
            {
                sqlText = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            parser.Parse(sqlText);

        }
        ProcedureConverter converter = new ProcedureConverter(parser);
        var scripts = converter.splitScriptByParameter();
        string transformedScript = converter.transformedScript.ToString();
        foreach (var s in scripts)
        {
            Console.WriteLine(s);
        }

        Console.WriteLine(transformedScript);
    }
}


