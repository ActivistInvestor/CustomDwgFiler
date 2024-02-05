using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Collections;
using AcRx = Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

namespace Autodesk.AutoCAD.DatabaseServices.MyExtensions
{
   /// <summary>
   /// Custom DwgFiler class that can be used to read DBObject
   /// data into a ResultBuffer or array of TypedValues.
   /// 
   /// Use the included DBObject.ToArray() extension method to
   /// read the DWG data of a DBObject:
   /// 
   ///    DBObject someDBObject = .....
   ///    TypedValue[] data = someDBObject.ToArray();
   /// 
   /// Note that while DwgDataFiler implements both read and
   /// write operations, read operations (e.g., DwgIn) have
   /// never been needed or used, are untested, and probably
   /// will not have the desired result.
   /// </summary>

   public static class DwgDataListExtensions
   {
      public static TypedValue[] ToArray(this DBObject obj)
      {
         if(obj == null)
            throw new ArgumentNullException(nameof(obj));
         using(DwgDataList list = new DwgDataList())
         {
            obj.DwgOut(list);
            return list.TypedValues;
         }
      }

      /// <summary>
      /// Example: Dumps the result of ToArray() to the console:
      /// </summary>

      public static void DwgDump(this DBObject obj)
      {
         var data = obj.ToArray();
         Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
         int i = 0;
         foreach(TypedValue tv in data)
         {
            ed.WriteMessage("\n[{0}] {1}: {2}",
               i++, (DxfCode) tv.TypeCode, tv.Value.ToString());
         }
      }
   }

   public class DwgDataList : DwgFiler, IList<DwgDataItem>
   {
      ErrorStatus status = ErrorStatus.OK;
      FilerType filerType = FilerType.CopyFiler;
      List<DwgDataItem> data = new List<DwgDataItem>();
      int position = 0;

      public DwgDataList(FilerType filerType = FilerType.CopyFiler)
      {
         this.filerType = filerType;
      }

      public void Rewind()
      {
         position = 0;
      }

      public DwgDataItem? Peek()
      {
         if(data.Count > 0 && position < data.Count - 1)
         {
            return data[position];
         }
         return null;
      }

      public IList<DwgDataItem> Data
      {
         get
         {
            return data.AsReadOnly();
         }
      }

      public override ErrorStatus FilerStatus
      {
         get
         {
            return status;
         }
         set
         {
            status = value;
         }
      }

      public override FilerType FilerType
      {
         get
         {
            return FilerType.CopyFiler;
         }
      }

      public override long Position
      {
         get
         {
            return position;
         }
      }

      public int Count
      {
         get
         {
            return data.Count;
         }
      }

      public bool IsReadOnly
      {
         get
         {
            return true;
         }
      }

      public bool IsEndOfData
      {
         get
         {
            return position > this.data.Count - 1;
         }
      }

      public DwgDataItem this[int index]
      {
         get
         {
            return data[CheckIndex(index)];
         }

         set
         {
            SetValueAt(index, value);
         }
      }

      T TypeMismatch<T>(int index, Type type)
      {
         throw new InvalidCastException($"Type mismatch at {index} ({type.Name}, expecting {typeof(T).Name})");
      }

      public override IntPtr ReadAddress()
      {
         return Read<IntPtr>();
      }

      public override byte[] ReadBinaryChunk()
      {
         return Read<byte[]>();
      }

      protected virtual T Read<T>()
      {
         if(IsEndOfData)
            throw new AcRx.Exception(AcRx.ErrorStatus.EndOfObject);
         object value = data[position].Value;
         if(value is T)
         {
            ++position;
            return (T) value;
         }
         return TypeMismatch<T>(position, value.GetType());
      }

      public override bool ReadBoolean()
      {
         return Read<bool>();
      }

      public override byte ReadByte()
      {
         return Read<Byte>();
      }

      public override void ReadBytes(byte[] value)
      {
         byte[] array = Read<byte[]>();
         Array.Copy(array, value, value.Length);
      }

      public override double ReadDouble()
      {
         return Read<double>();
      }

      public override Handle ReadHandle()
      {
         return Read<Handle>();
      }

      public override ObjectId ReadHardOwnershipId()
      {
         return Read<ObjectId>();
      }

      public override ObjectId ReadHardPointerId()
      {
         return Read<ObjectId>();
      }

      public override short ReadInt16()
      {
         return Read<short>();
      }

      public override int ReadInt32()
      {
         return Read<int>();
      }

      public override long ReadInt64()
      {
         return Read<long>();
      }

      public override Point2d ReadPoint2d()
      {
         return Read<Point2d>();
      }

      public override Point3d ReadPoint3d()
      {
         return Read<Point3d>();
      }

      public override Scale3d ReadScale3d()
      {
         return Read<Scale3d>();
      }

      public override ObjectId ReadSoftOwnershipId()
      {
         return Read<ObjectId>();
      }

      public override ObjectId ReadSoftPointerId()
      {
         return Read<ObjectId>();
      }

      public override string ReadString()
      {
         return Read<string>();
      }

      public override ushort ReadUInt16()
      {
         return Read<ushort>();
      }

      public override uint ReadUInt32()
      {
         return Read<uint>();
      }

      public override ulong ReadUInt64()
      {
         return Read<ulong>();
      }

      public override Vector2d ReadVector2d()
      {
         return Read<Vector2d>();
      }

      public override Vector3d ReadVector3d()
      {
         return Read<Vector3d>();
      }

      public override void ResetFilerStatus()
      {
         status = ErrorStatus.OK;
      }

      public override void Seek(long offset, int method)
      {
         throw new NotSupportedException();  // TODO
      }

      /// <summary>
      /// Write overrides, used by this class to generate
      /// a list of data items.
      /// </summary>
      /// <param name="value"></param>
      
      public override void WriteAddress(IntPtr value)
      {
         Add(DwgDataType.Ptr, value);
      }

      public override void WriteBinaryChunk(byte[] chunk)
      {
         if(chunk == null)
            throw new ArgumentNullException(nameof(chunk));
         byte[] data = new byte[chunk.Length];
         chunk.CopyTo(data, 0);
         this.Add(DwgDataType.BChunk, data);
      }

      public override void WriteBoolean(bool value)
      {
         this.Add(DwgDataType.Bool, value);
      }

      public override void WriteByte(byte value)
      {
         this.Add(DwgDataType.Byte, value);
      }

      public override void WriteBytes(byte[] value)
      {
         this.Add(DwgDataType.ByteArray, value);
      }

      public override void WriteDouble(double value)
      {
         this.Add(DwgDataType.Real, value);
      }

      public override void WriteHandle(Handle handle)
      {
         this.Add(DwgDataType.Handle, handle);
      }

      public override void WriteHardOwnershipId(ObjectId value)
      {
         this.Add(DwgDataType.HardOwnershipId, value);
      }

      public override void WriteHardPointerId(ObjectId value)
      {
         this.Add(DwgDataType.HardPointerId, value);
      }

      public override void WriteInt16(short value)
      {
         this.Add(DwgDataType.Int16, value);
      }

      public override void WriteInt32(int value)
      {
         this.Add(DwgDataType.Int32, value);
      }

      public override void WriteInt64(long value)
      {
         this.Add(DwgDataType.Int64, value);
      }

      public override void WritePoint2d(Point2d value)
      {
         this.Add(DwgDataType.Point2d, value);
      }

      public override void WritePoint3d(Point3d value)
      {
         this.Add(DwgDataType.Point3d, value);
      }

      public override void WriteScale3d(Scale3d value)
      {
         this.Add(DwgDataType.Scale3d, value);
      }

      public override void WriteSoftOwnershipId(ObjectId value)
      {
         this.Add(DwgDataType.SoftOwnershipId, value);
      }

      public override void WriteSoftPointerId(ObjectId value)
      {
         this.Add(DwgDataType.SoftPointerId, value);
      }

      public override void WriteString(string value)
      {
         this.Add(DwgDataType.Text, value);
      }

      public override void WriteUInt16(ushort value)
      {
         this.Add(DwgDataType.UInt16, value);
      }

      public override void WriteUInt32(uint value)
      {
         this.Add(DwgDataType.UInt32, value);
      }

      public override void WriteUInt64(ulong value)
      {
         this.Add(DwgDataType.UInt64, value);
      }

      public override void WriteVector2d(Vector2d value)
      {
         this.Add(DwgDataType.Vector2d, value);
      }

      public override void WriteVector3d(Vector3d value)
      {
         this.Add(DwgDataType.Vector3d, value);
      }

      TypedValue[] typedValues = null;
      
      public TypedValue[] TypedValues
      {
         get
         {
            if(typedValues == null)
            {
               TypedValue[] values = new TypedValue[this.Count];
               for(int i = 0; i < this.Count; i++)
                  values[i] = this[i].ToTypedValue();
               typedValues = values;
            }
            return typedValues;
         }
      }

      public List<object> Values
      {
         get
         {
            return data.ConvertAll(d => d.Value);
         }
      }

      /// <summary>
      /// The distinct set of DwgDataTypes contained in the instance.
      /// </summary>
      public IEnumerable<DwgDataType> Types
      {
         get
         {
            return data.Select(d => d.DataType).Distinct();
         }
      }

      int CheckIndex(int index)
      {
         if(index < 0 || index > data.Count - 1)
            throw new IndexOutOfRangeException($"index = {index} count = {data.Count}");
         return index;
      }

      public object GetValueAt(int index)
      {
         return data[CheckIndex(index)].Value;
      }

      public DwgDataType GetTypeAt(int index)
      {
         return data[CheckIndex(index)].DataType;
      }

      public bool ContainsType(DwgDataType type)
      {
         return IndexOfType(type) > -1;
      }

      public int IndexOfType(DwgDataType type)
      {
         return IndexOf(d => d.DataType == type);
      }

      public int IndexOfValue(object value)
      {
         return IndexOf(d => object.Equals(d.Value, value));
      }

      /// <summary>
      /// IList<DwgData> methods that modify the contents of
      /// the instance (namely delete) should never be used
      /// during a call to a method that triggers the use of
      /// the filer methods above. Altering the contents of
      /// the instance affects the Position property, which
      /// may be corrupt if one or more elements are removed
      /// from the instance.
      /// 
      /// Methods that trigger calls to the filer methods
      /// above include DBObject.DwgIn() and DwgOut(), along
      /// with the DwgInFields() and DwgOutFields() methods of
      /// non-DBObjects that were designed to be persisted in
      /// the data of a DBObject.
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>

      public int IndexOf(DwgDataItem item)
      {
         return data.IndexOf(item);
      }

      public int IndexOf(Func<DwgDataItem, bool> predicate)
      {
         for(int i = 0; i < data.Count; i++)
         {
            if(predicate(data[i]))
               return i;
         }
         return -1;
      }

      public void Insert(int index, DwgDataItem item)
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public void RemoveAt(int index)
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public void SetValueAt(int index, object value)
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public void Add(DwgDataType type, object value)
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public void Add(TypedValue value)
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public virtual void Add(DwgDataItem item)
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public void Clear()
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public bool Contains(DwgDataItem item)
      {
         return data.Contains(item);
      }

      public void CopyTo(DwgDataItem[] array, int arrayIndex)
      {
         data.CopyTo(array, arrayIndex);
      }

      public bool Remove(DwgDataItem item)
      {
         throw new NotSupportedException("The instance is read-only");
      }

      public IEnumerator<DwgDataItem> GetEnumerator()
      {
         return data.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return this.GetEnumerator();
      }
   }

   public enum DwgDataType
   {
      Null = 0,
      Real = 1,
      Int32 = 2,
      Int16 = 3,
      Int8 = 4,
      Text = 5,
      BChunk = 6,
      Handle = 7,
      HardOwnershipId = 8,
      SoftOwnershipId = 9,
      HardPointerId = 10,
      SoftPointerId = 11,
      Real3 = 12,
      Int64 = 13,
      NotRecognized = 19,

      /// <summary>
      /// Application-specific, not all values are supported.
      /// </summary>
      Point3d = 20,
      Point2d = 21,
      ByteArray = 22,
      Byte = 23,
      Bool = 24,
      Ptr = 25,
      Scale3d = 26,
      Vector3d = 27,
      Vector2d = 28,
      UInt16 = 29,
      UInt32 = 30,
      UInt64 = 31
   };

   public struct DwgDataItem : IEquatable<DwgDataItem>
   {
      public DwgDataItem(DwgDataType type, object value)
      {
         if(value == null)
            throw new ArgumentNullException("value");
         this.DataType = type;
         this.Value = value;
      }

      public DwgDataItem(TypedValue value)
         : this(DxfCodeToDwgDataType((DxfCode)value.TypeCode), value.Value)
      {
      }

      public DwgDataType DataType
      {
         get;
         private set;
      }

      public object Value
      {
         get; private set;
      }

      public TypedValue ToTypedValue()
      {
         switch(this.DataType)
         {
            case DwgDataType.Bool:
               return new TypedValue((short)DxfCode.Bool, (short)Value);
            case DwgDataType.Byte:
            case DwgDataType.Int8:
               return new TypedValue((short)DxfCode.Int8, Value);
            case DwgDataType.Int16:
               return new TypedValue((short)DxfCode.Int16, Value);
            case DwgDataType.Int32:
               return new TypedValue((short)DxfCode.Int32, Value);
            case DwgDataType.Int64:
               return new TypedValue((short)DxfCode.Int64, Value);
            case DwgDataType.Text:
               return new TypedValue((short)DxfCode.Text, Value);
            case DwgDataType.Handle:
               return new TypedValue((short)DxfCode.Handle, Value.ToString());
            case DwgDataType.HardOwnershipId:
               return new TypedValue((short)DxfCode.HardOwnershipId, Value);
            case DwgDataType.SoftOwnershipId:
               return new TypedValue((short)DxfCode.SoftOwnershipId, Value);
            case DwgDataType.HardPointerId:
               return new TypedValue((short)DxfCode.SoftPointerId, Value);
            case DwgDataType.SoftPointerId:
               return new TypedValue((short)DxfCode.SoftPointerId, Value);
            case DwgDataType.Point3d:
            case DwgDataType.Point2d:  // returns as Point3d
            case DwgDataType.Real3:
               return new TypedValue((short)DxfCode.XCoordinate, Value);
            case DwgDataType.Vector3d:
               return new TypedValue((short)DxfCode.NormalX);

            //case DwgDataType.Scale3d:
            //case DwgDataType.BChunk:
            //case DwgDataType.ByteArray:
            //case DwgDataType.Ptr:
            default:
               throw new NotSupportedException($"Unsupported DwgDatType: {DataType}");


         }
      }

      static Dictionary<DwgDataType, Type> typeMap = new Dictionary<DwgDataType, System.Type>();

      static DwgDataItem()
      {
         typeMap[DwgDataType.Text] = typeof(string);
         typeMap[DwgDataType.Bool] = typeof(bool);
         typeMap[DwgDataType.Int8] = typeof(char);
         typeMap[DwgDataType.Int16] = typeof(short);
         typeMap[DwgDataType.Int32] = typeof(int);
         typeMap[DwgDataType.Int64] = typeof(long);
         typeMap[DwgDataType.Real3] = typeof(Point3d);
         typeMap[DwgDataType.Byte] = typeof(byte);
         typeMap[DwgDataType.Handle] = typeof(Handle); // string in dxf
         typeMap[DwgDataType.HardOwnershipId] = typeof(ObjectId);
         typeMap[DwgDataType.SoftOwnershipId] = typeof(ObjectId);
         typeMap[DwgDataType.HardPointerId] = typeof(ObjectId);
         typeMap[DwgDataType.SoftPointerId] = typeof(ObjectId);
         typeMap[DwgDataType.UInt32] = typeof(UInt32);
         typeMap[DwgDataType.UInt16] = typeof(UInt16);
         typeMap[DwgDataType.UInt64] = typeof(UInt64);
         typeMap[DwgDataType.Vector2d] = typeof(Vector2d);
         typeMap[DwgDataType.Vector3d] = typeof(Vector3d);
         typeMap[DwgDataType.Scale3d] = typeof(Scale3d);
         typeMap[DwgDataType.Real] = typeof(double);
         typeMap[DwgDataType.Point3d] = typeof(Point3d);
         typeMap[DwgDataType.Point2d] = typeof(Point2d);
         typeMap[DwgDataType.BChunk] = typeof(byte[]);
         typeMap[DwgDataType.Byte] = typeof(byte);
      }

      static DwgDataType DxfCodeToDwgDataType(DxfCode dxfCode)
      {
         switch(dxfCode)
         {
            case DxfCode.Text:
               return DwgDataType.Text;
            case DxfCode.Bool:
               return DwgDataType.Bool;
            case DxfCode.Int16:
               return DwgDataType.Int16;
            case DxfCode.Int32:
               return DwgDataType.Int32;
            case DxfCode.Int64:
               return DwgDataType.Int64;
            case DxfCode.Int8:
               return DwgDataType.Int8;
            case DxfCode.Handle:
               return DwgDataType.Handle;
            case DxfCode.HardOwnershipId:
               return DwgDataType.HardOwnershipId;
            case DxfCode.SoftOwnershipId:
               return DwgDataType.SoftOwnershipId;
            case DxfCode.SoftPointerId:
               return DwgDataType.HardPointerId;
            case DxfCode.XCoordinate:
               return DwgDataType.Real3;
            case DxfCode.NormalX:
               return DwgDataType.Vector3d;
            default:
               throw new NotSupportedException($"Unsupported DxfCode: {dxfCode}");
         }
      }

      public bool Equals(DwgDataItem other)
      {
         return this.DataType == other.DataType && object.Equals(this.Value, other.Value);
      }

      public override bool Equals(object obj)
      {
         if(obj is DwgDataItem)
            return Equals((DwgDataItem)obj);
         else
            return false;
      }

      public override int GetHashCode()
      {
         return HashCode.Combine(this.DataType.GetHashCode(), this.Value.GetHashCode());
      }

      public override string ToString()
      {
         return string.Format("DataType: {0} Value: {1}",
            this.DataType, this.Value?.ToString() ?? "(null)");
      }
   }

}
