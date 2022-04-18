public interface IByteReader
{
    public byte ReadByte(bool moveIndex = true);
    public byte[] ReadBytes(int length, bool moveIndex = true);
    public short ReadShort(bool moveIndex = true);
    public int ReadInt(bool moveIndex = true);
    public long ReadLong(bool moveIndex = true);
    public float ReadFloat(bool moveIndex = true);
    public double ReadDouble(bool moveIndex = true);
    public bool ReadBool(bool moveIndex = true);
    public string ReadString(bool moveIndex = true);
}
