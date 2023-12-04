using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace MigrationManager
{
    public static class SqlManager
    {

        public static List<NamedTableReference> GetUniqueTables(List<NamedTableReference> tables)
        {
            var uniqueTables = new List<NamedTableReference>();

            foreach (NamedTableReference table in tables)
            {

                if (!uniqueTables.Where(x => SqlManager.GetTableFullName(x) == SqlManager.GetTableFullName(table)).Any())
                {
                    uniqueTables.Add(table);
                }
            }
            return uniqueTables;
        }

        public static List<NamedTableReference> PropagateUniqueTablesToStandard(List<NamedTableReference> tables, List<NamedTableReference> uniqueTables)
        {

            foreach (NamedTableReference table in uniqueTables)
            {
                var temp = tables.Where(x => x.SchemaObject.BaseIdentifier.Value == table.SchemaObject.BaseIdentifier.Value && x.SchemaObject.SchemaIdentifier.Value == table.SchemaObject.SchemaIdentifier.Value);
                if (temp.Any())
                {
                    foreach(var t in temp)
                    {
                        tables[tables.IndexOf(t)] = table;
                    }
                }

            }

            return tables;
        }

        public static string GetTableFullName(NamedTableReference table)
        {
            if (table.SchemaObject.BaseIdentifier != null)
            {
                if(table.SchemaObject.SchemaIdentifier != null)
                {
                    if(table.SchemaObject.DatabaseIdentifier != null)
                    {
                        return string.Format("{0}.{1}.{2}", table.SchemaObject.DatabaseIdentifier.Value, table.SchemaObject.SchemaIdentifier.Value, table.SchemaObject.BaseIdentifier.Value);
                    }
                    else
                    {
                        return string.Format("{0}.{1}", table.SchemaObject.SchemaIdentifier.Value, table.SchemaObject.BaseIdentifier.Value);
                    }
                }
                else
                {
                    return string.Format("{0}", table.SchemaObject.BaseIdentifier.Value);
                }
            }
            else
            {
                return "";
            }
        }

        public static int GetTableReferenceCount(NamedTableReference uniqueTable, List<NamedTableReference> tables)
        {
            return tables.Where(x => SqlManager.GetTableFullName(x) == SqlManager.GetTableFullName(uniqueTable)).Count();
        }

        public static string GetText(TSqlFragment statement)
        {
            string str = "";
            for (int i = statement.FirstTokenIndex; i <= statement.LastTokenIndex; i++)
            {
                str += statement.ScriptTokenStream[i].Text;
            }

            return str;
        }

        public static bool AvoidUsedTokens(TSqlFragment statement, int i)
        {
            if (statement.FirstTokenIndex >= i && i <= statement.LastTokenIndex )
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
