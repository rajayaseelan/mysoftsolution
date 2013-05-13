using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace MySoft.Security
{
    /// <summary> 
    /// AES加密
    /// </summary> 
    public static class AES
    {
        private static byte[] Keys = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79, 0x53, 0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };

        /// <summary>
        /// AES加密字符串
        /// </summary>
        /// <param name="encryptString"></param>
        /// <param name="encryptKey"></param>
        /// <returns></returns>
        public static string Encrypt(string encryptString, string encryptKey)
        {
            encryptKey = CoreHelper.GetSubString(encryptKey, 32, "");
            encryptKey = encryptKey.PadRight(32, ' ');

            RijndaelManaged rijndaelProvider = new RijndaelManaged();
            rijndaelProvider.Key = Encoding.UTF8.GetBytes(encryptKey);
            rijndaelProvider.IV = Keys;
            ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();

            byte[] inputData = Encoding.UTF8.GetBytes(encryptString);
            byte[] encryptedData = rijndaelEncrypt.TransformFinalBlock(inputData, 0, inputData.Length);

            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// AES解密字符串
        /// </summary>
        /// <param name="decryptString"></param>
        /// <param name="decryptKey"></param>
        /// <returns></returns>
        public static string Decrypt(string decryptString, string decryptKey)
        {
            decryptKey = CoreHelper.GetSubString(decryptKey, 32, "");
            decryptKey = decryptKey.PadRight(32, ' ');

            RijndaelManaged rijndaelProvider = new RijndaelManaged();
            rijndaelProvider.Key = Encoding.UTF8.GetBytes(decryptKey);
            rijndaelProvider.IV = Keys;
            ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();

            byte[] inputData = Convert.FromBase64String(decryptString);
            byte[] decryptedData = rijndaelDecrypt.TransformFinalBlock(inputData, 0, inputData.Length);

            return Encoding.UTF8.GetString(decryptedData);
        }

        #region 加密解密

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="inputdata">输入的数据</param>
        /// <param name="iv">向量128位</param>
        /// <param name="strKey">加密密钥</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] inputdata, byte[] iv, string strKey)
        {
            //分组加密算法   
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputByteArray = inputdata;//得到需要加密的字节数组       
            //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(strKey.Substring(0, 32));
            des.IV = iv;
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                byte[] cipherBytes = ms.ToArray();//得到加密后的字节数组   
                return cipherBytes;
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="inputdata">输入的数据</param>
        /// <param name="iv">向量128</param>
        /// <param name="strKey">key</param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] inputdata, byte[] iv, string strKey)
        {
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(strKey.Substring(0, 32));
            des.IV = iv;
            byte[] decryptBytes = new byte[inputdata.Length];
            using (MemoryStream ms = new MemoryStream(inputdata))
            using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
            {
                cs.Read(decryptBytes, 0, decryptBytes.Length);
                return decryptBytes;
            }
        }


        #endregion
    }
}
