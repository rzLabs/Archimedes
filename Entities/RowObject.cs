using Archimedes.Abstractions;
using Archimedes.Cells;
using Archimedes.Enums;
using Archimedes.Utilities;
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
    /// Enumerable object representing a collection of cells which exposes its own IO calls.
    /// </summary>
    public class RowObject : IEnumerable<KeyValuePair<CellBase, object>>
    {
        List<CellBase> cells = new List<CellBase>();
        object[] values = null;

        /// <summary>
        /// Construct a new row object instance based on the provided cells
        /// </summary>
        /// <param name="cells">Descriptions of the data to be held in this row object</param>
        public RowObject(List<CellBase> cells)
        {
            this.cells = cells;

            values = new object[cells.Count];
        }

        /// <summary>
        /// Get the boxed value object for the provided cell name
        /// </summary>
        /// <param name="name">Name of the cell to be described</param>
        /// <returns>Base cell class capable of describing the cell</returns>
        public object this[string name]
        {
            get
            {
                int index = cells.Find(c => c.Name == name).Index;

                return values?[index];
            }
            set
            {
                int index = cells.Find(c => c.Name == name).Index;

                if (index >= 0 && index < values.Length)
                    values[index] = value;
            }
        }

        /// <summary>
        /// Get the boxed value object at the given index
        /// </summary>
        /// <param name="index">0 based index</param>
        /// <returns>Boxed (object) value</returns>
        public object this[int index]
        {
            get => values?[index];
            set
            {
                if (index >= 0 && index < values.Length)
                    values[index] = value;
            }
        }

        /// <summary>
        /// Get the first boxed value object whose cell bears the provided flag
        /// </summary>
        /// <param name="flags">Flag'(s) to be matched against</param>
        /// <returns>Boxed (object) value</returns>
        public object this[CellFlags flags] => values?[GetCellByFlag(flags).Index];

        /// <summary>
        /// Length (of cells) contained in this row object
        /// </summary>
        public int Length => cells.Count;

        /// <summary>
        /// Enables enumeration over this row object by returning key value pairs of CellBase, object
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<CellBase, object>> GetEnumerator()
        {
            foreach (CellBase cell in cells)
                yield return new KeyValuePair<CellBase, object>(cell, values[cell.Index]);
        }

        /// <summary>
        /// Get a given cells description object
        /// </summary>
        /// <param name="index">Zero based index of the cell</param>
        /// <returns>Populated CellBase or null</returns>
        public CellBase GetCell(int index) => cells?[index];

        /// <summary>
        /// Get a given cells description object
        /// </summary>
        /// <param name="name">Name of the cell</param>
        /// <returns>Populated CellBase or null</returns>
        public CellBase GetCell(string name) => cells?.Find(c => c.Name == name);

        /// <summary>
        /// Get Cell description by the provided flags vector
        /// </summary>
        /// <param name="flags">Flags Vector <b>int</b> representing flag list</param>
        /// <returns>Cell description</returns>
        public CellBase GetCellByFlag(CellFlags flags) => cells.Find(c => c.Flags == flags);

        /// <summary>
        /// Get the bit cells (and their values) bearing the provided dependency
        /// </summary>
        /// <param name="name">Cell the bit fields depend on</param>
        /// <returns>Collection of cells and their values</returns>
        public KeyValuePair<CellBase, object>[] GetBits(string name)
        {
            List<CellBase> retCells = cells.FindAll(c => c.Dependency == name);
            KeyValuePair<CellBase, object>[] results = new KeyValuePair<CellBase, object>[retCells.Count];

            for (int i = 0; i < retCells.Count; i++)
                results[i] = new KeyValuePair<CellBase, object>(retCells[i], values[retCells[i].Index]);

            return results;
        }

        /// <summary>
        /// Get the value of a cell bearing the provided flags vector
        /// </summary>
        /// <typeparam name="T">Type to return the value as</typeparam>
        /// <param name="flags">Flags vector to be checked against</param>
        /// <returns>Value converted to T</returns>
        public T GetValueByFlag<T>(CellFlags flags) => (T)Convert.ChangeType(values?[cells.Find(c => c.Flags == flags).Index], typeof(T));

        /// <summary>
        /// Iterate over and read all cells in this row object
        /// </summary>
        /// <param name="stream">Stream of the rdb file being read</param>
        public void Read(MemoryStream stream, RowObject header = null)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                CellBase cell = cells[i];

                cell.Stream = stream;

                if (cell.SecondaryType == ArcType.TYPE_BIT_FROM_VECTOR) // this type is not physically read from the stream
                {
                    string depObjName = cell.Dependency;

                    int vector = (int)this[depObjName];
                    int offset = cell.Offset;

                    values[i] = Convert.ToInt32((vector & (1 << offset)) != 0);

                    continue;
                }
                else if (cell.SecondaryType == ArcType.TYPE_SKIP) // Is not actually read, just move the stream.
                {
                    values[i] = new byte[cell.Length];

                    stream.Seek(cell.Length, SeekOrigin.Current);

                    continue;
                }
                else if (cell.SecondaryType == ArcType.TYPE_COPY_INT32) // Value is fetched from previously read cell.
                {
                    CellBase depCell = cells?.Find(c => c.Name == cell.Dependency);

                    values[i] = values[depCell.Index];

                    stream.Seek(4, SeekOrigin.Current);

                    continue;
                }
                else if (cell.SecondaryType == ArcType.TYPE_SID) // Is not actually read, just increment the sid value and continue;
                {
                    values[i] = SID.Increment;

                    continue;
                }
                else if (cell.SecondaryType == ArcType.TYPE_STRING_BY_LEN) // Must pass in the length stored by the dependency cell before reading
                {
                    string depObjName = cell.Dependency;

                    cell.Length = (int)this[depObjName];
                }
                else if (cell.SecondaryType == ArcType.TYPE_STRING_BY_HEADER_REF) // Must get the length of the cell by its dependency header cell
                {
                    string depObjName = cell.Dependency;

                    cell.Length = (int)header[depObjName];
                }

                ICellIO readableObj = cells[i] as ICellIO;

                values[i] = readableObj.Read();
            }
        }

        /// <summary>
        /// Iterate over and write all cell data in this row object to disk
        /// </summary>
        /// <param name="stream">Stream of the rdb file being written to</param>
        public void Write(MemoryStream stream)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                CellBase cell = cells[i];

                // Bits get collected into a BIT_FROM_VECTOR, do not write!
                if (cell.SecondaryType == ArcType.TYPE_BIT_FROM_VECTOR)
                    continue;

                // Generate a new bit vector (int) from the toggled offsets
                if (cell.SecondaryType == ArcType.TYPE_BIT_VECTOR)
                {
                    // Get the dependent bit fields
                    KeyValuePair<CellBase, object>[] bits = GetBits(cell.Name);

                    // Create a default vector
                    int vector = default(int);

                    // Toggle the bit indexes appropriately
                    foreach (KeyValuePair<CellBase, object> bit in bits)
                        vector |= (Convert.ToInt32(bit.Value) << bit.Key.Offset);

                    // Update this cells value accordingly and proceed to write
                    values[cell.Index] = vector;
                }
                else if (cell.SecondaryType == ArcType.TYPE_SKIP) // Create the blank data to be written
                    values[cell.Index] = new byte[cell.Length];
                else if (cell.SecondaryType == ArcType.TYPE_COPY_INT32)
                {
                    CellBase depCell = cells?.Find(c => c.Name == cell.Dependency);

                    values[cell.Index] = values[depCell.Index];
                }

                cell.Stream = stream;

                ICellIO writableObj = cell as ICellIO;

                writableObj.Write(values[i]);
            }
        }

        /// <summary>
        /// Convert this rows contents into a csv string
        /// </summary>
        /// <returns>Formatted csv string</returns>
        public string ToCSVString()
        {
            List<string> valueStrings = new List<string>();

            for (int i = 0; i < Length; i++)
            {
                CellBase cell = GetCell(i);
                string cVal;

                if (cell.Flags.HasFlag(CellFlags.SqlIgnore))
                    continue;

                if (cell.PrimaryType == typeof(string))
                    cVal = ByteUtility.ToString((byte[])values[cell.Index]);
                else
                    cVal = values[cell.Index].ToString();

                valueStrings.Add(cVal);
            }

            return string.Join(", ", valueStrings);
        }

        /// <summary>
        /// Convert the schema and data of this row object into a prepared insert or update sql statement string.
        /// </summary>
        /// <param name="tableName">Table name (optional, <b>use only with type Update</b>)</param>
        /// <param name="type">Type of SQL string to be created</param>
        /// <param name="whereColumn">Name of column to use for where statement (optional, <b>use only with type Update</b>)</param>
        /// <returns>Prepared SQL statement</returns>
        public string ToSqlString(string tableName, SqlStringType type = SqlStringType.Insert, string whereColumn = null)
        {
            if (type == SqlStringType.Update && string.IsNullOrEmpty(whereColumn))
                throw new Exception("SqlStringType.Update requires a valid where column!");

            string cmdStr = (type == SqlStringType.Insert) ? $"INSERT INTO {tableName} " : $"UPDATE {tableName} SET ";
            string cellStr = string.Empty;
            string valStr = string.Empty;
            string tmpStr = string.Empty;

            for (int i = 0; i <= Length; i++)
            {
                CellBase cell;

                if (i == Length)
                {
                    if (type == SqlStringType.Insert)
                        return $"{cmdStr}({cellStr.Remove(cellStr.Length - 1)}) values ({valStr.Remove(valStr.Length - 1)})";
                    else
                    {
                        cell = GetCell(whereColumn);
                        return $"{cmdStr}{tmpStr.Remove(tmpStr.Length - 2, 2)} where [{whereColumn}] = {values[cell.Index]}";
                    }
                }

                cell = GetCell(i);
                object valObj = values[cell.Index];

                if (cell.Flags.HasFlag(CellFlags.SqlIgnore))
                    continue;

                if (type == SqlStringType.Insert)
                    cellStr += $"[{cell.Name}]";
                else
                {
                    cellStr = $"[{cell.Name}] = ";
                    valStr = string.Empty;
                }

                // Strings must be wrapped in '' and instances of the ' literal must be changed to sql friendly ''
                if (cell.SecondaryType == ArcType.TYPE_STRING || cell.SecondaryType == ArcType.TYPE_STRING_BY_LEN || cell.SecondaryType == ArcType.TYPE_STRING_BY_REF)
                {
                    string objStr = ByteUtility.ToString((byte[])valObj).ToString().TrimEnd('\0');
                    string convStr = objStr.Replace("'", "''");
                    valStr += $"'{convStr}'";
                }
                else if (cell.SecondaryType == ArcType.TYPE_DATETIME)
                {
                    //yyyy-MM-dd HH:mm:ss.fff
                    //1999-12-31 15:00:00.000
                    DateTime objDT = (DateTime)valObj;
                    valStr += $"'{objDT.ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
                }
                else
                    valStr += valObj.ToString();

                if (type == SqlStringType.Insert)
                {
                    cellStr += ",";
                    valStr += ",";
                }
                else
                    tmpStr += $"{cellStr} {valStr}, ";
            }

            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
