using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Classic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DesDump
{
    class Program
    {
        // robot names from Descent 2 Workshop, Copyright (c) 2019 SaladBadger
        public static string[] robots = {"Medium Hulk", "Medium Lifter", "Spider Processer", "Class 1 Drone", "Class 2 Drone",
                    "Cloaked Driller", "Cloaked Medium Hulk", "Supervisor", "Secondary Lifter", "Heavy Driller",
                    "Gopher", "Laser Platform Robot", "Missile Platform Robot", "Splitter Pod", "Baby Spider",
                    "Fusion Hulk", "Supermech", "Level 7 Boss", "Cloaked Lifter", "Class 1 Driller", "Light Hulk",
                    "Advanced Lifter", "Defense Prototype", "Level 27 Boss", "BPER Bot", "Smelter", "Ice Spindle",
                    "Bulk Destroyer", "TRN Racer", "Fox Attack Bot", "Sidearm", "Red Fatty Boss", "New Boss",
                    "Guidebot", "Mine Guard", "Evil Twin", "ITSC Bot", "ITD Bot", "PEST", "PIG",
                    "Diamond Claw", "Hornet", "Thief Bot", "Seeker", "E-Bandit", "Fire Boss", "Water Boss",
                    "Boarshead", "Spider", "Omega Defense Spawn", "Sidearm Modula", "LOU Guard", "Ailen 1 Boss",
                    "Popcorn Miniboss", "Cloaked Diamond Claw", "Cloaked Smelter", "Guppy", "Smelter Clone",
                    "Omega Defense Spawn Clone", "BPER Bot Clone", "Spider Clone", "Spawn Clone", "Ice Boss", "Spawn",
                    "Final Boss", "Mini Reactor"};

        // poly names from LibDescent, Copyright (c) 2019 SaladBadger
        public static string[] d2polymodels = {"Medium Hulk", "Medium Hulk LOD", "Medium Lifter", "Medium Lifter LOD", "Spider Processor",
                      "Spider Processor LOD", "Class 1 Drone", "Class 1 Drone LOD", "Class 2 Drone", "Class 2 Drone LOD", "Cloaked Driller",
                      "Cloaked Driller LOD", "Cloaked Medium Hulk", "Cloaked Medium Hulk LOD", "Supervisor", "Secondary Lifter", "Secondary Lifter LOD",
                      "Heavy Driller", "Heavy Driller LOD", "Gopher", "Laser Platform Robot", "Missile Platform Robot", "Splitter Pod", "Baby Spider",
                      "Baby Spider LOD", "Fusion Hulk", "Supermech", "Supermech LOD", "Level 7 Boss", "Cloaked Lifter", "Cloaked Lifter LOD",
                      "Class 1 Driller", "Class 1 Driller LOD", "Light Hulk", "Light Hulk LOD", "Advanced Lifter", "Advanced Lifter LOD",
                      "Defense Prototype", "Defense Prototype LOD", "Level 27 Boss", "BPER Bot", "Smelter", "Smelter LOD", "Ice Spindle",
                      "Bulk Destroyer", "TRN Racer", "Fox Attack Bot", "Sidearm", "Sidearm LOD", "Red Fatty Boss", "New Boss", "Guidebot",
                      "Mine Guard", "Mine Guard LOD", "Evil Twin", "ITSC Bot", "ITD Bot", "ITD Bot LOD", "PEST Bot", "PEST LOD",
                      "PIG", "PIG Bot LOD", "Diamond Claw", "Diamond Claw LOD", "Hornet", "Thief Bot", "Thief Bot (LD)", "Seeker",
                      "E-Bandit", "Fire Boss", "Water Boss", "Boarshead", "Spider", "Omega Defense Spawn", "Sidearm Modula", "LOU Guard",
                      "Alien 1 Boss", "Popcorn Miniboss", "Cloaked Diamond Claw", "Cloaked Diamond Claw LOD", "Cloaked Smelter", "Cloaked Smelter LOD",
                      "Guppy", "Smelter Clone", "Smelter Clone LOD", "Omega Defense Spawn Clone", "BPER Bot Clone", "Spider Clone", "Spawn", "Ice Boss",
                      "Spawn Clone", "Final Boss", "Mini Reactor", "Descent 1 Reactor", "Descent 1 Reactor Destroyed", "Alien Reactor", "Ailen Reactor Destroyed",
                      "Zeta Aquilae Reactor", "Zeta Aquilae Reactor Destroyed", "Water Reactor", "Water Reactor Destroyed", "Ailen 1 Reactor",
                      "Ailen 1 Reactor Destroyed", "Fire Reactor", "Fire Reactor Destroyed", "Ice Reactor", "Ice Reactor Destroyed", "Marker", "Pyro GX",
                      "Pyro GX LOD", "Pyro GX Debris", "Red Laser", "Red Laser LOD", "Red Laser LOD 2", "Red Laser Core", "Purple Laser", "Purple Laser LOD",
                      "Purple Laser LOD 2", "Purple Laser Core", "Light Blue Laser", "Light Blue Laser LOD", "Light Blue Laser LOD2", "Light Blue Laser Core", "Green Laser",
                      "Green Laser LOD", "Green Laser LOD 2", "Green Laser Core", "Concussion Missile", "Flare", "Robot Blue Laser", "Robot Blue Laser Core",
                      "Fusion Blob", "Fusion Blob Core", "Homing Missile", "Smart Missile", "Mega Missile", "Robot Homing Missile", "Robot Concussion Missile", "Robot Red Laser",
                      "Robot Red Laser Core", "Robot Green Laser", "Robot Green Laser Core", "Robot Mega Missile", "Yellow Laser", "Yellow Laser LOD", "Yellow Laser LOD 2", "Yellow Laser Core",
                      "White Laser", "White Laser LOD", "White Laser LOD 2", "White Laser Core", "Flash Missile", "Guided Missile", "Mercury Missile", "Earthshaker Missile",
                      "Robot Vulcan", "Robot White Laser", "Robot White Laser Core", "Robot Flash Missile", "Mine", "Earthshaker Child", "Robot Mercury Missile", "Robot Smart Missile",
                      "Robot Earthshaker Missile", "Robot Earthshaker Missile Child", "Robot Homing Flash Missile"};

        public static string[] d1polymodels = {"Medium Hulk", "Medium Hulk LOD", "Medium Lifter", "Medium Lifter LOD", "Spider Processor",
                      "Spider Processor LOD", "Class 1 Drone", "Class 1 Drone LOD", "Class 2 Drone", "Class 2 Drone LOD", "Cloaked Driller",
                      "Cloaked Driller LOD", "Cloaked Medium Hulk", "Cloaked Medium Hulk LOD", "Supervisor", "Secondary Lifter", "Secondary Lifter LOD",
                      "Heavy Driller", "Heavy Driller LOD", "Gopher", "Laser Platform Robot", "Missile Platform Robot", "Splitter Pod", "Baby Spider",
                      "Baby Spider LOD", "Fusion Hulk", "Supermech", "Supermech LOD", "Level 7 Boss", "Cloaked Lifter", "Cloaked Lifter LOD",
                      "Class 1 Driller", "Light Hulk", "Light Hulk LOD", "Advanced Lifter", "Advanced Lifter LOD",
                      "Defense Prototype", "Defense Prototype LOD", "Level 27 Boss",
                      "Descent 1 Reactor", "Descent 1 Reactor Destroyed",
                      "Exit", "Exit Destroyed",
                      "Pyro GX", "Pyro GX LOD", "Pyro GX Debris",
                      "Red Laser", "Red Laser LOD", "Red Laser LOD 2", "Red Laser Core", "Purple Laser", "Purple Laser LOD",
                      "Purple Laser LOD 2", "Purple Laser Core", "Light Blue Laser", "Light Blue Laser LOD", "Light Blue Laser LOD2", "Light Blue Laser Core", "Green Laser",
                      "Green Laser LOD", "Green Laser LOD 2", "Green Laser Core", "Concussion Missile", "Flare", "Robot Blue Laser", "Robot Blue Laser Core",
                      "Fusion Blob", "Fusion Blob Core", "Homing Missile", "Smart Missile", "Mega Missile", "Robot Homing Missile", "Robot Concussion Missile", "Robot Red Laser",
                      "Robot Red Laser Core", "Robot Green Laser", "Robot Green Laser Core", "Robot Mega Missile"};

        static byte bit5to8(int x)
        {
            return (byte)((x << 3) | (x >> 2));
        }

        struct Color
        {
            public byte r, g, b, a;
            public Color(byte r, byte g, byte b, byte a)
            { 
                this.r = r; this.g = g; this.b = b; this.a = a;
            }
        }

        // img is in argb format
        public static void WritePng(string fn, byte[] img, int w, int h)
        {
            var fmt = PixelFormat.Format32bppArgb;
            Rectangle rect = new Rectangle(0, 0, w, h);
            using (Bitmap b = new Bitmap(w, h, fmt))
            {
                BitmapData d = b.LockBits(rect, ImageLockMode.ReadWrite, fmt);
                Marshal.Copy(img, 0, d.Scan0, img.Length);
                b.UnlockBits(d);
                b.Save(fn, ImageFormat.Png);
            }
        }

        public static void WritePigBitmapToPng(string fn, byte[] pal, Pig pig, PigBitmap bmp)
        {
            int w = bmp.width, h = bmp.height;
            byte[] img = new byte[w * h * 4];
            int dstOfs = 0;

            byte[] img8 = pig.GetBitmap(bmp);
            int srcOfs = 0;
            int th = bmp.height;
            for (int y = 0; y < th; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int colorIdx = img8[srcOfs++] * 3;
                    img[dstOfs++] = pal[colorIdx + 2];
                    img[dstOfs++] = pal[colorIdx + 1];
                    img[dstOfs++] = pal[colorIdx];
                    img[dstOfs++] = (colorIdx >= 254 * 3 ? (byte)0 : (byte)255);
                }
            }
            WritePng(fn, img, w, h);
        }

        void DumpModel(int modelNum, string outName)
        {
            PolyModel model = data.PolygonModels[modelNum];

            //spCol.radius = model.rad.ToFloat();

            var modelReader = new ModelReader();

            int texCount = model.n_textures;
            int matCount = texCount + 3; // allow 3 flat color materials
            var r = new BinaryReader(new MemoryStream(model.data));
            var flatColors = new List<int>();
            modelReader.Reset(matCount);
            modelReader.ReadModelData(r, flatColors, model.n_textures);

            bool colorPal = version == Classic.Version.D1;
            string outDir = Path.GetDirectoryName(outName);
            string mtlName = Path.ChangeExtension(outName, ".mtl");
            using (var f = new StreamWriter(outName))
            using (var fmtl = new StreamWriter(mtlName))
            {
                f.WriteLine("mtllib " + Path.GetFileName(mtlName));
                f.WriteLine("o " + Path.GetFileNameWithoutExtension(outName).Replace(' ', '_'));
                foreach (var vert in modelReader.Verts.ItemList)
                    f.WriteLine("v " + -vert.x.ToFloat() + " " + vert.y + " " + vert.z);
                foreach (var norm in modelReader.Norms.ItemList)
                    f.WriteLine("vn " + -norm.x.ToFloat() + " " + norm.y + " " + norm.z);
                foreach (var uv in modelReader.UVs.ItemList)
                    f.WriteLine("vt " + (uv.u.ToFloat()) + " " + (-uv.v.ToFloat()));
                for (var matIdx = 0; matIdx < matCount; matIdx++)
                {
                    if (!modelReader.faces[matIdx].Any())
                        continue;
                    string matName;
                    if (matIdx < texCount)
                    {
                        var bmpIdx = data.ObjBitmaps[data.ObjBitmapPtrs[model.first_texture + matIdx]].index - 1;
                        var bmp = pig.bitmaps[bmpIdx];
                        matName = bmp.name;
                        fmtl.WriteLine("newmtl " + matName);
                        fmtl.WriteLine("illum 2");
                        fmtl.WriteLine("Kd 1.00 1.00 1.00");
                        fmtl.WriteLine("Ka 0.00 0.00 0.00");
                        fmtl.WriteLine("Ks 0.00 0.00 0.00");
                        fmtl.WriteLine("d 1.0");
                        fmtl.WriteLine("map_Kd " + matName + ".png");
                        WritePigBitmapToPng(Path.Combine(outDir, matName + ".png"), pal, pig, bmp);
                    }
                    else
                    {
                        var color = flatColors[matIdx - texCount];
                        matName = "flat" + flatColors[matIdx - texCount];
                        var rgb = colorPal ? pal32[color] : new Color(bit5to8(color >> 10), bit5to8((color >> 5) & 31), bit5to8(color & 31), 255);
                        fmtl.WriteLine("newmtl " + matName);
                        fmtl.WriteLine("illum 2");
                        fmtl.WriteLine("Kd " + rgb.r / 255.0 + " " + rgb.g / 255.0 + " " + rgb.b / 255.0);
                        fmtl.WriteLine("Ka 0.00 0.00 0.00");
                        fmtl.WriteLine("Tf 1.00 1.00 1.00");
                        fmtl.WriteLine("Ni 0.00");
                    }
                    f.WriteLine("usemtl " + matName);
                    f.WriteLine("s off");
                    foreach (var face in modelReader.faces[matIdx])
                        f.WriteLine("f " + string.Join(" ", face.Reverse().Select(v => (v[0] + 1) + "/" + (v[2] == -1 ? "" : (v[2] + 1).ToString()) + "/" + (v[1] + 1))));
                }
            }
        }

        ClassicData data;
        Pig pig;
        byte[] pal;
        Color[] pal32;
        Classic.Version version;

        void Run(string[] args)
        {
            var dir = args.Length >= 1 ? args[0] : "";
            version = default(Classic.Version);
            string pigName = null;
            string palName = null;
            string hogName = null;
            string[] polyNames = null;

            if (File.Exists(Path.Combine(dir, "descent2.hog")) && File.Exists(Path.Combine(dir, "descent2.ham")) && File.Exists(Path.Combine(dir, "groupa.pig")))
            {
                version = Classic.Version.D2;
                hogName = "descent2.hog";
                pigName = "groupa.pig";
                palName = "groupa.256";
                polyNames = d2polymodels;
            }
            else if (File.Exists(Path.Combine(dir, "descent.hog")) && File.Exists(Path.Combine(dir, "descent.pig")))
            {
                version = Classic.Version.D1;
                hogName = "descent.hog";
                pigName = "descent.pig";
                palName = "palette.256";
                polyNames = d1polymodels;
            }
            if (version == Classic.Version.UNKNOWN)
            {
                Console.WriteLine("No Descent 1 / Descent 2 data files found in " + dir);
                return;
            }
            var hog = new Hog(Path.Combine(dir, hogName));

            byte[] vgaPal = hog.ItemData(palName);
            pal = ClassicLoader.VgaPalConv(vgaPal);
            pal32 = new Color[256];
            for (int i = 0; i < 256; i++)
                pal32[i] = new Color(pal[i * 3], pal[i * 3 + 1], pal[i * 3 + 2], 255);

            pig = new Pig(Path.Combine(dir, pigName));
            if (version == Classic.Version.D2)
            {
                byte[] bytes = File.ReadAllBytes(Path.Combine(dir, "descent2.ham"));
                data = new ClassicData();
                data.Read(new BinaryReader(new MemoryStream(bytes, 8, bytes.Length - 8)), version);
            }
            else
            {
                pig.ReadTableData(out data);
            }

            var outDir = args.Length >= 2 ? args[1] : "";
            //for (int robotIdx = 0; robotIdx < data.N_robot_types; robotIdx++)
            //    DumpModel(data.RobotInfo[robotIdx].model_num, Path.Combine(outDir, robots[robotIdx].Replace(' ', '_') + ".obj"));
            for (int modelIdx = 0; modelIdx < data.N_polygon_models; modelIdx++)
            {
                string name = modelIdx < polyNames.Length ? polyNames[modelIdx] : "model_" + modelIdx.ToString("000");
                DumpModel(modelIdx, Path.Combine(outDir, name.Replace(' ', '_') + ".obj"));
            }
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            new Program().Run(args);
        }
    }
}
