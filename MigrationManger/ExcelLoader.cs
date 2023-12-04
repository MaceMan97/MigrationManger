using MigrationManager;
using OfficeOpenXml;

namespace MigrationManger
{


    public class ExcelLoader
    {
        public static void SaveTableInfoToExcel(List<TableInfo> data, string filePath)
        {
            List<string> variableNames = new List<string>
        {
            "tableName",
            "schemaName",
            "databaseName",
            "alias",
            "newTableName",
            "newSchemaName",
            "newDatabaseName",
            "newAlias",
            "tokenStart",
            "tokenEnd"
        };

            if (variableNames == null || data == null || string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Invalid input data");
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("TableInfo");

                    for (int col = 1; col <= variableNames.Count; col++)
                    {
                        worksheet.Cells[1, col].Value = variableNames[col - 1];
                    }

                    for (int row = 2; row <= data.Count + 1; row++)
                    {
                        worksheet.Cells[row, 1].Value = data[row - 2].tableName;
                        worksheet.Cells[row, 2].Value = data[row - 2].schemaName;
                        worksheet.Cells[row, 3].Value = data[row - 2].databaseName;
                        worksheet.Cells[row, 4].Value = data[row - 2].alias;
                        worksheet.Cells[row, 5].Value = data[row - 2].newTableName;
                        worksheet.Cells[row, 6].Value = data[row - 2].newSchemaName;
                        worksheet.Cells[row, 7].Value = data[row - 2].newDatabaseName;
                        worksheet.Cells[row, 8].Value = data[row - 2].newAlias;
                        worksheet.Cells[row, 9].Value = data[row - 2].tokenStart;
                        worksheet.Cells[row, 10].Value = data[row - 2].tokenEnd;
                    }

                    package.SaveAs(new FileInfo(filePath));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while saving to the Excel file: {e.Message}");
            }
        }

        public static void LoadParamtersToExcel(List<ParameterInfo> data, string filePath)
        {

            List<string> variableNames = new List<string>
                {
                    "Name",
                    "Type",
                    "Default",
                    "NewName",
                    "NewType",
                    "NewDefault",
                    "IsToSplit",
                    "tokenStart",
                    "tokenEnd"
                };
            if (variableNames == null || data == null || string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Invalid input data");
            }
            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Create a new Excel package and add a worksheet
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Paramters");

                // Add column headers to the worksheet
                for (int col = 1; col <= variableNames.Count; col++)
                {
                    worksheet.Cells[1, col].Value = variableNames[col - 1];
                }

                
                // Add data rows to the worksheet
                for (int row = 2; row <= data.Count + 1; row++)
                {
                    worksheet.Cells[row, 1].Value = data[row-2].Name;
                    worksheet.Cells[row, 2].Value = data[row - 2].Type;
                    worksheet.Cells[row, 3].Value = data[row - 2].Default;
                    worksheet.Cells[row, 4].Value = data[row - 2].NewName;
                    worksheet.Cells[row, 5].Value = data[row - 2].NewType;
                    worksheet.Cells[row, 6].Value = data[row - 2].NewDefault;
                    worksheet.Cells[row, 7].Value = data[row - 2].IsToSplit;
                    worksheet.Cells[row, 8].Value = data[row - 2].tokenStart;
                    worksheet.Cells[row, 9].Value = data[row - 2].tokenEnd;
                    
                }

                // Save the Excel file to the specified path
                package.SaveAs(new FileInfo(filePath));
            }
        }

        public static void CreateIfNotExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }

            if (!File.Exists(filePath))
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Create a new Excel package and add a worksheet
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    // Optionally, you can add data or formatting to the worksheet here

                    // Save the Excel file to the specified path
                    package.SaveAs(new FileInfo(filePath));
                }
            }
        }

        public static List<ParameterInfo> ReadParametersFromExcel(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }

            List<ParameterInfo> parameterList = new List<ParameterInfo>();

            try
            {            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets["Paramters"]; // Make sure the worksheet name matches

                    if (worksheet != null)
                    {
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            ParameterInfo parameter = new ParameterInfo
                            {
                                Name = worksheet.Cells[row, 1].Text,
                                Type = worksheet.Cells[row, 2].Text,
                                Default = worksheet.Cells[row, 3].Text,
                                NewName = worksheet.Cells[row, 4].Text,
                                NewType = worksheet.Cells[row, 5].Text,
                                NewDefault = worksheet.Cells[row, 6].Text,
                                IsToSplit = bool.Parse(worksheet.Cells[row, 7].Text),
                                tokenStart = int.Parse(worksheet.Cells[row, 8].Text),
                                tokenEnd = int.Parse(worksheet.Cells[row, 9].Text)
                            };

                            parameterList.Add(parameter);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Worksheet 'Paramters' not found in the Excel file.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while reading from the Excel file: {e.Message}");
            }

            return parameterList;
        }

        public static void SaveIfStatementInfoToExcel(List<IfStatementInfo> data, string filePath)
        {
            List<string> variableNames = new List<string>
        {
            "Predicate",
            "ThenStatement",
            "ElseStatement",
            "tokenStart",
            "tokenEnd"
        };

            if (variableNames == null || data == null || string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Invalid input data");
            }

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("IfStatementInfo");

                    for (int col = 1; col <= variableNames.Count; col++)
                    {
                        worksheet.Cells[1, col].Value = variableNames[col - 1];
                    }

                    for (int row = 2; row <= data.Count + 1; row++)
                    {
                        worksheet.Cells[row, 1].Value = data[row - 2].Predicate;
                        worksheet.Cells[row, 2].Value = data[row - 2].ThenStatement;
                        worksheet.Cells[row, 3].Value = data[row - 2].ElseStatement;
                        worksheet.Cells[row, 4].Value = data[row - 2].tokenStart;
                        worksheet.Cells[row, 5].Value = data[row - 2].tokenEnd;
                    }

                    package.SaveAs(new FileInfo(filePath));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while saving to the Excel file: {e.Message}");
            }
        }
        public static List<TableInfo> ReadTableInfoFromExcel(string filePath)
        {
            List<string> variableNames = new List<string>
        {
            "tableName",
            "schemaName",
            "databaseName",
            "alias",
            "newTableName",
            "newSchemaName",
            "newDatabaseName",
            "newAlias",
            "tokenStart",
            "tokenEnd"
        };

        List<TableInfo> tableInfoList = new List<TableInfo>();

        try
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["TableInfo"]; // Make sure the worksheet name matches

                if (worksheet != null)
                {
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        TableInfo tableInfo = new TableInfo
                        {
                            tableName = worksheet.Cells[row, 1].Text,
                            schemaName = worksheet.Cells[row, 2].Text,
                            databaseName = worksheet.Cells[row, 3].Text,
                            alias = worksheet.Cells[row, 4].Text,
                            newTableName = worksheet.Cells[row, 5].Text,
                            newSchemaName = worksheet.Cells[row, 6].Text,
                            newDatabaseName = worksheet.Cells[row, 7].Text,
                            newAlias = worksheet.Cells[row, 8].Text,
                            tokenStart = int.Parse(worksheet.Cells[row, 9].Text),
                            tokenEnd = int.Parse(worksheet.Cells[row, 10].Text)
                        };

        tableInfoList.Add(tableInfo);
                    }
}
                else
{
    Console.WriteLine("Worksheet 'TableInfo' not found in the Excel file.");
}
            }
        }
        catch (Exception e)
{
    Console.WriteLine($"An error occurred while reading from the Excel file: {e.Message}");
}

return tableInfoList;
    }

    public static List<IfStatementInfo> ReadIfStatementInfoFromExcel(string filePath)
{
    List<string> variableNames = new List<string>
        {
            "Predicate",
            "ThenStatement",
            "ElseStatement",
            "tokenStart",
            "tokenEnd"
        };

    List<IfStatementInfo> ifStatementInfoList = new List<IfStatementInfo>();

    try
    {
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets["IfStatementInfo"]; // Make sure the worksheet name matches

            if (worksheet != null)
            {
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    IfStatementInfo ifStatementInfo = new IfStatementInfo
                    {
                        Predicate = worksheet.Cells[row, 1].Text,
                        ThenStatement = worksheet.Cells[row, 2].Text,
                        ElseStatement = worksheet.Cells[row, 3].Text,
                        tokenStart = int.Parse(worksheet.Cells[row, 4].Text),
                        tokenEnd = int.Parse(worksheet.Cells[row, 5].Text)
                    };

                    ifStatementInfoList.Add(ifStatementInfo);
                }
            }
            else
            {
                Console.WriteLine("Worksheet 'IfStatementInfo' not found in the Excel file.");
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"An error occurred while reading from the Excel file: {e.Message}");
    }

    return ifStatementInfoList;
}
    }

}
