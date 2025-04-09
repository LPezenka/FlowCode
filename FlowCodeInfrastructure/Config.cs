using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCodeInfrastructure
{
    public static class Config
    {
        public static void Load()
        {
            // Load configuration settings
            // This is a placeholder for actual configuration loading logic
            Console.WriteLine("Loading configuration...");
        }

        public static void Save()
        {
            // Save configuration settings
            // This is a placeholder for actual configuration saving logic
            Console.WriteLine("Saving configuration...");
        }

        public enum KeyWord
        {
            Function,
            None,
            If,
            Else,
            While,
            For,
            Do,
            Switch,
            Case,
            Break,
            Continue,
            Return,
            True,
            False
        }

        public static Dictionary<KeyWord, string> KeywordMapper = new Dictionary<KeyWord, string>();


        public static string GetKeyword(KeyWord key)
        {
            if (KeywordMapper.ContainsKey(key))
            {
                return KeywordMapper[key];
            }
            else
            {
                return key.ToString();
            }
        }

        public static void SetKeyWord(KeyWord key, string value)
        {
            if (KeywordMapper.ContainsKey(key))
            {
                KeywordMapper[key] = value;
            }
            else
            {
                KeywordMapper.Add(key, value);
            }
        }
    }
}
