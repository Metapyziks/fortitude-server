using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FortitudeServer.Entities
{
#if LINUX
    using DBConnection = Mono.Data.Sqlite.SqliteConnection;
    using DBCommand = Mono.Data.Sqlite.SqliteCommand;
    using DBDataReader = Mono.Data.Sqlite.SqliteDataReader;
#else
    using DBConnection = System.Data.SqlServerCe.SqlCeConnection;
    using DBCommand = System.Data.SqlServerCe.SqlCeCommand;
    using DBDataReader = System.Data.SqlServerCe.SqlCeDataReader;
    using DBEngine = System.Data.SqlServerCe.SqlCeEngine;
#endif

    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseEntityAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : ColumnAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : NotNullAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : ColumnAttribute
    {
        public readonly Type ForeignEntityType;

        public ForeignKeyAttribute(Type foreignEntityType)
        {
            ForeignEntityType = foreignEntityType;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : UniqueAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementAttribute : ColumnAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class FixedLengthAttribute : ColumnAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class CapacityAttribute : ColumnAttribute
    {
        public readonly int Value;
        public readonly int Value2;

        public CapacityAttribute(int value, int value2 = 0)
        {
            Value = value;
            Value2 = value2;
        }
    }

    public class DatabaseColumn
    {
        private readonly PropertyInfo _property;

        public String Name { get { return _property.Name; } }
        public Type Type { get { return _property.PropertyType; } }

        public bool NotNull { get; private set; }
        public bool Unique { get; private set; }
        public bool ForeignKey { get; private set; }
        public bool PrimaryKey { get; private set; }
        public bool AutoIncrement { get; private set; }
        public bool FixedLength { get; private set; }

        public DatabaseTable[] ForeignTables { get; private set; }

        public int Capacity { get; private set; }
        public int Capacity2 { get; private set; }

        public DatabaseColumn(PropertyInfo property)
        {
            _property = property;

            NotNull = property.IsDefined<NotNullAttribute>();
            Unique = property.IsDefined<UniqueAttribute>();
            ForeignKey = property.IsDefined<ForeignKeyAttribute>();
            PrimaryKey = property.IsDefined<PrimaryKeyAttribute>();
            AutoIncrement = property.IsDefined<AutoIncrementAttribute>();
            FixedLength = property.IsDefined<FixedLengthAttribute>();
            
            if (property.IsDefined<CapacityAttribute>()) {
                CapacityAttribute val = property.GetCustomAttribute<CapacityAttribute>();
                Capacity = val.Value;
                Capacity2 = val.Value2;
            } else {
                Capacity = 0;
                Capacity2 = 0;
            }
        }

        internal void ResolveForeignKeys()
        {
            if (ForeignKey) {
                ForeignTables = (
                    from attrib in _property.GetCustomAttributes(typeof(ForeignKeyAttribute), false)
                    select DatabaseManager.GetTable(((ForeignKeyAttribute)attrib).ForeignEntityType)
                ).ToArray();
            }
        }

        private static String GetSQLTypeName(DatabaseColumn col, Type type)
        {
            if (type.IsEnum)
                return GetSQLTypeName(col, Enum.GetUnderlyingType(type));

            if (type == typeof(String) || type == typeof(Char[])) {
                String name = col.FixedLength ? "NCHAR({0})" : "NVARCHAR({0})";
#if LINUX
                name += "COLLATE NOCASE";
#endif
                return name;
            }

            if (type == typeof(Int64) || type == typeof(DateTime))
                return "BIGINT";

            if (type == typeof(Int32))
                return "INTEGER";

            if (type == typeof(Int16))
                return "SMALLINT";

            if (type == typeof(Byte))
                return "TINYINT";

            if (type == typeof(Double) || type == typeof(Single))
                return "DECIMAL({0},{1})";

            if (type == typeof(Boolean))
                return "BOOLEAN";

            throw new Exception("Can't find the SQL type of " + type.FullName);
        }

        public String GenerateDefinitionStatement()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("{0} {1}", Name, String.Format(
                GetSQLTypeName(this, Type), Capacity, Capacity2));

            if (PrimaryKey)
                builder.Append(" PRIMARY KEY");
            else if (Unique)
                builder.Append(" UNIQUE");
            else if (NotNull)
                builder.Append(" NOT NULL");

            if (AutoIncrement)
#if LINUX
                builder.Append( " AUTOINCREMENT" );
#else
                builder.Append(" IDENTITY");
#endif

            return builder.ToString();
        }

        public object GetValue(object entity)
        {
            object val = _property.GetValue(entity, null);
            if (val is DateTime)
                return ((DateTime) val).Ticks;
            else if (val.GetType().IsEnum)
                return Convert.ChangeType(val, Enum.GetUnderlyingType(val.GetType()));
            else if (val is char[])
                return new String((char[]) val);
            else
                return _property.GetValue(entity, null);
        }

        public void SetValue(object entity, object val)
        {
            if (_property.PropertyType == typeof(DateTime))
                _property.SetValue(entity, new DateTime(Convert.ToInt64(val)), null);
            else if (_property.PropertyType.IsEnum)
                _property.SetValue(entity, Convert.ChangeType(val,
                    Enum.GetUnderlyingType(_property.PropertyType)), null);
            else if (_property.PropertyType == typeof(char[]))
                _property.SetValue(entity, Convert.ToString(val).ToCharArray(), null);
            else
                _property.SetValue(entity, Convert.ChangeType(val, _property.PropertyType), null);
        }

        public override string ToString()
        {
            return GenerateDefinitionStatement();
        }
    }

    public class DatabaseTable
    {
        private readonly Type _type;

        public Type Type { get { return _type; } }
        public String Name { get { return _type.Name; } }

        public DatabaseTable SuperTable { get; private set; }
        public DatabaseColumn[] Columns { get; private set; }

        public DatabaseTable(Type type)
        {
            _type = type;
        }

        private bool ShouldInclude(PropertyInfo property)
        {
            if (!property.IsDefined<ColumnAttribute>()) return false;

            if (property.IsDefined<PrimaryKeyAttribute>()) return true;

            Type super = _type.BaseType;
            while (super.IsDefined<DatabaseEntityAttribute>()) {
                if (super.GetProperty(property.Name) != null) {
                    return false;
                }
                super = super.BaseType;
            }

            return true;
        }

        internal void BuildColumns()
        {
            int count = _type.GetProperties().Count(x => ShouldInclude(x));
            Columns = new DatabaseColumn[count];

            int i = 0;
            foreach (PropertyInfo property in _type.GetProperties()) {
                if (ShouldInclude(property)) {
                    Columns[i++] = new DatabaseColumn(property);
                }
            }
        }

        internal void ResolveForeignKeys()
        {
            if (_type.BaseType.IsDefined<DatabaseEntityAttribute>()) {
                SuperTable = DatabaseManager.GetTable(_type.BaseType);
            }

            foreach (var col in Columns) {
                col.ResolveForeignKeys();
            }
        }

        public String GenerateDefinitionStatement()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("CREATE TABLE {0}\n(\n", Name);
            for (int i = 0; i < Columns.Length; ++i) {
                builder.AppendFormat("  {0}{1}\n", Columns[i].GenerateDefinitionStatement(),
                    i < Columns.Length - 1 ? "," : "");
            }
            builder.AppendFormat(");\n");
            return builder.ToString();
        }

        public void Drop()
        {
            Console.WriteLine("  Dropping table {0}...", Name);
            DatabaseManager.ExecuteNonQuery("DROP TABLE {0}", Name);
        }

        public void Create()
        {
            Console.WriteLine("  Creating table {0}...", Name);
            DatabaseManager.ExecuteNonQuery(GenerateDefinitionStatement());
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class DatabaseManager
    {
        public static CultureInfo CultureInfo = new CultureInfo("en-US");

        public static String FileName = "Database.db";
        private static DBConnection _sConnection;

        private static List<DatabaseTable> _sTables = new List<DatabaseTable>();

        public static void Connect(String connStrFormat, params String[] args)
        {
            if (_sConnection != null)
                Disconnect();

            Console.WriteLine("Establishing database connection...");
            String connectionString = String.Format(connStrFormat, args);
            _sConnection = new DBConnection(connectionString);
            _sConnection.Open();

            Type[] types;

            try {
                types = Assembly.GetExecutingAssembly().GetTypes();
            } catch (ReflectionTypeLoadException e) {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var ex in e.LoaderExceptions) {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(ex.StackTrace);
                }
                Console.ResetColor();

                throw;
            }

            foreach (Type type in types) {
                if (type.IsDefined<DatabaseEntityAttribute>()) {
                    DatabaseTable table = CreateTable(type);
                    table.BuildColumns();
                    Console.WriteLine("- Initialized table {0}", table.Name);
                }
            }
            
            foreach (var table in _sTables) {
                table.ResolveForeignKeys();
                if (!TableExists(table)) {
                    table.Create();
                }
            }

        }

        public static void ConnectLocal()
        {
//#if DEBUG
//            DropDatabase();
//#endif

            if (!File.Exists(FileName))
                CreateDatabase("Data Source={0};", FileName);
            else
                Connect("Data Source={0};", FileName);
        }

        public static void DropDatabase()
        {
            if (_sConnection != null)
                Disconnect();

            if (File.Exists(FileName))
                File.Delete(FileName);
        }

        private static void CreateDatabase(String connStrFormat, params String[] args)
        {
#if !LINUX
            DBEngine engine = new DBEngine(String.Format(connStrFormat, args));
            engine.CreateDatabase();
            engine.Dispose();
#endif
            Connect(connStrFormat, args);
        }

        private static DatabaseTable CreateTable<T>()
            where T : new()
        {
            return CreateTable(typeof(T));
        }

        private static DatabaseTable CreateTable(Type type)
        {
            DatabaseTable newTable = new DatabaseTable(type);
            _sTables.Add(newTable);
            return newTable;
        }

        private static bool RequiresParam(Expression exp)
        {
            if (exp is UnaryExpression)
                return RequiresParam(((UnaryExpression) exp).Operand);

            if (exp is BinaryExpression) {
                BinaryExpression bExp = (BinaryExpression) exp;
                return RequiresParam(bExp.Left) || RequiresParam(bExp.Right);
            }

            if (exp is MemberExpression) {
                MemberExpression mExp = (MemberExpression) exp;
                return RequiresParam(mExp.Expression);
            }

            if (exp is MethodCallExpression) {
                MethodCallExpression mcExp = (MethodCallExpression) exp;
                return mcExp.Arguments.Any(x => RequiresParam(x));
            }

            if (exp is ParameterExpression)
                return true;

            if (exp is ConstantExpression)
                return false;

            throw new Exception("Cannot reduce an expression of type " + exp.GetType());
        }

        private static String SerializeExpression(Expression exp, bool removeParam = false)
        {
            if (!RequiresParam(exp)) {
                if (exp.Type == typeof(bool)) {
                    Expression<Func<bool,String>> toString = x => x ? "1'='1" : "1'='0";
                    return String.Format("'{0}'", Expression.Lambda<Func<String>>(
                        Expression.Invoke(toString, exp)).Compile()());
                } else if (exp.Type == typeof(int)) {
                    Expression<Func<int,String>> toString = x => x.ToString();
                    return String.Format("'{0}'", Expression.Lambda<Func<String>>(
                        Expression.Invoke(toString, exp)).Compile()());
                } else if (exp.Type == typeof(double)) {
                    Expression<Func<double,String>> toString = x => x.ToString();
                    return String.Format("'{0}'", Expression.Lambda<Func<String>>(
                        Expression.Invoke(toString, exp)).Compile()());
                } else
                    return String.Format("'{0}'", Expression.Lambda<Func<Object>>(exp).Compile()());
            }

            if (exp is UnaryExpression) {
                UnaryExpression uExp = (UnaryExpression) exp;
                String oper = SerializeExpression(uExp.Operand, removeParam);

                switch (exp.NodeType) {
                    case ExpressionType.Not:
                        return String.Format("(NOT {0})", oper);
                    case ExpressionType.Convert:
                        return SerializeExpression(uExp.Operand, removeParam);
                    default:
                        throw new Exception("Cannot convert an expression of type "
                            + exp.NodeType + " to SQL");
                }
            }

            if (exp is BinaryExpression) {
                BinaryExpression bExp = (BinaryExpression) exp;
                String left = SerializeExpression(bExp.Left, removeParam);
                String right = SerializeExpression(bExp.Right, removeParam);
                switch (exp.NodeType) {
                    case ExpressionType.Equal:
                        return String.Format("({0} = {1})", left, right);
                    case ExpressionType.NotEqual:
                        return String.Format("({0} != {1})", left, right);
                    case ExpressionType.LessThan:
                        return String.Format("({0} < {1})", left, right);
                    case ExpressionType.LessThanOrEqual:
                        return String.Format("({0} <= {1})", left, right);
                    case ExpressionType.GreaterThan:
                        return String.Format("({0} > {1})", left, right);
                    case ExpressionType.GreaterThanOrEqual:
                        return String.Format("({0} >= {1})", left, right);
                    case ExpressionType.AndAlso:
                        return String.Format("({0} AND {1})", left, right);
                    case ExpressionType.OrElse:
                        return String.Format("({0} OR {1})", left, right);
                    default:
                        throw new Exception("Cannot convert an expression of type "
                            + exp.NodeType + " to SQL");
                }
            }

            switch (exp.NodeType) {
                case ExpressionType.Parameter:
                    ParameterExpression pExp = (ParameterExpression) exp;
                    return pExp.Name;
                case ExpressionType.Constant:
                    ConstantExpression cExp = (ConstantExpression) exp;
                    return String.Format("'{0}'", cExp.Value.ToString());
                case ExpressionType.MemberAccess:
                    MemberExpression mExp = (MemberExpression) exp;
                    if (removeParam && mExp.Expression is ParameterExpression)
                        return mExp.Member.Name;
                    return String.Format("{0}.{1}", SerializeExpression(
                            mExp.Expression, removeParam),
                        mExp.Member.Name);
                default:
                    throw new Exception("Cannot convert an expression of type "
                        + exp.NodeType + " to SQL");
            }
        }

        public static int ExecuteNonQuery(String format, params Object[] args)
        {
            var cmd = new DBCommand(String.Format(format, args), _sConnection);
#if DEBUG
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(cmd.CommandText);
            Console.ResetColor();
#endif
            return cmd.ExecuteNonQuery();
        }

        public static DatabaseTable GetTable<T>()
        {
            return GetTable(typeof(T));
        }

        public static DatabaseTable GetTable(Type t)
        {
            return _sTables.FirstOrDefault(x => x.Type == t);
        }

        public static DatabaseTable[] GetTables()
        {
            return _sTables.ToArray();
        }

        public static bool TableExists<T>()
        {
            return TableExists(GetTable(typeof(T)));
        }

        public static bool TableExists(Type t)
        {
            return TableExists(GetTable(t));
        }

        public static bool TableExists(DatabaseTable table)
        {
#if LINUX
            String statement = String.Format("SELECT * FROM sqlite_master " +
                "WHERE type = 'table' AND name = '{0}'", table.Name);
#else
            String statement = String.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_NAME = '{0}'", table.Name);
#endif
            DBCommand cmd = new DBCommand(statement, _sConnection);
            using (var reader = cmd.ExecuteReader()) {
                return reader.Read();
            }
        }

        private static DBCommand GenerateSelectCommand<T>(DatabaseTable table, params Expression<Func<T, bool>>[] predicates)
            where T : new()
        {
            for (int i = 1; i < predicates.Length; ++i)
                if (predicates[i].Parameters[0].Name != predicates[0].Parameters[0].Name)
                    throw new Exception("All predicates must use the same parameter name");

            String alias = predicates[0].Parameters[0].Name;

            String columns = String.Join(",\n  ", table.Columns.Select(x => alias + "." + x.Name));

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT\n  {0}\nFROM {1} AS {2}\n", columns,
                table.Name, alias);

            builder.AppendFormat("WHERE {0}", String.Join("\n  OR ",
                predicates.Select(x => SerializeExpression(x.Body))));

#if DEBUG
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(builder.ToString());
            Console.ResetColor();
#endif

            return new DBCommand(builder.ToString(), _sConnection);
        }

        private static DBCommand GenerateSelectCommand<T0, T1>(DatabaseTable table0,
            DatabaseTable table1, params Expression<Func<T0, T1, bool>>[] predicates)
            where T0 : new()
            where T1 : new()
        {
            for (int i = 1; i < predicates.Length; ++i) {
                if (predicates[i].Parameters[0].Name != predicates[0].Parameters[0].Name
                    || predicates[i].Parameters[1].Name != predicates[0].Parameters[1].Name) {
                    throw new Exception("All predicates must use the same parameter name");
                }
            }

            var alias0 = predicates[0].Parameters[0].Name;
            var alias1 = predicates[0].Parameters[1].Name;

            var columns = String.Join(",\n  ", table0.Columns.Select(x => alias0 + "." + x.Name))
                + ",\n  " + String.Join(",\n  ", table1.Columns.Select(x => alias1 + "." + x.Name));

            var joinOn = String.Format("{0}.{1} = {2}.{3}", alias0,
                table0.Columns.First(x => x.PrimaryKey).Name, alias1,
                table1.Columns.First(x => x.ForeignTables != null && x.ForeignTables.Contains(table0)).Name);

            var builder = new StringBuilder();
            builder.AppendFormat("SELECT\n  {0}\nFROM {1} AS {2}\nINNER JOIN {3} AS {4}\nON {5}\n", columns,
                table0.Name, alias0, table1.Name, alias1, joinOn);

            builder.AppendFormat("WHERE {0}", String.Join("\n  OR ",
                predicates.Select(x => SerializeExpression(x.Body))));

#if DEBUG
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(builder.ToString());
            Console.ResetColor();
#endif
            return new DBCommand(builder.ToString(), _sConnection);
        }

        private static T ReadEntity<T>(this DBDataReader reader, DatabaseTable table)
            where T : new()
        {
            T entity = default(T);

            if (reader.Read()) {
                entity = new T();

                foreach (DatabaseColumn col in table.Columns)
                    col.SetValue(entity, reader[col.Name]);
            }

            return entity;
        }

        private static Tuple<T0, T1> ReadEntity<T0, T1>(this DBDataReader reader, DatabaseTable table0, DatabaseTable table1)
            where T0 : new()
            where T1 : new()
        {
            Tuple<T0, T1> entity = null;

            if (reader.Read()) {
                entity = new Tuple<T0, T1>(new T0(), new T1());

                foreach (DatabaseColumn col in table0.Columns)
                    col.SetValue(entity.Item1, reader[col.Name]);

                foreach (DatabaseColumn col in table1.Columns)
                    col.SetValue(entity.Item2, reader[col.Name]);
            }

            return entity;
        }

        /// <summary>
        /// Returns the first item from the specified table that matches
        /// all given predicates, or null if no items do.
        /// </summary>
        /// <typeparam name="T">Entity type of the table to select from</typeparam>
        /// <param name="predicates">Predicates for the selected item to match</param>
        /// <returns>The first item that matches all given predicates, or null</returns>
        public static T SelectFirst<T>(params Expression<Func<T, bool>>[] predicates)
            where T : new()
        {
            DatabaseTable table = GetTable<T>();
            DBCommand cmd = GenerateSelectCommand(table, predicates);

            T entity;
            using (DBDataReader reader = cmd.ExecuteReader())
                entity = reader.ReadEntity<T>(table);

            return entity;
        }

        /// <summary>
        /// Returns the first pair of items from the cartesian product of two
        /// tables that matches all given predicates, or null if no pairs do.
        /// </summary>
        /// <typeparam name="T0">Entity type of the first table to select from</typeparam>
        /// <typeparam name="T1">Entity type of the second table to select from</typeparam>
        /// <param name="predicates">Predicates for the </param>
        /// <returns></returns>
        public static Tuple<T0, T1> SelectFirst<T0, T1>(params Expression<Func<T0, T1, bool>>[] predicates)
            where T0 : new()
            where T1 : new()
        {
            var table0 = GetTable<T0>();
            var table1 = GetTable<T1>();
            var cmd = GenerateSelectCommand(table0, table1, predicates);

            Tuple<T0, T1> entity;
            using (DBDataReader reader = cmd.ExecuteReader())
                entity = reader.ReadEntity<T0, T1>(table0, table1);

            return entity;
        }

        public static List<T> Select<T>(params Expression<Func<T, bool>>[] predicates)
            where T : new()
        {
            DatabaseTable table = GetTable<T>();
            DBCommand cmd = GenerateSelectCommand(table, predicates);

            List<T> entities = new List<T>();
            using (DBDataReader reader = cmd.ExecuteReader()) {
                T entity;
                while ((entity = reader.ReadEntity<T>(table)) != null)
                    entities.Add(entity);
            }

            return entities;
        }

        public static List<Tuple<T0, T1>> Select<T0, T1>(params Expression<Func<T0, T1, bool>>[] predicates)
            where T0 : new()
            where T1 : new()
        {
            var table0 = GetTable<T0>();
            var table1 = GetTable<T1>();
            var cmd = GenerateSelectCommand(table0, table1, predicates);

            var entities = new List<Tuple<T0, T1>>();
            using (DBDataReader reader = cmd.ExecuteReader()) {
                Tuple<T0, T1> entity;
                while ((entity = reader.ReadEntity<T0, T1>(table0, table1)) != null)
                    entities.Add(entity);
            }

            return entities;
        }

        public static List<T> SelectAll<T>()
            where T : new()
        {
            return Select<T>(x => true);
        }

        public static List<Tuple<T0, T1>> SelectAll<T0, T1>()
            where T0 : new()
            where T1 : new()
        {
            return Select<T0, T1>((x, y) => true);
        }

        public static int Insert<T>(T entity)
            where T : new()
        {
            return Insert(GetTable<T>(), entity);
        }

        private static int Insert(DatabaseTable table, Object entity)
        {
            if (table.SuperTable != null) Insert(table.SuperTable, entity);
            
            IEnumerable<DatabaseColumn> valid = table.Columns.Where(x => !x.AutoIncrement);

            String columns = String.Join(",\n  ", valid.Select(x => x.Name));
            String values = String.Join("',\n  '", valid.Select(x => x.GetValue(entity)));

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("INSERT INTO {0}\n(\n  {1}\n) VALUES (\n  '{2}'\n)",
                table.Name, columns, values);

            return new DBCommand(builder.ToString(), _sConnection).ExecuteNonQuery();
        }

        public static int Update<T>(T entity)
            where T : new()
        {
            return Update(GetTable<T>(), entity);
        }

        private static int Update(DatabaseTable table, Object entity)
        {
            if (table.SuperTable != null) Update(table.SuperTable, entity);

            DatabaseColumn primaryKey = table.Columns.First(x => x.PrimaryKey);

            IEnumerable<DatabaseColumn> valid = table.Columns.Where(x => x != primaryKey);

            String columns = String.Join(",\n  ", valid.Select(x =>
                String.Format("{0} = '{1}'", x.Name, x.GetValue(entity))));

            String predicate = String.Format("{0}='{1}'", primaryKey.Name, primaryKey.GetValue(entity));

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("UPDATE {0} SET\n  {1}\nWHERE {2}",
                table.Name, columns, predicate);

            return new DBCommand(builder.ToString(), _sConnection).ExecuteNonQuery();
        }

        // TODO: Delete should cascade with supertables
        public static int Delete<T>(T entity)
            where T : new()
        {
            return Delete<T>(new T[] { entity });
        }

        public static int Delete<T>(IEnumerable<T> entities)
            where T : new()
        {
            DatabaseTable table = GetTable<T>();
            DatabaseColumn primaryKey = table.Columns.First(x => x.PrimaryKey);

            String predicate = String.Join("\n  OR ", entities.Select(x => String.Format("{0}='{1}'",
                primaryKey.Name, primaryKey.GetValue(x))));

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("DELETE FROM {0} WHERE {1}",
                table.Name, predicate);

            return new DBCommand(builder.ToString(), _sConnection).ExecuteNonQuery();
        }

        public static int Delete<T>(params Expression<Func<T, bool>>[] predicates)
        {
            DatabaseTable table = GetTable<T>();

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("DELETE FROM {0} ", table.Name);

            builder.AppendFormat("WHERE {0}", String.Join("\n  OR ",
                predicates.Select(x => SerializeExpression(x.Body, true))));

            return new DBCommand(builder.ToString(), _sConnection).ExecuteNonQuery();
        }

        public static void Disconnect()
        {
            _sConnection.Close();
            _sConnection = null;

            _sTables.Clear();
        }
    }
}
