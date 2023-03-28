using System.Text;
using Lantern.Discv5.Rlp;

namespace Lantern.Discv5.Enr.Content.EntryTypes;

public class EntryTcp : IContentEntry
{
    public EntryTcp(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public string Key => EnrContentKey.Tcp;

    public IEnumerable<byte> EncodeEntry()
    {
        return ByteArrayUtils.JoinByteArrays(RlpEncoder.EncodeString(Key, Encoding.ASCII),
            RlpEncoder.EncodeInteger(Value));
    }
}