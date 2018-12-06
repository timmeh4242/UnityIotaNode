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

            transactionsGroup = GetComponentGroup(typeof(Hash));
        }

        private ComponentGroup transactionsGroup;

        protected override void OnUpdate()
        {
            var transactions = transactionsGroup.GetBufferArray<Hash>();

            //UnityEngine.Debug.Log(transactions.Length);
            for(var i = 0; i < transactions.Length; i++)
            {
                //UnityEngine.Debug.Log(transactions[i].Length);
            }

            //UnityEngine.Debug.Log(transactions[transactions.Length - 1].Hash.ToString());

            //var aaa = "YT9CVQDZMIFXNAYXAPHIFGEMIEBVGXIZVPXCYFI9YSOJRVWKY9SNYPWNXQVHGLVZTFWMBLSWEIPVA9999";
            //var bbb = "YT9CVQDZMIFXNAYXAPHIFGEMIEBVGXIZVPX9PWl9YSOJRVWKY9SNYPWNXQVHGLVZTFWMBLSWEIPVA9999";
            //var ccc = "YT9CVQDZAIFXWAYX9PHIFGEMIEBVGXIZVPXCYFI9YSOJRVWKY9SNYPWNXQVHGLVZTFWMBLSWEIPVA9999";

            //UnityEngine.Debug.Log(GetHash(aaa));
            //UnityEngine.Debug.Log(GetHash(bbb));
            //UnityEngine.Debug.Log(GetHash(ccc));
        }

        private string GetHash(string message)
        {
            byte[] data = hasher.ComputeHash(Encoding.UTF8.GetBytes(message));
            var sBuilder = new StringBuilder();

            UnityEngine.Debug.Log(data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        private unsafe string GetString(byte* data)
        {
            var sBuilder = new StringBuilder();

            //UnityEngine.Debug.Log(data.Length);

            //for (int i = 0; i < data.Length; i++)
            //{
            //    sBuilder.Append(data[i].ToString("x2"));
            //}

            return sBuilder.ToString();
        }
    }
}
