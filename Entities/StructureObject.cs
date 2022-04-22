using Archimedes.Cells;
using Archimedes.Entities;
using Archimedes.Enums;
using Archimedes.Utilities;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes
{
    /// <summary>
    /// Object representing a given structure lua and information regarding it and it's author.
    /// </summary>
    public sealed class StructureObject : ICloneable
    {
        Script scriptObj = null;

        bool useProcessor => scriptObj?.Globals["ProcessRow"] is not null;

        string selectStatement { get; set; }


        /// <summary>
        /// Path to the structure file set during this objects construction
        /// </summary>
        public readonly string FilePath;

        /// <summary>
        /// Name (without extension) of the actual structure lua this object represents
        /// </summary>
        public string StructName => Path.GetFileNameWithoutExtension(FilePath);

        /// <summary>
        /// Name of this structure object
        /// </summary>
        public string Name;

        /// <summary>
        /// Overridable default filename for load/save operations (includes extension)
        /// </summary>
        public string RDBName;

        /// <summary>
        /// Overridable database name (will default to Arcadia)
        /// </summary>
        public string DatabaseName;

        /// <summary>
        /// Overridable database table name (will be requested if undefined)
        /// </summary>
        public string TableName;

        /// <summary>
        /// Version of this structure object
        /// </summary>
        public Version Version = new Version(0, 0, 0, 0);

        /// <summary>
        /// Author of this structure object
        /// </summary>
        public string Author;

        /// <summary>
        /// Supported epics of this structure object
        /// </summary>
        public float[] Epic;

        /// <summary>
        /// Encoding which strings will be encoded/decoded
        /// </summary>
        public Encoding Encoding
        {
            get => ByteUtility.Encoding;
            set => ByteUtility.Encoding = value;
        }

        /// <summary>
        /// Any special case the engine may need to consider
        /// </summary>
        public CaseFlag SpecialCase;

        /// <summary>
        /// If traditional, standard header read/write will be used.
        /// </summary>
        public HeaderType HeaderType { get; set; } = HeaderType.Traditional;

        /// <summary>
        /// Collection of cells that describe the header section of an rdb
        /// </summary>
        public List<CellBase> HeaderCells = new List<CellBase>();

        /// <summary>
        /// Collection of cells describing the data of an rdb
        /// </summary>
        public List<CellBase> DataCells = new List<CellBase>();

        /// <summary>
        /// Collection of cell names that are not hidden
        /// </summary>
        public string[] VisibleCellNames
        {
            get
            {
                List<string> names = new List<string>();

                foreach (CellBase cell in DataCells)
                    if (!cell.Flags.HasFlag(CellFlags.Hidden))
                        names.Add(cell.Name);

                return names.ToArray();
            }
        }

        /// <summary>
        /// Row object containing header data
        /// </summary>
        public RowObject Header = null;

        /// <summary>
        /// Collection of row objects storing schema and data
        /// </summary>
        public List<RowObject> Rows = new List<RowObject>();

        /// <summary>
        /// The amount of rows loaded into this structure object
        /// </summary>
        public int RowCount => (int)Header[CellFlags.RowCount];

        /// <summary>
        /// SQL Select statement to be used when loading data for this structure object from a SQL database
        /// </summary>
        public string SelectStatement
        {
            get
            {
                if (selectStatement != null)
                    return selectStatement;

                if (TableName == null)
                    throw new NullReferenceException("TableName is null!");

                string statement = "SELECT ";

                foreach (CellBase cell in DataCells)
                {
                    if (cell.Flags.HasFlag(CellFlags.SqlIgnore))
                        continue;

                    statement += $"[{cell.Name}],";
                }

                return selectStatement = $"{statement.Remove(statement.Length - 1, 1)} FROM dbo.{TableName} with (NOLOCK)";
            }
            set => selectStatement = value;
        }

        /// <summary>
        /// Construct this structure object with user input via a structure lua
        /// </summary>
        /// <param name="filename">Path to the structure lua to be parsed</param>
        /// <param name="parse">Defaults to true, if false the script will not be parsed only constructed.</param>
        public StructureObject(string filename, bool parse = true)
        {
            this.FilePath = filename;

            if (filename == null || !File.Exists(filename))
                throw new FileNotFoundException("Cannot find the structure file!", filename);

            if (parse)
                ParseScript();

            // We need to tell our ByteUtility what encoding the user declared (if any)
            ByteUtility.Encoding = Encoding;
        }

        /// <summary>
        /// Create a new Header row object from previously loaded HeaderCells
        /// </summary>
        public void SetHeader() => Header = new RowObject(HeaderCells);

        /// <summary>
        /// Create a new header row object based on the provided info and set it as this structure objects header.
        /// </summary>
        /// <param name="date">The date as a DateString</param>
        /// <param name="signature">120 byte array (which can contain strings)</param>
        /// <param name="rowCount">Amount of rows contained in this structure object</param>
        public void SetHeader(DateString date, byte[] signature, int rowCount)
        {
            List<CellBase> cells = new List<CellBase>();

            cells.Add(new DateStringCell("CreationDate") { Index = 0 });
            cells.Add(new ByteArrayCell("Signature", 120) { Index = 1 });
            cells.Add(new IntCell("RowCount", CellFlags.RowCount) { Index = 2 });

            Header = new RowObject(cells);
            Header[0] = date;
            Header[1] = signature;
            Header[2] = rowCount;

        }

        /// <summary>
        /// Parse the contents of the provided structure lua into information usable by this structure object
        /// </summary>
        /// <param name="flags">Bit vector containing combination of read flags</param>
        public void ParseScript(ParseFlags flags = ParseFlags.Both)
        {
            string scriptStr = File.ReadAllText(FilePath);

            if (string.IsNullOrEmpty(scriptStr))
                throw new NullReferenceException("Failed to read the structure lua contents!");

            scriptObj = new Script();

            string curDir = Directory.GetCurrentDirectory();

            // Enable - `require 'ModuleName'` statements
            ((ScriptLoaderBase)scriptObj.Options.ScriptLoader).ModulePaths = new string[]
            {
                $"{curDir}\\Modules\\?",
                $"{curDir}\\Modules\\?.lua",
                $"{curDir}\\Enums\\?.lua"
            };

            // declare our globals with Moonsharp interpreter
            declareGlobals(scriptObj);

            scriptObj.DoString(scriptStr);

            Dictionary<string, int> enumDict = new Dictionary<string, int>();

            // If the info flag isnt present, proceed to processing the header and data cells
            if (!flags.HasFlag(ParseFlags.Info))
                goto structure;

            Name = scriptObj.Globals["name"] as string ?? "UNDEFINED";
            Version = new Version(scriptObj.Globals["version"] as string ?? "0.0.0.0");
            Author = scriptObj.Globals["author"] as string ?? "UNDEFINED";
            RDBName = scriptObj.Globals["file_name"] as string;
            DatabaseName = scriptObj.Globals["database"] as string ?? "Arcadia";
            TableName = scriptObj.Globals["table_name"] as string ?? "UNDEFINED";
            SelectStatement = scriptObj.Globals["select_statement"] as string ?? $"SELECT * FROM dbo.{TableName}";

            if (scriptObj.Globals["encoding"] != null)
            {
                int codepage = Convert.ToInt32(scriptObj.Globals["encoding"]);

                Encoding = Encoding.GetEncoding(codepage);
            }

            Table epicTbl = scriptObj.Globals["epic"] as Table;

            if (epicTbl == null)
                Epic = new float[1] { 0.0f };
            else
            {
                Epic = new float[epicTbl.Length];

                for (int i = 1; i < epicTbl.Length + 1; i++)
                    Epic[i - 1] = Convert.ToSingle(epicTbl.Get(i).Number);
            }

            SpecialCase = (scriptObj.Globals["special_case"] != null) ? (CaseFlag)Convert.ToInt32(scriptObj.Globals["special_case"]) : CaseFlag.None;

        structure:

            if (!flags.HasFlag(ParseFlags.Structure))
                return;

            Table headerTbl = scriptObj.Globals["header"] as Table;

            if (headerTbl == null)
                SetHeader(new DateString("19900101"), new byte[120], 0);
            else // In the case of .ref file headers
            {
                HeaderType = HeaderType.Defined;
                HeaderCells = generateCells(headerTbl);

                SetHeader();
            }

            Table dataTable = scriptObj.Globals["cells"] as Table;

            if (dataTable == null)
                throw new NullReferenceException("Fields table is null!");

            DataCells = generateCells(dataTable);
        }

        /// <summary>
        /// Init the global variables the provided script object will require to process any provided structure lua
        /// </summary>
        /// <param name="scriptObject"><c>Script</c> object that will be loading a structure lua</param>
        void declareGlobals(Script scriptObject)
        {
            #region Type Globals

            string[] arcTypeNames = Enum.GetNames(typeof(ArcType));
            int[] values = (int[])Enum.GetValues(typeof(ArcType));

            for (int i = 0; i < 31; i++)
                scriptObject.Globals[arcTypeNames[i].Remove(0, 5)] = values[i];

            #endregion

            #region Direction Globals

            scriptObject.Globals["READ"] = "read";
            scriptObject.Globals["WRITE"] = "write";

            #endregion

            #region Special Case Globals

            // These are the ordinal position of the bits
            scriptObject.Globals["HIDDEN"] = "1:1";
            scriptObject.Globals["ROWCOUNT"] = "2:2";
            scriptObject.Globals["RDBIGNORE"] = "3:4";
            scriptObject.Globals["SQLIGNORE"] = "4:8";
            scriptObject.Globals["LOOPCOUNTER"] = "5:16";

            // SpecialCase globals
            scriptObject.Globals["DOUBLELOOP"] = 1;

            #endregion

            #region Flag Type Globals

            scriptObject.Globals["BIT_FLAG"] = 3;
            scriptObject.Globals["ENUM"] = 4;

            #endregion

        }

        /// <summary>
        /// Generate derived BaseCell objects describing rdb data from user defined structure lua
        /// </summary>
        /// <param name="cells">Fields lua table</param>
        /// <returns>Populated list of boxed CellBase objects</returns>
        List<CellBase> generateCells(Table cells)
        {
            List<CellBase> objCollection = new List<CellBase>();

            for (int i = 1; i < cells.Length + 1; i++)
            {
                Table objData = cells.Get(i).Table;

                if (objData == null)
                    throw new NullReferenceException("Failed to get the object properties table!");

                CellBase dataObj = null;

                // Set the Secondary and Primary type for the new data object
                ArcType secondaryType = objData.Get(2).ToObject<ArcType>();
                Type objType = GetObjectType(secondaryType);

                string name = objData.Get(1).String;

                // Create the object in its derived state (we will box it as cellect for transport)
                if (objType == typeof(IntCell))
                {
                    dataObj = new IntCell(name);

                    if (secondaryType == ArcType.TYPE_COPY_INT32)
                        dataObj.Dependency = objData.Get(3).String;
                }
                else if (objType == typeof(EncodedIntCell))
                {
                    dataObj = new EncodedIntCell(name);
                }
                else if (objType == typeof(ShortCell))
                {
                    dataObj = new ShortCell(name);
                }
                else if (objType == typeof(LongCell))
                {
                    dataObj = new LongCell(name);
                }
                else if (objType == typeof(FloatCell))
                {
                    dataObj = new FloatCell(name);
                }
                else if (objType == typeof(DoubleCell))
                {
                    dataObj = new DoubleCell(name);
                }
                else if (objType == typeof(DecimalCell))
                {
                    dataObj = new DecimalCell(name);
                }
                else if (objType == typeof(ByteCell))
                {
                    dataObj = new ByteCell(name);

                    if (secondaryType == ArcType.TYPE_BIT_FROM_VECTOR)
                    {
                        dataObj.Dependency = objData.Get(3).String;
                        dataObj.Offset = (int)objData.Get(4).Number;
                    }
                }
                else if (objType == typeof(ByteArrayCell))
                {
                    dataObj = new ByteArrayCell(name, (int)objData.Get(3).Number);
                }
                else if (objType == typeof(StringCell))
                {
                    if (secondaryType == ArcType.TYPE_STRING)
                        dataObj = new StringCell(name, (int)objData.Get(3).Number);
                    else if (secondaryType == ArcType.TYPE_STRING_BY_LEN | secondaryType == ArcType.TYPE_STRING_BY_HEADER_REF) // Both BY_LEN & BY_HEADER_REF have their dependency in the third slot. Group them
                        dataObj = new StringCell(name, -1, ArcType.TYPE_STRING_BY_LEN, objData.Get(3).String);
                }
                else if (objType == typeof(DateTime))
                {
                    dataObj = new DateTimeCell(name);
                }
                else if (objType == typeof(DateStringCell))
                {
                    dataObj = new DateStringCell(name);
                    dataObj.Length = (int)objData.Get(3).Number;
                }

                dataObj.SecondaryType = secondaryType;

                // Flag will always be the last field
                int flagsOffset = 3;

                if (objType == typeof(StringCell) || objType == typeof(ByteArrayCell) || objType == typeof(int) && secondaryType == ArcType.TYPE_COPY_INT32)
                    flagsOffset = 4;
                else if (objType == typeof(ByteCell) && secondaryType == ArcType.TYPE_BIT_FROM_VECTOR)
                    flagsOffset = 5;

                DynValue flagObj = objData.Get(flagsOffset);

                // So check to make sure the last field isn't nil and is a number
                if (!flagObj.IsNil())
                {
                    if (flagObj.Type == DataType.String) // User may have defined only a single flag by keyword (instead of SetFlags func)
                    {
                        string flag = flagObj.String;

                        int idx = flag.IndexOf(":");

                        if (idx > 0)
                        {
                            flag = flag.Substring(++idx, flag.Length - idx); // Get the actual flag (enum) value by moving the index forward 1 and reading the rest of the string

                            int nFlag;
                            if (int.TryParse(flag, out nFlag))
                                dataObj.Flags = (CellFlags)nFlag;
                        }
                    }
                    else if (flagObj.Type == DataType.Number)
                        dataObj.Flags = (CellFlags)flagObj.Number;
                }

                // The index can only be set once and then becomes immutable.
                dataObj.Index = i - 1;

                objCollection.Add(dataObj);
            }

            return objCollection;
        }

        /// <summary>
        /// Determine the derived cell type of the given LuaType
        /// </summary>
        /// <param name="type">Cell type loaded from structure lua</param>
        /// <returns>System Type representing a derived cell</returns>
        public Type GetObjectType(ArcType type)
        {
            switch (type)
            {
                case ArcType.TYPE_BYTE:
                    return typeof(ByteCell);

                case ArcType.TYPE_SKIP:
                case ArcType.TYPE_BYTE_ARRAY:
                    return typeof(ByteArrayCell);

                case ArcType.TYPE_BIT_VECTOR:
                    return typeof(IntCell);

                case ArcType.TYPE_BIT_FROM_VECTOR:
                    return typeof(ByteCell);

                case ArcType.TYPE_DECIMAL:
                    return typeof(DecimalCell);

                case ArcType.TYPE_INT16:
                case ArcType.TYPE_SHORT:
                    return typeof(ShortCell);

                case ArcType.TYPE_ENCODED_INT32:
                    return typeof(EncodedIntCell);

                case ArcType.TYPE_INT32:
                case ArcType.TYPE_INT:
                case ArcType.TYPE_SID:
                case ArcType.TYPE_STRING_LEN:
                case ArcType.TYPE_COPY_INT32:
                    return typeof(IntCell);

                case ArcType.TYPE_INT64:
                case ArcType.TYPE_LONG:
                    return typeof(LongCell);

                case ArcType.TYPE_SINGLE:
                case ArcType.TYPE_FLOAT:
                case ArcType.TYPE_FLOAT32:
                    return typeof(FloatCell);

                case ArcType.TYPE_DOUBLE:
                    return typeof(DoubleCell);

                case ArcType.TYPE_DATETIME:
                    return typeof(DateTime);

                case ArcType.TYPE_DATESTRING:
                    return typeof(DateStringCell);

                case ArcType.TYPE_STRING:
                case ArcType.TYPE_STRING_BY_HEADER_REF:
                case ArcType.TYPE_STRING_BY_LEN:
                case ArcType.TYPE_STRING_BY_REF:
                    return typeof(StringCell);
            }

            return null;
        }

        /// <summary>
        /// Determine the real underlying data type of the given LuaType
        /// </summary>
        /// <param name="type">Cell type loaded from structure lua</param>
        /// <returns>System Type representing data</returns>
        public Type GetObjectRealType(ArcType type)
        {
            switch (type)
            {
                case ArcType.TYPE_BYTE:
                case ArcType.TYPE_BIT_FROM_VECTOR:
                    return typeof(byte);

                case ArcType.TYPE_SKIP:
                case ArcType.TYPE_BYTE_ARRAY:
                    return typeof(byte[]);

                case ArcType.TYPE_DECIMAL:
                    return typeof(decimal);

                case ArcType.TYPE_SHORT:
                case ArcType.TYPE_INT16:
                    return typeof(short);

                case ArcType.TYPE_UINT_16:
                case ArcType.TYPE_USHORT:
                    return typeof(ushort);

                case ArcType.TYPE_ENCODED_INT32:
                    return typeof(int);

                case ArcType.TYPE_INT:
                case ArcType.TYPE_INT32:
                case ArcType.TYPE_BIT_VECTOR:
                case ArcType.TYPE_SID:
                case ArcType.TYPE_STRING_LEN:
                    return typeof(int);

                case ArcType.TYPE_UINT32:
                case ArcType.TYPE_UINT:
                    return typeof(uint);

                case ArcType.TYPE_LONG:
                case ArcType.TYPE_INT64:
                    return typeof(long);

                case ArcType.TYPE_SINGLE:
                case ArcType.TYPE_FLOAT:
                case ArcType.TYPE_FLOAT32:
                    return typeof(float);

                case ArcType.TYPE_DOUBLE:
                    return typeof(double);

                case ArcType.TYPE_DATESTRING:
                    return typeof(DateString);

                case ArcType.TYPE_STRING:
                case ArcType.TYPE_STRING_BY_HEADER_REF:
                case ArcType.TYPE_STRING_BY_LEN:
                case ArcType.TYPE_STRING_BY_REF:
                    return typeof(string);
            }

            return typeof(object);
        }

        /// <summary>
        /// Parse a user defined Enums lua that has been saved to the .\Modules folder
        /// </summary>
        /// <param name="key">Name of the table defined in the enum.lua</param>
        /// <returns>Prepared dictionary of enum values</returns>
        public Dictionary<string, int> GetEnum(string key)
        {
            Dictionary<string, int> enumDict = new Dictionary<string, int>();

            if (scriptObj.Globals[key] != null)
            {
                Table enumTbl = scriptObj.Globals[key] as Table;

                for (int i = 1; i < enumTbl.Length + 1; i++)
                {
                    Table valTbl = enumTbl.Get(i).Table;

                    enumDict.Add(valTbl.Get(1).String, (int)valTbl.Get(2).Number);
                }
            }

            return enumDict;
        }

        /// <summary>
        /// Buffer the file at the provided filename and parse the contents based on the structure contained in the previously parsed structure lua.
        /// </summary>
        /// <param name="filename">Path to the rdb file to be read</param>
        public void Read(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"Cannot locate file!\n\t- {filename}");

            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(filename)))
            {
                Header.Read(stream);

                SID.New();

                for (int i = 0; i < RowCount; i++)
                {
                    int readCount = 1;

                    if (SpecialCase == CaseFlag.DoubleLoop)
                    {
                        byte[] buffer = new byte[4];

                        stream.Read(buffer, 0, buffer.Length);

                        readCount = BitConverter.ToInt32(buffer, 0);
                    }

                    for (int l = 0; l < readCount; l++)
                    {
                        RowObject dataRow = new RowObject(DataCells);

                        dataRow.Read(stream, Header);

                        if (useProcessor)
                            processRow(dataRow, ProcessorMode.Read);

                        Rows.Add(dataRow);
                    }
                }
            }
        }

        /// <summary>
        /// Write the previously loaded contents of this structure object to disk at the provided path
        /// </summary>
        /// <param name="filename"></param>
        public void Write(string filename)
        {
            using (MemoryStream Stream = new MemoryStream())
            {
                Header["Signature"] = ByteUtility.ToBytes("\0\0\0\0\0\0\0\0Written by Archemedes v0.1.0", 120);
                Header.Write(Stream);

                int rowCount = Rows.Count;

                if (SpecialCase == CaseFlag.DoubleLoop)
                {
                    int pVal = 0;

                    for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
                    {
                        RowObject row = Rows[rowIdx];
                        int cVal = row.GetValueByFlag<int>(CellFlags.LoopCounter);

                        if (pVal != cVal)
                        {
                            IntCell counter = row.GetCellByFlag(CellFlags.LoopCounter) as IntCell;
                            List<RowObject> treeRows = Rows.FindAll(r => (int)r[counter.Index] == cVal);

                            byte[] buffer = BitConverter.GetBytes(treeRows.Count);

                            Stream.Write(buffer, 0, buffer.Length);

                            for (int tR = 0; tR < treeRows.Count; tR++)
                                treeRows[tR].Write(Stream); // We will never need to process this row

                            pVal = cVal;
                        }
                    }
                }
                else
                    for (int i = 0; i < rowCount; i++)
                    {
                        if (useProcessor)
                            processRow(Rows[i], ProcessorMode.Write);

                        Rows[i].Write(Stream);
                    }

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    Stream.WriteTo(fs);
            }
        }

        /// <summary>
        /// Generate a csv document from the schema and data contents of this structure object
        /// </summary>
        /// <returns>Formatted string representing csv file data</returns>
        public string GenerateCSV()
        {
            string csv = string.Empty;

            string[] names = VisibleCellNames;

            csv += $"{string.Join(", ", names)}\n";

            for (int i = 0; i < Rows.Count; i++)
                csv += $"{Rows[i].ToCSVString()}\n";

            return csv;
        }

        /// <summary>
        /// Search the loaded collection of rows for cells containing a value'(s) that meet the provided operator criteria
        /// </summary>
        /// <param name="column">Cell being targeted</param>
        /// <param name="value">Value to be compared against</param>
        /// <param name="op">Operator of the comparison</param>
        /// <returns>List of row/cell index pairs if returnType is Indicies, List of value objects otherwise</returns>
        public object Search(string column, Array value, SearchOperator op) => Search(column, value, op);

        /// <summary>
        /// Search the loaded collection of rows for cells containing a value'(s) that meet the provided operator criteria
        /// </summary>
        /// <param name="column">Cell being targeted</param>
        /// <param name="value">Value to be compared against</param>
        /// <param name="ret">Type of data to be returned.</param>
        /// <returns>List of row/cell index pairs if returnType is Indicies, List of value objects otherwise</returns>
        public object Search(string column, Array value, SearchReturn ret) => Search(column, value, SearchOperator.Equal, ret);

        /// <summary>
        /// Search the loaded collection of rows for cells containing a value'(s) that meet the provided operator criteria
        /// </summary>
        /// <param name="column">Cell being targeted</param>
        /// <param name="value">Value to be compared against</param>
        /// <param name="op">Operator of the comparison</param>
        /// <param name="returnType">Type of data to be returned.</param>
        /// <returns>List of row/cell index pairs if return type is Indicies, List of value objects otherwise</returns>
        public object Search(string column, Array value, SearchOperator op = SearchOperator.Equal, SearchReturn returnType = SearchReturn.Values)
        {
            if (column == null)
                throw new ArgumentNullException("Column name cannot be null!");

            CellBase cell = DataCells.Find(c => c.Name == column);

            IList values = new List<object>();

            if (returnType == SearchReturn.Indicies)
                values = new List<KeyValuePair<int, int>>();

            if (cell == null)
                throw new NullReferenceException($"Could not find cell by name: {column}");

            for (int i = 0; i < RowCount; i++)
            {
                RowObject row = Rows[i];

                for (int j = 0; j < value.Length; j++) // Since the input 'value' is an array of values between 1 and n elements, lets loop the generic array
                {
                    dynamic curVal = (cell.PrimaryType == typeof(string)) ? Encoding.GetString((byte[])row[column]) : Convert.ChangeType(row[column], cell.PrimaryType);
                    dynamic cmpVal = Convert.ChangeType(value.GetValue(j), cell.PrimaryType); // Now we can convert the generic array element into a proper value type

                    bool add = false;

                    switch (op)
                    {
                        case SearchOperator.Above:
                            add = curVal > cmpVal;
                            break;

                        case SearchOperator.Below:
                            add = curVal < cmpVal;
                            break;

                        case SearchOperator.Equal:
                            add = curVal == cmpVal;
                            break;

                        case SearchOperator.NotEqual:
                            add = curVal != cmpVal;
                            break;

                        case SearchOperator.Like:
                            {
                                string curStr = curVal.ToString();
                                string cmpStr = cmpVal.ToString();

                                if (curStr.Contains(cmpStr))
                                    add = true;
                            }
                            break;

                        case SearchOperator.NotLike:
                            {
                                string curStr = curVal.ToString();
                                string cmpStr = cmpVal.ToString();

                                if (!curStr.Contains(cmpStr))
                                    add = true;
                            }
                            break;

                        case SearchOperator.Between:
                            {
                                if (value.Length != 2)
                                    throw new InvalidDataException("the input value must consist of two comma delimited numbers!");

                                int min = Convert.ToInt32(value.GetValue(0));
                                int max = Convert.ToInt32(value.GetValue(1));

                                if (curVal >= min && curVal <= max)
                                    add = true;
                            }
                            break;
                    }

                    if (add)
                        if (returnType == SearchReturn.Indicies)
                            values.Add(new KeyValuePair<int, int>(i, cell.Index));
                        else
                            values.Add(curVal);
                }
            }

            if (returnType == SearchReturn.Values)
                return values.Cast<object>().ToArray();

            return values.Cast<KeyValuePair<int, int>>().ToArray();
        }

        /// <summary>
        /// Create a clone of this structure object (contains only the schema, not actual data!)
        /// </summary>
        /// <returns>Clone of this structure</returns>
        public object Clone()
        {
            StructureObject cloneStruct = new StructureObject(FilePath, false);

            cloneStruct.Name = Name;
            cloneStruct.Author = Author;
            cloneStruct.RDBName = RDBName;
            cloneStruct.DatabaseName = DatabaseName;
            cloneStruct.TableName = TableName;
            cloneStruct.SelectStatement = selectStatement;
            cloneStruct.Version = Version;
            cloneStruct.Epic = Epic;
            cloneStruct.Encoding = Encoding;
            cloneStruct.SpecialCase = SpecialCase;
            cloneStruct.HeaderType = HeaderType;

            for (int i = 0; i < HeaderCells.Count; i++)
            {
                ICloneable clonableCell = HeaderCells[i] as ICloneable;

                cloneStruct.HeaderCells.Add((CellBase)clonableCell.Clone());
            }

            for (int i = 0; i < DataCells.Count; i++)
            {
                ICloneable clonableCell = DataCells[i] as ICloneable;

                cloneStruct.DataCells.Add((CellBase)clonableCell.Clone());
            }

            return cloneStruct;
        }

        void processRow(RowObject rowObj, string mode) => scriptObj.Call("ProcessRow", new object[] { rowObj, mode });
    }
}
