using System;
using System.Collections.Generic;
using System.Text;

namespace ASTool.Decrypt
{
    public class AESCTR
    {
        Aes128CounterMode m_am;
        CounterModeCryptoTransform m_ct;
        byte[] m_key;
        byte[] m_kid;
        bool m_bEncrypt;
        private AESCTR(byte[] key, byte[] kid)
        {
            if (key != null)
            {

                m_key = new byte[key.Length];
                key.CopyTo(m_key, 0);
            }
            if (kid != null)
            {
                m_kid = new byte[kid.Length];
                kid.CopyTo(m_kid, 0);
            }
            // Useful information in the kid limited to 8 bytes
            for (int i = 8; (i < 16) && (i < m_kid.Length); i++)
                m_kid[i] = 0;



        }
        private bool CreateEncryptor()
        {
            byte[] kid = new byte[m_kid.Length];
            m_kid.CopyTo(kid, 0);
            m_am = new Aes128CounterMode(kid);
            if (m_am != null)
            {

                m_ct = (CounterModeCryptoTransform)m_am.CreateEncryptor(m_key, null);
                if (m_ct != null)
                    return true;
            }
            return false;
        }
        private bool CreateDecryptor()
        {
            byte[] kid = new byte[m_kid.Length];
            m_kid.CopyTo(kid, 0);
            m_am = new Aes128CounterMode(kid);
            if (m_am != null)
            {

                m_ct = (CounterModeCryptoTransform)m_am.CreateDecryptor(m_key, null);
                if (m_ct != null)
                    return true;
            }
            return false;
        }
        public static AESCTR CreateEncryptor(byte[] key, byte[] kid)
        {
            AESCTR aesenc = new AESCTR(key,kid);
            if(aesenc!=null)
            {
                aesenc.m_bEncrypt = true;
                return aesenc ;
            }
            return null;
        }
        public static AESCTR CreateDecryptor(byte[] key, byte[] kid)
        {

            AESCTR aesenc = new AESCTR(key, kid);
            if (aesenc != null)
            {
                aesenc.m_bEncrypt = false;
                return aesenc;
            }
            return null;
        }
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (m_bEncrypt)
                CreateEncryptor();
            else
                CreateDecryptor();
            if (m_ct!=null)
            {
                return m_ct.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }
            return 0;
        }
    }
}
