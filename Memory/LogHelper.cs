using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Memory
{
    static class LogHelper
    {
        public static void EncryptLog(string plainText)
        {
            /*
            string msg = plainText;
            ISyncOutput output = IO.CurrentIO;

#if !DEBUG
            string _public_key = @"<RSAKeyValue><Modulus>yAs66SUY9SqPiZcoriVGzbLkpHGzJcyhLustyfA6fNQjE8COalr6rnjgyI44hFSYkhpz6ThMjsnINLDPv23k6ZkPzQSXA7HyBDHUj6L8xf9YoypWjGlRbou6usynWfK525bzOomGaLFSmz8WN0KZgzfsP42oHBHcwv6DeWurwH2KZogYv8NDAACslizbApJET3oPFPdiO/PnwMOoPpXnJYSE00S23ZsEFkqj1eGOWnB7Xije/NDL1ijxSFn27YhT66dI64mluz1818LaaPDYvCHivkCKqhKdpJeDrYfOZiY2v2Hpn3hr/DUEM14vJTpBTDxBfG498X5j5J0gZDQ8gQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        
            output = IO.FileLogger;
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(_public_key);
                byte[] cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), false);
                msg = Convert.ToBase64String(cipherbytes);
            }
#endif

            output.Write($"{msg}");
            */

            //now is directly output to default in DebugMode
            Logger.Debug(plainText);
        }
    }
}
