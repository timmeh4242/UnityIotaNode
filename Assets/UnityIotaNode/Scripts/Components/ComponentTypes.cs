using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace uIota
{
    //public struct Transaction : IComponentData
    //{
    //    public Hash Hash;
    //    public Hash SignatureMessageFragment;
    //    public Hash Address;
    //    public int Value;
    //    public Hash ObsoleteTag;
    //    public int TimeStamp;
    //    public int CurrentIndex;
    //    public int LastIndex;

    //    public Hash Bundle;
    //    public Hash Trunk;
    //    public Hash Branch;

    //    public Hash Tag;

    //    public int AttachmentTimeStamp;
    //    public int AttachmentTimeStampLowerBound;
    //    public int AttachmentTimeStampUpperBound;
    //    public Hash Nonce;

    //    public bool2 Persistence;
    //}

    //public struct Hash
    //{
    //    public string Value;
    //}

    //public struct Transaction : IComponentData { }
    //TODO -> replace this with instead dynamically adding Trunk and Branch components...
    //... however, we don't do this at the moment as there seem to be performance losses with AddComponent and RemoveComponent...
    //... vs just transforming the data
    public struct HasTips : IComponentData { }

    public struct Hash : IBufferElementData
    {
        public byte Value;
    }

    public struct SignatureMessageFragment : IBufferElementData
    {
        public byte Value;
    }

    public struct Address : IBufferElementData
    {
        public byte Value;
    }

    public struct IotaValue : IComponentData
    {
        public long Value;
    }

    public struct ObsoleteTag : IBufferElementData
    {
        public byte Value;
    }

    public struct Nonce : IBufferElementData
    {
        public byte Value;
    }

    public struct Bundle : IBufferElementData
    {
        public byte Value;
    }

    public struct Trunk : IBufferElementData
    {
        public byte Value;
    }

    public struct Branch : IBufferElementData
    {
        public byte Value;
    }

    public struct TimeStamps : IComponentData
    {
        public long TimeStamp;
        public long LowerBound;
        public long UpperBound;
    }

    public struct TransactionIndices : IComponentData
    {
        public long Current;
        public long Last;
    }

    public struct Persistence : IComponentData
    {
        public bool Value;
    }
}
