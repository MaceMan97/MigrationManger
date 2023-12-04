using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace MigrationManager
{

    public struct TableInfo
    {
        public string? tableName { get; set; }
        public string? schemaName { get; set; }
        public string? databaseName { get; set; }
        public string? alias { get; set; }
        public string? newTableName { get; set; }
        public string? newSchemaName { get; set; }
        public string? newDatabaseName { get; set; }
        public string? newAlias { get; set; }
        public int tokenStart { get; set; }
        public int tokenEnd { get; set; }
        public TSqlParserToken[] tokens;

        public TableInfo(string tableName, string schemaName, string databaseName, string alias, int tokenStart, int tokenEnd, TSqlParserToken[] tokens)
        {
            this.tableName = tableName;
            this.schemaName = schemaName;
            this.databaseName = databaseName;
            this.alias = alias;
            this.newTableName = "";
            this.newSchemaName = "";
            this.newDatabaseName = "";
            this.newAlias = "";
            this.tokenStart = tokenStart;
            this.tokenEnd = tokenEnd;
            this.tokens = tokens;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj.GetType() == typeof(TableInfo))
            {
                var temp = (TableInfo)obj;
                bool isSame = false;
                if (this.tableName == temp.tableName &&
                    this.schemaName == temp.schemaName &&
                    this.databaseName == temp.databaseName)
                {
                    return true;
                }
                else
                {
                    return base.Equals(obj);
                }
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override string ToString()
        {
            string temp = "";
            foreach (var t in tokens)
            {
                temp += t.Text;
            }
            return temp;
        }
    }

    public struct IfStatementInfo
    {
        public string Predicate;
        public string ThenStatement;
        public string? ElseStatement;
        public int tokenStart;
        public int tokenEnd;
        public TSqlParserToken[] tokens;

        public IfStatementInfo(string Predicate, string ThenStatement, string ElseStatement, int tokenStart, int tokenEnd, TSqlParserToken[] tokens)
        {
            this.Predicate = Predicate;
            this.ThenStatement = ThenStatement;
            this.ElseStatement = ElseStatement;
            this.tokenStart = tokenStart;
            this.tokenEnd = tokenEnd;
            this.tokens = tokens;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj.GetType() == typeof(IfStatementInfo))
            {
                var temp = (IfStatementInfo)obj;
                bool isSame = false;
                if (this.Predicate == temp.Predicate &&
                    this.ThenStatement == temp.ThenStatement &&
                    this.ElseStatement == temp.ElseStatement)
                {
                    return true;
                }
                else
                {
                    return base.Equals(obj);
                }
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override string ToString()
        {
            string temp = "";
            foreach (var t in tokens)
            {
                temp += t.Text;
            }
            return temp;
        }
    }
        public struct ParameterInfo
        {
            public string Name;
            public string Type;
            public string? Default;
            public string NewName;
            public string NewType;
            public string? NewDefault;
            public bool IsToSplit;
            public int tokenStart;
            public int tokenEnd;
            public TSqlParserToken[] tokens;

            public ParameterInfo(string Name, string Type, string Default, int tokenStart, int tokenEnd, TSqlParserToken[] tokens)
            {
                this.Name = Name;
                this.Type = Type;
                this.Default = Default;
                this.tokenStart = tokenStart;
                this.tokenEnd = tokenEnd;
                this.tokens = tokens;
                this.NewName = "";
                this.NewType = "";
                this.NewDefault = "";
                this.IsToSplit = false;

            }

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj.GetType() == typeof(ParameterInfo))
                {
                    var temp = (ParameterInfo)obj;
                    bool isSame = false;
                    if (this.Name == temp.Name &&
                        this.Type == temp.Type &&
                        this.Default == temp.Default)
                    {
                        return true;
                    }
                    else
                    {
                        return base.Equals(obj);
                    }
                }
                else
                {
                    return base.Equals(obj);
                }
            }

            public override string ToString()
            {
                string temp = "";
                foreach (var t in tokens)
                {
                    temp += t.Text;
                }
                return temp;
            }

        }



        public class ProcedureParser : TSqlFragmentVisitor
        {

            //column identifiers keyed by FirstTokenIndex
            private Dictionary<int, Identifier> columnIdentifiers = new Dictionary<int, Identifier>();
            public List<CreateOrAlterProcedureStatement> Procedures = new List<CreateOrAlterProcedureStatement>();
            public List<IfStatement> IfStatements = new List<IfStatement>();
            public List<TSqlStatement> Statements = new List<TSqlStatement>();
            public List<UpdateStatement> UpdateStatements = new List<UpdateStatement>();
            public List<DeleteStatement> DeleteStatements = new List<DeleteStatement>();
            public List<InsertStatement> InsertStatements = new List<InsertStatement>();
            public List<NamedTableReference> Tables = new List<NamedTableReference>();
            public List<VariableReference> VariableStatements = new List<VariableReference>();
            public TSqlFragment tsqlScriptFragment;

            public List<TableInfo> TableInfo = new List<TableInfo>();
            public List<TableInfo> UniqueTables = new List<TableInfo>();

            public List<IfStatementInfo> IfStatement = new List<IfStatementInfo>();

            public List<ParameterInfo> Parameters = new List<ParameterInfo>();

            public void Parse(string sqlSelect)
            {

                var parser = new TSql150Parser(false);
                var rd = new StringReader(sqlSelect);
                IList<ParseError> errors;
                tsqlScriptFragment = parser.Parse(rd, out errors);
                if (errors.Count > 0)
                {
                    throw new ArgumentException($"Error(s) parsing SQL script. {errors.Count} errors found.");
                }

                tsqlScriptFragment.AcceptChildren(this);

            }

            //this is not used in this example but retained if you need it for other use cases        
            private string getNodeTokenText(TSqlFragment fragment)
            {
                StringBuilder tokenText = new StringBuilder();
                for (int counter = fragment.FirstTokenIndex; counter <= fragment.LastTokenIndex; counter++)
                {
                    tokenText.Append(fragment.ScriptTokenStream[counter].Text);
                }

                return tokenText.ToString();
            }

            //add identifiers in ColumnReferenceExpression to dictionary upon visit
            public override void Visit(ColumnReferenceExpression node)
            {

                foreach (var identifier in node.MultiPartIdentifier.Identifiers)
                {
                    this.columnIdentifiers.Add(identifier.FirstTokenIndex, identifier);
                }

            }

            //add identifiers in ColumnReferenceExpression to dictionary upon visit
            public override void Visit(UpdateStatement node)
            {
                this.UpdateStatements.Add(node);
            }


            //add identifiers in ColumnReferenceExpression to dictionary upon visit
            public override void Visit(NamedTableReference node)
            {
                //base.Visit(node);
                var table = new TableInfo(node.SchemaObject.BaseIdentifier?.Value, node.SchemaObject.SchemaIdentifier?.Value, node.SchemaObject.DatabaseIdentifier?.Value, node.Alias?.Value, node.SchemaObject.FirstTokenIndex, node.SchemaObject.LastTokenIndex, node.ScriptTokenStream.Skip(node.FirstTokenIndex).Take(node.LastTokenIndex - node.FirstTokenIndex + 1).ToArray());
                TableInfo.Add(table);
                if (!UniqueTables.Contains(table))
                {
                    UniqueTables.Add(table);
                }
            }

            //add identifiers in ColumnReferenceExpression to dictionary upon visit
            public override void Visit(CreateOrAlterProcedureStatement node)
            {
                this.Procedures.Add(node);
                foreach (var statement in node.Parameters)
                {
                    string value = "";
                    for (int i = statement.Value.FirstTokenIndex; i <= statement.Value.LastTokenIndex; ++i)
                    {
                        value += statement.Value.ScriptTokenStream[i].Text;
                    }

                    this.Parameters.Add(new ParameterInfo(statement.VariableName.Value, statement.DataType.Name.BaseIdentifier.Value, value, statement.FirstTokenIndex, statement.LastTokenIndex, statement.ScriptTokenStream.Skip(statement.FirstTokenIndex).Take(statement.LastTokenIndex - statement.FirstTokenIndex + 1).ToArray()));
                }

            }

            //add identifiers in ColumnReferenceExpression to dictionary upon visit
            public override void Visit(IfStatement node)
            {
                this.IfStatements.Add(node);

                string Predicate = "";
                for (int i = node.Predicate.FirstTokenIndex; i <= node.Predicate.LastTokenIndex; ++i)
                {
                    Predicate += node.Predicate.ScriptTokenStream[i].Text;
                }

                string Then = "";
                for (int i = node.ThenStatement.FirstTokenIndex; i <= node.ThenStatement.LastTokenIndex; ++i)
                {
                    Then += node.ThenStatement.ScriptTokenStream[i].Text;
                }

                string Else = "";
                if (node.ElseStatement != null)
                {

                    for (int i = node.ElseStatement.FirstTokenIndex; i <= node.ElseStatement.LastTokenIndex; ++i)
                    {
                        Else += node.ElseStatement.ScriptTokenStream[i].Text;
                    }
                }
                this.IfStatement.Add(new IfStatementInfo(Predicate, Then, Else, node.ThenStatement.FirstTokenIndex + 1, node.ThenStatement.LastTokenIndex - 2, node.ThenStatement.ScriptTokenStream.Skip(node.ThenStatement.FirstTokenIndex).Take(node.ThenStatement.LastTokenIndex - node.ThenStatement.FirstTokenIndex + 1).ToArray()));

            }
        
            //add identifiers in ColumnReferenceExpression to dictionary upon visit
            public override void Visit(VariableReference node)
            {

                this.VariableStatements.Add(node);

            }

            private string getTransformedSqlScript()
            {

                var transformedScript = new StringBuilder();

                for (int i = 0; i < tsqlScriptFragment.ScriptTokenStream.Count; ++i)
                {

                    if (columnIdentifiers.ContainsKey(i))
                    {
                        //replace square braket enclosures with single quotes, if needed
                        var columnIdentifier = columnIdentifiers[i];
                        var newcolumnIdentifier = columnIdentifier.QuoteType == QuoteType.SquareBracket ? $"'{columnIdentifier.Value}'" : columnIdentifier.Value;
                        transformedScript.Append(newcolumnIdentifier);
                    }
                    else
                    {
                        //keep original script text
                        transformedScript.Append(tsqlScriptFragment.ScriptTokenStream[i].Text);
                    }

                }

                return transformedScript.ToString();
            }

        }
    }

