public interface IByteWriter
{
    public void WriteByte(byte value);
    public void WriteBytes(byte[] value);
    public void WriteShort(short value);
    public void WriteInt(int value);
    public void WriteLong(long value);
    public void WriteFloat(float value);
    public void WriteDouble(double value);
    public void WriteBool(bool value);
    public void WriteString(string value);
}
