using System.Security.Cryptography;
using System.Text;

namespace MySoft.Common
{
    /// <summary>
    /// SymmCrypto : 实现.Net框架下的带密钥的加密/解密算法封装类。
    /// </summary>
    public class SymmCrypto
    {
        /// <summary>
        /// 加密/解密算法的方式
        /// </summary>
        public enum SymmProvEnum : int
        {
            /// <summary>
            /// DES算法
            /// </summary>
            DES,

            /// <summary>
            /// RC2算法
            /// </summary>
            RC2,

            /// <summary>
            /// Rijndael算法
            /// </summary>
            Rijndael
        }

        private SymmetricAlgorithm mobjCryptoService;

        /// <remarks> 
        /// 使用.Net SymmetricAlgorithm 类的构造器. 
        /// </remarks> 
        public SymmCrypto(SymmProvEnum NetSelected)
        {
            switch (NetSelected)
            {
                case SymmProvEnum.DES:
                    mobjCryptoService = new DESCryptoServiceProvider();
                    break;
                case SymmProvEnum.RC2:
                    mobjCryptoService = new RC2CryptoServiceProvider();
                    break;
                case SymmProvEnum.Rijndael:
                    mobjCryptoService = new RijndaelManaged();
                    break;
            }
        }

        /// <remarks> 
        /// 使用自定义SymmetricAlgorithm类的构造器. 
        /// </remarks> 
        public SymmCrypto(SymmetricAlgorithm ServiceProvider)
        {
            mobjCryptoService = ServiceProvider;
        }

        /// <remarks> 
        /// Depending on the legal key size limitations of  
        /// a specific CryptoService provider and length of  
        /// the private key provided, padding the secret key  
        /// with space character to meet the legal size of the algorithm. 
        /// </remarks> 
        private byte[] GetLegalKey(string Key)
        {
            string sTemp;
            if (mobjCryptoService.LegalKeySizes.Length > 0)
            {
                int lessSize = 0, moreSize = mobjCryptoService.LegalKeySizes[0].MinSize;
                // key sizes are in bits 
                while (Key.Length * 8 > moreSize)
                {
                    lessSize = moreSize;
                    moreSize += mobjCryptoService.LegalKeySizes[0].SkipSize;
                }
                sTemp = Key.PadRight(moreSize / 16, ' ');
            }
            else
                sTemp = Key;

            // convert the secret key to byte array 
            return Encoding.UTF8.GetBytes(sTemp);
        }


        /// <summary>
        /// 对字符串进行密钥加密
        /// </summary>
        public string Encrypting(string Source, string Key)
        {
            byte[] bytIn = System.Text.ASCIIEncoding.ASCII.GetBytes(Source);
            // create a MemoryStream so that the process can be done without I/O files 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            byte[] bytKey = GetLegalKey(Key);

            // set the private key 
            mobjCryptoService.Key = bytKey;
            mobjCryptoService.IV = bytKey;

            // create an Encryptor from the Provider Service instance 
            ICryptoTransform encrypto = mobjCryptoService.CreateEncryptor();

            // create Crypto Stream that transforms a stream using the encryption 
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);

            // write out encrypted content into MemoryStream 
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();

            // get the output and trim the '\0' bytes 
            byte[] bytOut = ms.GetBuffer();
            int i = 0;
            for (i = 0; i < bytOut.Length; i++)
                if (bytOut[i] == 0)
                    break;

            // convert into Base64 so that the result can be used in xml 
            return System.Convert.ToBase64String(bytOut, 0, i);
        }

        /// <summary>
        /// 对字符串进行密钥解密
        /// </summary>
        public string Decrypting(string Source, string Key)
        {
            // 将 Base64 转化为二进制
            byte[] bytIn = System.Convert.FromBase64String(Source);

            // 为其分配内存空间
            System.IO.MemoryStream ms = new System.IO.MemoryStream(bytIn, 0, bytIn.Length);

            byte[] bytKey = GetLegalKey(Key);

            // 设置解密密钥
            mobjCryptoService.Key = bytKey;
            mobjCryptoService.IV = bytKey;

            // create a Decryptor from the Provider Service instance
            ICryptoTransform encrypto = mobjCryptoService.CreateDecryptor();

            // create Crypto Stream that transforms a stream using the decryption
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);

            // read out the result from the Crypto Stream
            System.IO.StreamReader sr = new System.IO.StreamReader(cs);
            return sr.ReadToEnd();
        }
    }

}
