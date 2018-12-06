﻿using Unity.Collections;
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

    public struct Trunk : ISharedComponentData
    {
        public byte Value;
    }

    public struct Branch : ISharedComponentData
    {
        public byte Value;
    }

    public struct TimeStamps : IComponentData
    {
        public long TimeStamp;
        public long LowerBound;
        public long UpperBound;
    }

    public struct TransactionValue : IComponentData
    {
        public long Value;
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
