using System.Security.Cryptography;
using Unity.Entities;

namespace uIota
{
    public class TransactionCreationSystem : ComponentSystem
    {
        ComponentGroup transactions;

        float currentTick;
        int requiredTicks;

        //private SHA256 hasher = SHA256.Create();
        private static byte[] genesisHash = new byte[] { 9, 9, 9, 9, 9, 9, 9, 9, 9, };
        private static char[] chars = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '9', };

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            var genesis = EntityManager.CreateEntity();

            EntityManager.AddBuffer<Hash>(genesis);
            var hashArray = new Hash[9];
            for (var i = 0; i < genesisHash.Length; i++)
            { hashArray[i].Value = genesisHash[i]; }
            var hashBuffer = EntityManager.GetBuffer<Hash>(genesis);
            hashBuffer.CopyFrom(hashArray);

            var timeStamps = new TimeStamps();
            timeStamps.TimeStamp = (long)-1;
            EntityManager.AddComponent(genesis, typeof(TimeStamps));
            EntityManager.SetComponentData(genesis, timeStamps);

            EntityManager.AddBuffer<Trunk>(genesis);
            EntityManager.AddBuffer<Branch>(genesis);
            EntityManager.AddComponent(genesis, typeof(HasTips));

            transactions = GetComponentGroup(typeof(Hash));

            requiredTicks = 3;
        }

        protected override void OnUpdate()
        {
            currentTick += UnityEngine.Time.deltaTime;
            if(currentTick >= requiredTicks)
            {
                currentTick = 0;
                var numberOfTx = UnityEngine.Random.Range(1, 5);
                for (var i = 0; i < numberOfTx; i++)
                {
                    CreateTransaction();
                }
            }
        }

        Entity CreateTransaction()
        {
            //var entity = EntityManager.CreateEntity(AppManager.BaseTransactionArchetype);
            var entity = EntityManager.CreateEntity();

            EntityManager.AddBuffer<Hash>(entity);
            byte[] hashBytes = new byte[81];
            //NativeArray<Hash> hashBytes;
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(hashBytes);
            }
            for (var i = 0; i < hashBytes.Length; i++)
            {
                hashBytes[i] = (byte)(hashBytes[i] % chars.Length);
            }

            var hashArray = new Hash[hashBytes.Length];
            for (var i = 0; i < hashBytes.Length; i++)
            {
                hashArray[i].Value = hashBytes[i];
            }
            var hashBuffer = EntityManager.GetBuffer<Hash>(entity);
            hashBuffer.CopyFrom(hashArray);

            //buffer.Clear();
            //buffer.Reinterpret<Hash>().AddRange(hashBytes);

            var timeStamps = new TimeStamps();
            timeStamps.TimeStamp = (long)UnityEngine.Time.realtimeSinceStartup;
            EntityManager.AddComponent(entity, typeof(TimeStamps));
            EntityManager.SetComponentData(entity, timeStamps);
            EntityManager.AddBuffer<Trunk>(entity);
            EntityManager.AddBuffer<Branch>(entity);
            return entity;
        }

        //byte[] GetHash(string message)
        //{
        //    return hasher.ComputeHash(Encoding.UTF8.GetBytes(message));
        //}

        //string GetString(byte[] data)
        //{
        //    var sBuilder = new StringBuilder();

        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        sBuilder.Append(data[i].ToString("x2"));
        //    }

        //    return sBuilder.ToString();
        //}
    }
}
