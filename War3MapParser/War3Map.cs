using MpqLib.Mpq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace War3MapParser
{
    class War3Map
    {

        #region Properties
        private byte[] buffer;
        private String _name = "";                          // 0x8, Variable Length
        private int _maxplayers =  -1;                      // name + 0x5

        private bool _hideMiniMapInPreview = false;         // name + 0x1, Bit 1
        private bool _modifyAllyPriorities = false;         // name + 0x1, Bit 2
        private bool _meleeMap = false;                     // name + 0x1, Bit 3
        private bool _largeNeverReduced = false;            // name + 0x1, Bit 4

        private bool _maskedPartiallyVisible = false;       // name + 0x2, Bit 1
        private bool _fixedPlayerCustomForces = false;      // name + 0x2, Bit 2
        private bool _customForces = false;                 // name + 0x2, Bit 3
        private bool _customTechTree = false;               // name + 0x2, Bit 4

        private bool _customAbilities = false;              // name + 0x3, Bit 1
        private bool _customUpgrades = false;               // name + 0x3, Bit 2
        private bool _propertiesOpenedPostCreate = false;   // name + 0x3, Bit 3
        private bool _waterOnCliffShores = false;           // name + 0x3, Bit 4

        private bool _waterOnRollingShores = false;         // name + 0x4, Bit 1

        private int indxMapNameEnd = -1;
        
        #endregion

        #region Accessors
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public int MaxPlayers
        {
            get
            {
                return _maxplayers;
            }
            set
            {
                _maxplayers = value;
            }
        }
        public bool HideMiniMapInPreview
        {
            get
            {
                return _hideMiniMapInPreview;
            }
            set
            {
                _hideMiniMapInPreview = value;
            }
        }
        public bool ModifyAllyPriorities
        {
            get
            {
                return _modifyAllyPriorities;
            }
            set
            {
                _modifyAllyPriorities = value;
            }
        }
        public bool MeleeMap
        {
            get
            {
                return _meleeMap;
            }
            set
            {
                _meleeMap = value;
            }
        }
        public bool LargeNeverReducedToMedium
        {
            get
            {
                return _largeNeverReduced;
            }
            set
            {
                _largeNeverReduced = value;
            }
        }
        public bool MaskedAreasPartiallyVisible
        {
            get
            {
                return _maskedPartiallyVisible;
            }
            set
            {
                _maskedPartiallyVisible = value;
            }
        }
        public bool FixedPlayerCustomForces
        {
            get
            {
                return _fixedPlayerCustomForces;
            }
            set
            {
                _fixedPlayerCustomForces = value;
            }
        }
        public bool CustomForces
        {
            get
            {
                return _customForces;
            }
            set
            {
                _customForces = value;
            }
        }
        public bool CustomTechTree
        {
            get
            {
                return _customTechTree;
            }
            set
            {
                _customTechTree = value;
            }
        }
        public bool CustomAbilities
        {
            get
            {
                return _customAbilities;
            }
            set
            {
                _customAbilities = value;
            }
        }
        public bool CustomUpgrades
        {
            get
            {
                return _customUpgrades;
            }
            set
            {
                _customUpgrades = value;
            }
        }
        public bool PropertiesWindowOpened
        {
            get
            {
                return _propertiesOpenedPostCreate;
            }
            set
            {
                _propertiesOpenedPostCreate = value;
            }
        }
        public bool WaterWavesOnCliffs
        {
            get
            {
                return _waterOnCliffShores;
            }
            set
            {
                _waterOnCliffShores = value;
            }
        }
        public bool WaterWavesOnRolling
        {
            get
            {
                return _waterOnRollingShores;
            }
            set
            {
                _waterOnRollingShores = value;
            }
        }
        #endregion

        public War3Map(byte[] buffer)
        {
            this.buffer = buffer;
            this.Verify();
            this.ParseParameters();
        }

        public bool Verify()
        {
            byte[] preable = new byte[4];
            Array.Copy(buffer, preable, 4);
            String strPreamble = new String(Encoding.ASCII.GetChars(preable));
            return strPreamble == "HM3W";
        }

        private void ParseParameters()
        {
            indxMapNameEnd = GetNextTermination(0x8);
            this.Name = new String(Encoding.ASCII.GetChars(buffer.Slice(0x8,indxMapNameEnd)));
            this.MaxPlayers = BitConverter.ToInt32(buffer, indxMapNameEnd + 0x5);
        }

        private int GetNextTermination(int indxStart)
        {
            for (int i = indxStart; i < buffer.Length; i++)
            {
                if (buffer[i] == 0x0)
                {
                    return i;
                }
            }

            return -1;
        }

        public String WriteMPQToFile()
        {
            try
            {
                byte[] mpq = buffer.Slice<byte>(512, buffer.Length - 1); // In a W3M, there's no suffix with the SHA-1 hash and "NGIS".
                BinaryWriter binw = new BinaryWriter(File.Open(this.Name + ".mpq", FileMode.Create, FileAccess.Write));
                Console.WriteLine('\t' + "INFO: Writing " + this.Name);
                binw.Write(mpq);
                binw.Flush();
                binw.Close();
                binw.Dispose();

            }
            catch (Exception ex)
            {
                Program.WriteWithColor("ERROR: ", ConsoleColor.Red);
                Console.WriteLine(ex.Message);
                return "";
            }
            return this.Name + ".mpq";

        }

        public String WriteMapTGAToFile()
        {

            String strMPQ = WriteMPQToFile();

            try
            {
                using (CArchive mpq = new CArchive(Environment.CurrentDirectory + "\\" + strMPQ, false))
                {
                    if (mpq.FileExists("war3map.wpm"))
                    {
                        mpq.ExportFile("war3map.wpm", this.Name + ".wpm.tmp");
                    }
                    else
                    {
                        return "";
                    }
                }

                // Delete the extracted MPQ; we're done with it.
                if(File.Exists(this.Name + ".mpq"))
                {
                    //File.Delete(this.Name + ".mpq");
                }

                // Get map dimensions for INFO output.
                BinaryReader bindim = new BinaryReader(File.Open(this.Name + ".wpm.tmp", FileMode.Open, FileAccess.Read));
                byte[] header = new byte[0x10];
                for (int i = 0; i < header.Length; i++)
                {
                    header[i] = bindim.ReadByte();
                }
                bindim.Close();
                bindim.Dispose();
                int intMapWidth = BitConverter.ToInt32(header.Slice<byte>(0x8, 0xC), 0);
                int intMapHeight = BitConverter.ToInt32(header.Slice<byte>(0xC, 0x10), 0);
                Console.Write('\t' + "DIMENSIONS:" + '\t');
                Program.WriteLineWithColor(intMapWidth.ToString() + " x " + intMapHeight.ToString(), ConsoleColor.Cyan);

                // Extract the real TGA from WPM File HERE.

            }
            catch (Exception ex)
            {
                Program.WriteWithColor("ERROR: ", ConsoleColor.Red);
                Console.WriteLine(ex.Message);
                return "";
            }
            return this.Name + ".tga";
        }

    }

    public static class Extensions
    {
        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }
    }

}