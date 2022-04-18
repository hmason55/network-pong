using Godot;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Data object sent between the server and clients.
/// </summary>
public class Packet : IByteReader, IByteWriter, IDisposable
{
    /// <summary>
    /// The buffer holding packet data.
    /// </summary>
    private List<byte> _buffer;

    /// <summary>
    /// The readable section of the buffer.
    /// </summary>
    private byte[] _readableBuffer;

    /// <summary>
    /// The current index of the reader.
    /// </summary>
    private int _readIndex;

    #region Constructors
    /// <summary>
    /// Creates a new empty packet (without an ID).
    /// </summary>
    public Packet()
    {
        _buffer = new();
        _readIndex = 0;
    }

    /// <summary>
    /// Creates a new packet with a given ID. Used for sending.
    /// </summary>
    /// <param name="id">The packet ID.</param>
    public Packet(int id)
    {
        _buffer = new();
        _readIndex = 0;

        WriteInt(id);
    }


    /// <summary>
    /// Creates a packet from which data can be read. Used for receiving.
    /// </summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public Packet(byte[] data)
    {
        _buffer = new();
        _readIndex = 0;

        SetBytes(data);
    }
    #endregion Constructors

    #region Functions
    /// <summary>Sets the packet's content and prepares it to be read.</summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public void SetBytes(byte[] data)
    {
        WriteBytes(data);
        _readableBuffer = _buffer.ToArray();
    }

    /// <summary>
    /// Writes the length of the buffer to the beginning of the packet.
    /// </summary>
    public void WriteLength() => _buffer.InsertRange(0, BitConverter.GetBytes(_buffer.Count));

    /// <summary>
    /// Inserts an integer to the beginning of the packet.
    /// </summary>
    /// <param name="value">The integer to insert.</param>
    public void InsertInt(int value) => _buffer.InsertRange(0, BitConverter.GetBytes(value));

    /// <summary>
    /// Gets the packet's content in array form.
    /// </summary>
    /// <returns>The packet's contents as a byte array.</returns>
    public byte[] ToArray()
    {
        _readableBuffer = _buffer.ToArray();
        return _readableBuffer;
    }

    /// <summary>
    /// Gets the length of the buffer.
    /// </summary>
    /// <returns>The length of the buffer.</returns>
    public int Length() => _buffer.Count;

    /// <summary>
    /// Gets the number of unread bytes in the buffer.
    /// </summary>
    /// <returns>The number of unread bytes in the buffer.</returns>
    public int UnreadLength() => Length() - _readIndex;

    /// <summary>
    /// Resets the packet instance to allow it to be reused.
    /// </summary>
    /// <param name="shouldReset">Whether or not to reset the packet.</param>
    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            _buffer.Clear();
            _readableBuffer = null;
            _readIndex = 0;
        }
        else
        {
            // Move the read index back by an integer.
            _readIndex -= sizeof(int);
        }
    }
    #endregion

    #region Write Functions
    /// <summary>
    /// Write the byte value to the buffer.
    /// </summary>
    /// <param name="value">The byte value to write.</param>
    public void WriteByte(byte value) => _buffer.Add(value);

    /// <summary>
    /// Write the byte array to the buffer.
    /// </summary>
    /// <param name="value">The byte array to write.</param>
    public void WriteBytes(byte[] value) => _buffer.AddRange(value);

    /// <summary>
    /// Write the short value to the buffer.
    /// </summary>
    /// <param name="value">The short value to write.</param>
    public void WriteShort(short value) => _buffer.AddRange(BitConverter.GetBytes(value));

    /// <summary>
    /// Write the int value to the buffer.
    /// </summary>
    /// <param name="value">The int value to write.</param>
    public void WriteInt(int value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    /// <summary>
    /// Write the long value to the buffer.
    /// </summary>
    /// <param name="value">The long value to write.</param>
    public void WriteLong(long value) => _buffer.AddRange(BitConverter.GetBytes(value));

    /// <summary>
    /// Write the float value to the buffer.
    /// </summary>
    /// <param name="value">The float value to write.</param>
    public void WriteFloat(float value) => _buffer.AddRange(BitConverter.GetBytes(value));

    /// <summary>
    /// Write the double value to the buffer.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    public void WriteDouble(double value) => _buffer.AddRange(BitConverter.GetBytes(value));

    /// <summary>
    /// Write the bool value to the buffer.
    /// </summary>
    /// <param name="value">The bool value to write.</param>
    public void WriteBool(bool value) => _buffer.AddRange(BitConverter.GetBytes(value));

    /// <summary>
    /// Write the string value to the buffer.
    /// </summary>
    /// <param name="value">The string value to write.</param>
    public void WriteString(string value)
    {
        WriteInt(value.Length); // Add the length of the string to the packet
        _buffer.AddRange(Encoding.ASCII.GetBytes(value)); // Add the string itself
    }

    /// <summary>
    /// Write the Vector2 value to the buffer.
    /// </summary>
    /// <param name="value">The Vector2 value to write.</param>
    public void WriteVector2(Vector2 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
    }

    /// <summary>
    /// Write the Vector3 value to the buffer.
    /// </summary>
    /// <param name="value">The Vector3 value to write.</param>
    public void WriteVector3(Vector3 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
    }

    /*
     * Additional write methods can go here for custom objects...
     */
    #endregion

    #region Read Functions
    /// <summary>
    /// Reads a byte from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The byte value that was read.</returns>
    public byte ReadByte(bool moveIndex = true)
    {
        CheckReadIndex(typeof(byte));

        // If there are unread bytes
        byte value = _readableBuffer[_readIndex];
        MoveReadIndex(moveIndex, sizeof(byte));
        return value;
    }

    /// <summary>
    /// Reads an array of bytes from the packet.
    /// </summary>
    /// <param name="length">The length of the byte array.</param>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The byte array that was read.</returns>
    public byte[] ReadBytes(int length, bool moveIndex = true)
    {
        CheckReadIndex(typeof(byte[]));

        byte[] value = _buffer.GetRange(_readIndex, length).ToArray();
        MoveReadIndex(moveIndex, length);
        return value;
    }

    /// <summary>
    /// Reads a short from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The short value that was read.</returns>
    public short ReadShort(bool moveIndex = true)
    {
        CheckReadIndex(typeof(short));

        short value = BitConverter.ToInt16(_readableBuffer, _readIndex);
        MoveReadIndex(moveIndex, sizeof(short));
        return value;
    }

    /// <summary>
    /// Reads an int from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The int value that was read.</returns>
    public int ReadInt(bool moveIndex = true)
    {
        
        CheckReadIndex(typeof(int));

        int value = BitConverter.ToInt32(_readableBuffer, _readIndex);
        MoveReadIndex(moveIndex, 4);
        return value;
    }

    /// <summary>
    /// Reads a long from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The long value that was read.</returns>
    public long ReadLong(bool moveIndex = true)
    {
        CheckReadIndex(typeof(long));

        long value = BitConverter.ToInt64(_readableBuffer, _readIndex);
        MoveReadIndex(moveIndex, sizeof(long));
        return value;
    }

    /// <summary>
    /// Reads a float from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The float value that was read.</returns>
    public float ReadFloat(bool moveIndex = true)
    {
        CheckReadIndex(typeof(float));

        float value = BitConverter.ToSingle(_readableBuffer, _readIndex);
        MoveReadIndex(moveIndex, sizeof(float));
        return value;
    }

    /// <summary>
    /// Reads a double from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The double value that was read.</returns>
    public double ReadDouble(bool moveIndex = true)
    {
        CheckReadIndex(typeof(double));

        double value = BitConverter.ToDouble(_readableBuffer, _readIndex);
        MoveReadIndex(moveIndex, sizeof(double));
        return value;
    }

    /// <summary>
    /// Reads a bool from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The bool value that was read.</returns>
    public bool ReadBool(bool moveIndex = true)
    {
        CheckReadIndex(typeof(bool));

        bool value = BitConverter.ToBoolean(_readableBuffer, _readIndex);
        MoveReadIndex(moveIndex, sizeof(bool));
        return value;
    }

    /// <summary>
    /// Reads a string from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The string value that was read.</returns>
    public string ReadString(bool moveIndex = true)
    {
        try
        {
            int length = ReadInt();
            string value = Encoding.ASCII.GetString(_readableBuffer, _readIndex, length);
            if (moveIndex && value.Length > 0)
            {
                _readIndex += length;
            }
            return value;
        }
        catch
        {
            throw new Exception($"Could not read value of type {typeof(string).Name}");
        }
    }

    /// <summary>
    /// Reads a Vector2 from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The Vector2 value that was read.</returns>
    public Vector2 ReadVector2(bool moveIndex = true) => new(ReadFloat(moveIndex), ReadFloat(moveIndex));

    /// <summary>
    /// Reads a Vector3 from the packet.
    /// </summary>
    /// <param name="moveIndex">Whether or not to move the buffer's read position.</param>
    /// <returns>The Vector3 value that was read.</returns>
    public Vector3 ReadVector3(bool moveIndex = true) => new(ReadFloat(moveIndex), ReadFloat(moveIndex), ReadFloat(moveIndex));

    /// <summary>
    /// Checks if the read index is at the end of the buffer.
    /// </summary>
    /// <param name="type">The type of value to read.</param>
    /// <exception cref="Exception">The type cannot be read because the read index is at the end of the buffer.</exception>
    public void CheckReadIndex(Type type)
    {
        if (_readIndex >= _buffer.Count)
        {
            throw new Exception($"Could not read value of type '{type.Name}'");
        }
    }

    /// <summary>
    /// Move the read index.
    /// </summary>
    /// <param name="move">When true, move the read index.</param>
    /// <param name="length">The distance to move the read index.</param>
    public void MoveReadIndex(bool move, int length)
    {
        if (move)
        {
            _readIndex += length;
        }
    }

    /*
     * Additional read methods can go here for custom objects...
     */
    #endregion

    #region Dispose Functions
    /// <summary>
    /// When true, the packet is in the process of disposing.
    /// </summary>
    private bool disposed = false;

    /// <summary>
    /// Dispose the packet.
    /// </summary>
    /// <param name="disposing">When true, the packet is already disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _buffer = null;
                _readableBuffer = null;
                _readIndex = 0;
            }

            disposed = true;
        }
    }

    /// <summary>
    /// Dispose the packet, force garbage collection.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}