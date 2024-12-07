using UnityEngine;

namespace Utils
{
    public class LogUtil
    {
        
        private static string PREFIX = "[LemonAP] ";
        
        public static void Info(string message)
        {
            Debug.Log(PREFIX + message);
        }

        public static void Error(string message)
        {
            Debug.LogError(PREFIX + message);
        }

        public static void Warning(string message)
        {
            Debug.LogWarning(PREFIX + message);
        }
    }
}