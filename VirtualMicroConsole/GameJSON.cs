using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace VirtualMicroConsole
{
    [DataContract]
    public class GameJSON
    {
        [DataMember]
        public BinaryTexture[] Textures;

        [DataMember]
        public List<int[][]> Maps;

        [DataMember]
        public string FileCodePath;

        [DataMember]
        public Dictionary<int, int[]> Flags;

    }

    [DataContract]
    public class UserJSON
    {
        [DataMember]
        public BinaryTexture[] Textures;

        [DataMember]
        public List<int[][]> Maps;

        [DataMember]
        public Dictionary<int, int[]> Flags;
    }

    public static class SaveSystem
    {
        private static string Path { get => Directory.GetCurrentDirectory() + "/save"; }
        public static string UserPath;

        public static void LoadTextures(ref BinaryTexture[] textures)
        {
            if (File.Exists(UserPath))
            {
                UserJSON json;
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(UserPath)));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(UserJSON));
                json = (UserJSON)ser.ReadObject(stream);
                if (json.Textures != null) textures = json.Textures;
                else textures = new BinaryTexture[0];
            }
            else
            {
                textures = new BinaryTexture[0];
            }
        }
        public static void LoadMaps(ref List<int[,]> maps)
        {
            if (File.Exists(UserPath))
            {
                UserJSON json;
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(UserPath)));
                //Debug.WriteLine(openFileDialog.FileName);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(UserJSON));
                json = (UserJSON)ser.ReadObject(stream);
                if (json.Maps != null) maps = ConvertToMatrice(json.Maps);
                else maps = null;
            }
            else
            {
                maps = null;
            }
        }
        public static void LoadFileCodePath(ref string path)
        {
            GameJSON json;
            if (File.Exists(Path))
            {
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(Path)));
                //Debug.WriteLine(openFileDialog.FileName);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GameJSON));
                json = (GameJSON)ser.ReadObject(stream);
                path = json.FileCodePath;
            }
            else path = "";                 
        }
        public static void LoadFlags(ref Dictionary<int, int[]> flags)
        {
            if (File.Exists(UserPath))
            {
                UserJSON json;
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(UserPath)));
                //Debug.WriteLine(openFileDialog.FileName);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(UserJSON));
                json = (UserJSON)ser.ReadObject(stream);
                if (json.Maps != null) flags = json.Flags;
            }
            else
            {
                flags = null;
            }
        }

        public static void SaveUser(BinaryTexture[] textures, List<int[,]> maps, Dictionary<int, int[]> flags)
        {
            // User save
            var userJson = new UserJSON();
            userJson.Maps = ConvertToJagged(maps);
            userJson.Textures = textures;
            userJson.Flags = flags;
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(UserJSON));
                serializer.WriteObject(stream, userJson);

                stream.Position = 0; // 👈 Revenir au début du stream (pas 2 !)

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string jsonText = reader.ReadToEnd();
                    File.WriteAllText(UserPath, jsonText, Encoding.UTF8); // 👈 UTF8 garanti
                }
            }
        }
        public static void SaveGame(string fileCodePath)
        {
            var gameJson = new GameJSON();
            gameJson.FileCodePath = fileCodePath;


            // Game save
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(GameJSON));
                serializer.WriteObject(stream, gameJson);

                stream.Position = 0; // 👈 Revenir au début du stream (pas 2 !)

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string jsonText = reader.ReadToEnd();
                    File.WriteAllText(Path, jsonText, Encoding.UTF8); // 👈 UTF8 garanti
                }
            }
        }

        public static List<int[][]> ConvertToJagged(List<int[,]> data)
        {
            List<int[][]> convert = new List<int[][]>();
            for (int i = 0; i < data.Count; i++)
            {
                convert.Add(new int[data[i].GetLength(0)][]);
                for (int l = 0; l < data[i].GetLength(0); l++)
                {
                    convert[i][l] = new int[data[i].GetLength(1)];
                    for (int c = 0; c < data[i].GetLength(1); c++)
                    {
                        convert[i][l][c] = data[i][l, c];
                    }
                }
            }
            return convert;
        }
        public static List<int[,]> ConvertToMatrice(List<int[][]> data)
        {
            List<int[,]> convert = new List<int[,]>();

            for (int i = 0; i < data.Count; i++)
            {
                convert.Add(new int[data[i].Length, data[i].Length]);
                for (int l = 0; l < data[i].GetLength(0); l++)
                {
                    for (int c = 0; c < data[i].GetLength(0); c++)
                    {
                        convert[i][l, c] = data[i][l][c];
                    }
                }
            }

            return convert;
        }

    }
}
