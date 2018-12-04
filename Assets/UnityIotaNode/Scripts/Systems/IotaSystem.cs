using Unity.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Collections;

namespace uIota
{
    //[DisableAutoCreation]
    public class IotaSystem : ComponentSystem
    {
        private SHA256 hasher = SHA256.Create();

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            transactionsGroup = GetComponentGroup(typeof(Transaction));
        }

        private ComponentGroup transactionsGroup;

        protected override void OnUpdate()
        {
            var transactions = transactionsGroup.GetComponentDataArray<Transaction>();
            UnityEngine.Debug.Log(transactions[transactions.Length - 1].Hash.ToString());
        }

        private string GetHash(string message)
        {
            byte[] data = hasher.ComputeHash(Encoding.UTF8.GetBytes(message));
            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
