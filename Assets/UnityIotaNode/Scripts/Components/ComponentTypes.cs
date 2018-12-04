using Unity.Entities;

namespace uIota
{
    public struct Transaction : IComponentData
    {
        public int Hash;
        //public Hash Hash;
        //public Hash SignatureMessageFragment;
        //public Hash Address;
        public int Value;
        //public Hash ObsoleteTag;
        public int TimeStamp;
        public int CurrentIndex;
        public int LastIndex;

        //public Hash Bundle;
        //public Hash Trunk;
        //public Hash Branch;

        //public Hash Tag;

        public int AttachmentTimeStamp;
        public int AttachmentTimeStampLowerBound;
        public int AttachmentTimeStampUpperBound;
        //public Hash Nonce;

        //public bool Persistence;
    }

    //public struct Hash
    //{
    //    public string Value;
    //}
}
