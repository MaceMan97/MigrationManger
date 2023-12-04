
using Microsoft.Extensions.Primitives;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using static MigrationManager.IfStatementInfo;

namespace MigrationManager
{
    public class ProcedureConverter
    {
        private ProcedureParser parser = new ProcedureParser();
        private TSqlFragment tsqlScriptFragment;
        public StringBuilder transformedScript = new StringBuilder();
        public ProcedureConverter(ProcedureParser parser)
        {
            this.parser = parser;
            tsqlScriptFragment = parser.tsqlScriptFragment;
        }

        
        public string getTransformedSqlScript()
        {

            var transformedScript = new StringBuilder();
            
            for (int i = 0; i < tsqlScriptFragment.ScriptTokenStream.Count; ++i)
            {
                bool istrue = false;
                foreach (var t in parser.Tables)
                {
                    istrue = SqlManager.AvoidUsedTokens(t, i);
                }
                if (!istrue)
                {
                    //keep original script text
                    transformedScript.Append(tsqlScriptFragment.ScriptTokenStream[i].Text);
                }
            }

            return transformedScript.ToString();
        }

        public List<string> splitScriptByParameter()
        {
            List<string> scripts = new List<string>();

            string parameterExec = "";
            string parameterExecTemp = "";

            string parameterDecl = "";
            string parameterDeclTemp = "";

            string tableIdentifier = "";
            string tableIdentifierTemp = "";
            for (int i = 0; i < tsqlScriptFragment.ScriptTokenStream.Count; ++i)
            {
                bool istrue = false;

                foreach (var p in parser.Parameters)
                {

                    if (i >= p.tokenStart && i <= p.tokenEnd)
                    {
                        istrue = true;
                        parameterDecl = generateParamterDecl(p);
                    }
                    else
                    {
                        foreach (var s in parser.IfStatement.Where(x => x.Predicate.Contains(p.Name + " = 1")))
                        {
                            if (i >= s.tokenStart && i <= s.tokenEnd)
                            {
                                istrue = true;
                                if (!scripts.Contains(generateCreateProc(p, s)))
                                {
                                    scripts.Add(generateCreateProc(p, s));
                                    parameterExec = generateExecProc(p, s);
                                }
                            }
                        }
                    }
                }

                foreach (var t in parser.TableInfo)
                {
                    
                    if (i >= t.tokenStart && i <= t.tokenEnd)
                    {
                        istrue = true;
                        tableIdentifier = generateModifiedTableName(t);
                    }
                }

                if (!istrue)
                {
                    //keep original script text
                    transformedScript.Append(tsqlScriptFragment.ScriptTokenStream[i].Text);
                }
                else
                {
                    if (parameterDecl != parameterDeclTemp) 
                    {
                        transformedScript.Append(parameterDecl);
                        parameterDeclTemp = parameterDecl;
                    }

                    if (parameterExecTemp != parameterExec)
                    {
                        transformedScript.Append(parameterExec);
                        parameterExecTemp = parameterExec;
                    }

                    if (tableIdentifier != tableIdentifierTemp)
                    {
                        transformedScript.Append(tableIdentifier);
                        tableIdentifierTemp = tableIdentifier;
                    }
                }

            }


            return scripts;
        }

        private string generateCreateProc(ParameterInfo parameter, IfStatementInfo statement)
        {
            string createProc = string.Format("create procedure {0}_{1}\n as \n{3}", parser.Procedures[0].ProcedureReference.Name.SchemaIdentifier.Value + "." + parser.Procedures[0].ProcedureReference.Name.BaseIdentifier.Value, (parameter.NewName != "") ? parameter.NewName : parameter.Name.Substring(1), (parameter.NewType != "") ? parameter.NewType : parameter.Type, statement.ThenStatement);
            return createProc;

        }

        private string generateExecProc(ParameterInfo parameter, IfStatementInfo statement)
        {
            string createProc = string.Format("\nexec {0}_{1};\n", parser.Procedures[0].ProcedureReference.Name.SchemaIdentifier.Value + "." + parser.Procedures[0].ProcedureReference.Name.BaseIdentifier.Value, (parameter.NewName != "") ? parameter.NewName : parameter.Name.Substring(1));
            return createProc;

        }

        private string generateParamterDecl(ParameterInfo parameter)
        {
            return string.Format("{0} {1}", (parameter.NewName != "") ? parameter.NewName : parameter.Name.Substring(1), (parameter.NewType != "") ? parameter.NewType : parameter.Type);
        }

        private string generateModifiedTableName(TableInfo parameter)
        {
            string table = "";
            string database = "";
            string schema = "";
            
            if (parameter.newTableName != "")
            {
                table = parameter.newTableName;
                if (parameter.newSchemaName != "")
                {
                    schema = parameter.newSchemaName;
                    if (parameter.newDatabaseName != "")
                    {
                        database = parameter.newDatabaseName;
                        return string.Format("{0}.{1}.{2}", database, schema, table);
                    }
                    else
                    {
                        return string.Format("{0}.{1}", schema, table);
                    }
                }
                else
                {
                    return table;
                }
            }
            else
            {
                return parameter.ToString();

            }

        }
    }


}

