using System.IO;
using UnityEngine;

public class ApiKey : MonoBehaviour
{
    public static string key = null;

    public static string GetApiKey()
    {
        string fileName = "key.txt";
        if (key == null)
        {
            if (File.Exists(fileName))
            {
                key = File.ReadAllText(fileName);
                Debug.Log("Successfully read in API key");
            }
            else
                Debug.LogError("Failed to read in API key: " + fileName + " does not exist");
        }
        return key;
    }
}
