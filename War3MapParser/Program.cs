using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace War3MapParser
{
    class Program
    {

        public const int intWar3HeaderSize = 512; // Bytes
        public const String strMapPath = "C:\\Program Files (x86)\\Warcraft III\\Maps";

        public void War3MapParser()
        {
        }

        static void Main(string[] args)
        {

            IEnumerable<String> iMaps = Directory.EnumerateFiles(strMapPath, "*.w3m");

            foreach (String strMap in iMaps)
            {
                Console.WriteLine("FILE: " + strMap);

                byte[] buffer = ReadHeader(strMap);

                War3Map map = new War3Map(buffer);

                try
                {
                    Console.Write('\t' + "MAP NAME: " + '\t');
                    WriteLineWithColor(map.Name, ConsoleColor.Cyan);
                    Console.Write('\t' + "MAX PLAYERS:" + '\t');
                    WriteLineWithColor(map.MaxPlayers.ToString(), ConsoleColor.Cyan);

                    if (map.Name == "Booty Bay")
                    {
                        map.WriteMapTGAToFile();
                    }

                }
                catch (Exception ex)
                {
                    WriteWithColor("ERROR: ", ConsoleColor.Red);
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine("DONE.");
            PromptEnd();
            return;

        }

        public static void PromptEnd()
        {
            Console.WriteLine("\nPress ENTER to end...");
            Console.ReadLine();
        }

        public static byte[] ReadHeader(String strFilePath)
        {
            BinaryReader rdrMap = new BinaryReader(File.Open(strFilePath, FileMode.Open, FileAccess.Read));
            Byte[] buffer = new Byte[rdrMap.BaseStream.Length];

            for (int i = 0; i < rdrMap.BaseStream.Length; i++)
            {
                buffer[i] = rdrMap.ReadByte();
            }

            rdrMap.Close();

            return buffer;
        }

        public static void WriteWithColor(String str, ConsoleColor clr)
        {
            ConsoleColor current = Console.ForegroundColor;
            Console.ForegroundColor = clr;
            Console.Write(str);
            Console.ForegroundColor = current;
        }

        public static void WriteLineWithColor(String str, ConsoleColor clr)
        {
            ConsoleColor current = Console.ForegroundColor;
            Console.ForegroundColor = clr;
            Console.WriteLine(str);
            Console.ForegroundColor = current;
        }

    }
}
