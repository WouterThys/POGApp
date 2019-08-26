using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class IniFile
    {
        private string filePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public IniFile(string filePath)
        {
            this.filePath = filePath;
        }

        public void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }

        public string ReadValue(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", temp, 255, filePath);
            return temp.ToString();
        }

        #region READ
        public string ReadString(string section, string key)
        {
            return ReadValue(section, key);
        }

        public int ReadInt(string section, string key)
        {
            string val = ReadValue(section, key);
            int.TryParse(val, out int res);
            return res;
        }

        public long ReadLong(string section, string key)
        {
            string val = ReadValue(section, key);
            long.TryParse(val, out long res);
            return res;
        }

        public bool ReadBool(string section, string key)
        {
            string val = ReadValue(section, key);
            bool.TryParse(val, out bool res);
            return res;
        }
        #endregion

        #region WRITE
        public void WriteString(string section, string key, string value)
        {
            WriteValue(section, key, value);
        }

        public void WriteInt(string section, string key, int value)
        {
            WriteValue(section, key, value.ToString());
        }

        public void WriteLong(string section, string key, long value)
        {
            WriteValue(section, key, value.ToString());
        }

        public void WriteBool(string section, string key, bool value)
        {
            WriteValue(section, key, value.ToString());
        }
        #endregion


    }
}
