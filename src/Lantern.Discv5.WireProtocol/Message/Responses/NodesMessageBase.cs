using Lantern.Discv5.Enr;
using Lantern.Discv5.Enr.Factory;
using Lantern.Discv5.Rlp;

namespace Lantern.Discv5.WireProtocol.Message.Responses;

public class NodesMessageBase : MessageBase
{
    public NodesMessageBase(int total, EnrRecord[] enrs) : base(Discv5.WireProtocol.Message.MessageType.Nodes)
    {
        Total = total;
        Enrs = enrs;
    }

    private NodesMessageBase(byte[] requestId, int total, EnrRecord[] enrs) : base(Discv5.WireProtocol.Message.MessageType.Nodes)
    {
        RequestId = requestId;
        Total = total;
        Enrs = enrs;
    }

    public int Total { get; }

    public EnrRecord[] Enrs { get; }

    public override byte[] EncodeMessage()
    {
        var messageId = new[] { MessageType };
        var encodedRequestId = RlpEncoder.EncodeBytes(RequestId);
        var encodedTotal = RlpEncoder.EncodeInteger(Total);

        using var stream = new MemoryStream();
        foreach (var enr in Enrs) stream.Write(enr.EncodeEnrRecord());

        var encodedEnrs = RlpEncoder.EncodeCollectionOfBytes(stream.ToArray());
        var encodedItems =
            RlpEncoder.EncodeCollectionOfBytes(ByteArrayUtils.Concatenate(encodedRequestId, encodedTotal,
                encodedEnrs));
        return ByteArrayUtils.Concatenate(messageId, encodedItems);
    }

    public static NodesMessageBase DecodeMessage(byte[] message)
    {
        var rawMessage = message[1..];
        var decodedMessage = RlpDecoder.Decode(rawMessage);
        var requestId = decodedMessage[0];
        var total = RlpExtensions.ByteArrayToInt32(decodedMessage[1]);
        var enrs = new EnrRecordFactory().CreateFromMultipleEnrList(ExtractEnrRecord(decodedMessage.Skip(2).ToList(), total));
        return new NodesMessageBase(requestId, total, enrs);
    }

    private static List<List<byte[]>> ExtractEnrRecord(IReadOnlyList<byte[]> data, int total)
    {
        var list = new List<List<byte[]>>(total);

        for (var i = 0; i < data.Count; i++)
        {
            // If the ENR uses a different identity scheme then the signature length could be different.
            // Add a check first if the identity scheme is v4, then check if the length is 64.
            if (data[i].Length != 64) continue;

            var subList = new List<byte[]>();

            for (var j = i; j < data.Count; j++)
            {
                if (i != j && data[j].Length == 64)
                    break;

                subList.Add(data[j]);
            }

            list.Add(subList);
        }

        return list;
    }
}