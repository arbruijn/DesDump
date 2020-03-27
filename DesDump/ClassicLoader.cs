using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.Runtime.InteropServices;
using System.Globalization;
using System.Threading;

namespace Classic
{
class Debug
{
    public static void Log(string msg)
    {
        System.Diagnostics.Debug.WriteLine(msg);
        //UnityEngine.Debug.Log(msg);
    }
}
class Bits
{
    public static int GetInt32(byte[] a, int ofs)
    {
        return (int)a[ofs] + ((int)a[ofs + 1] << 8) +
                ((int)a[ofs + 2] << 16) + ((int)a[ofs + 3] << 24);
    }
    public static int GetUInt16(byte[] a, int ofs)
    {
        return (int)a[ofs] + ((int)a[ofs + 1] << 8);
    }
    public static int GetInt16(byte[] a, int ofs)
    {
        int n = GetUInt16(a, ofs);
        if (n >= 32768)
            n -= 65536;
        return n;
    }
    public static string GetString(byte[] a, int ofs, int maxLen)
    {
        int len = 0;
        while (len < maxLen && a[ofs + len] != 0)
            len++;
        byte[] text = new byte[len];
        Array.Copy(a, ofs, text, 0, len);
        return Encoding.GetEncoding("ISO-8859-1").GetString(text);
    }
    public static double GetFixed(byte[] a, int ofs)
    {
        return (double)GetInt32(a, ofs) / 65536.0;
    }
    public static double GetShortFixed(byte[] a, int ofs)
    {
        return (double)GetInt16(a, ofs) / 4096.0;
    }
    public static void SetInt16(byte[] a, int ofs, int val)
    {
        a[ofs] = (byte)val;
        a[ofs + 1] = (byte)(val >> 8);
    }
    public static void SetInt32(byte[] a, int ofs, int val)
    {
        a[ofs] = (byte)val;
        a[ofs + 1] = (byte)(val >> 8);
        a[ofs + 2] = (byte)(val >> 16);
        a[ofs + 3] = (byte)(val >> 24);
    }
}

public class HogItem
{
    public HogItem(byte[] data, int ofs)
    {
        name = Bits.GetString(data, ofs, 13);
        dataSize = Bits.GetInt32(data, ofs + 13);
        dataOfs = ofs + 13 + 4;
    }
    public string name;
    public int dataSize;
    public int dataOfs;
}

public class Hog
{
    public Hog(string filename)
    {
        this.filename = filename;
        load(filename);
    }
    private void load(string filename)
    {
        data = File.ReadAllBytes(filename);
        size = data.Length;
        if (!data.Take(3).SequenceEqual(new byte[] { (byte)'D', (byte)'H', (byte)'F' }))
            throw new Exception("invalid header");
        items = new List<HogItem>();
        index = new Dictionary<string, HogItem>();
        int ofs = 3;
        while (ofs < size)
        {
            HogItem item = new HogItem(data, ofs);
            //Debug.WriteLine(item.name);
            //Debug.WriteLine(item.dataSize);
            ofs = item.dataOfs + item.dataSize;
            items.Add(item);
            index.Add(item.name, item);
        }
    }
    private string filename;
    private byte[] data;
    private int size;
    public List<HogItem> items;
    public Dictionary<string, HogItem> index;
    public byte[] ItemData(string name)
    {
        HogItem item = index[name];
        var ret = new byte[item.dataSize];
        Array.Copy(data, item.dataOfs, ret, 0, item.dataSize);
        return ret;
    }
    public string ItemString(string name)
    {
        byte[] data = ItemData(name);
        return Encoding.GetEncoding("ISO-8859-1").GetString(data);
    }
}

[Flags]
enum PigFlag
{
    Transparent = 1,
    SuperTransparent = 2,
    NoLighting = 4,
    RLE = 8,
    PagedOut = 16,
    RLEBig = 32
}

class PigBitmap
{
    public PigBitmap(byte[] data, int ofs, bool d2)
    {
        int d2add = d2 ? 1 : 0;
        name = Bits.GetString(data, ofs, 8);
        frame = data[ofs + 8];
        width = data[ofs + 9];
        height = data[ofs + 10];
        if (d2) {
            int wh_extra = data[ofs + 11];
            width += (short)((wh_extra & 0x0f) << 8);
            height += (short)((wh_extra & 0xf0) << 4);
        }
        flags = (PigFlag)data[ofs + d2add + 11];
        aveColor = data[ofs + d2add + 12];
        this.ofs = Bits.GetInt32(data, ofs + d2add + 13);
        // d1 size = 17, d2 size = 18
    }
    public string name;
    public byte frame;
    public short width;
    public short height;
    public PigFlag flags;
    public byte aveColor;
    public int ofs;
}

class PigSound
{
    public PigSound(byte[] data, int ofs)
    {
        name = Bits.GetString(data, ofs, 8);
        length = Bits.GetInt32(data, ofs + 8);
        data_length = Bits.GetInt32(data, ofs + 12);
        offset = Bits.GetInt32(data, ofs + 16);
        // size = 20
    }
    public string name;
    public int length;
    public int data_length;
    public int offset;
}

static class Ext
{
    public static void Read(this BinaryReader r, out sbyte v)
    {
        v = r.ReadSByte();
    }
    public static void Read(this BinaryReader r, out byte v)
    {
        v = r.ReadByte();
    }
    public static void Read(this BinaryReader r, out short v)
    {
        v = r.ReadInt16();
    }
    public static void Read(this BinaryReader r, out ushort v)
    {
        v = r.ReadUInt16();
    }
    public static void Read(this BinaryReader r, out int v)
    {
        v = r.ReadInt32();
    }
    public static void Read(this BinaryReader r, out uint v)
    {
        v = r.ReadUInt32();
    }
    public static void Read(this sbyte[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            r.Read(out v[i]);
    }
    public static void Read(this byte[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            r.Read(out v[i]);
    }
    public static void Read(this short[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            r.Read(out v[i]);
    }
    public static void Read(this ushort[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            r.Read(out v[i]);
    }
    public static void Read(this int[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            r.Read(out v[i]);
    }
    public static void Read(this Fix[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r);
    }
    public static void Read(this vms_vector[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r);
    }
    public static void Read(this g3s_uvl[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r);
    }
    public static void Read(this BitmapIndex[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r);
    }
    public static void Read(this TmapInfo[] v, BinaryReader r, Version version)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r, version);
    }
    public static void Read(this Vclip[] v, BinaryReader r, Version version)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r, version);
    }
    public static void Read(this Eclip[] v, BinaryReader r, Version version)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r, version);
    }
    public static void Read(this Wclip[] v, BinaryReader r, Version version)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r, version);
    }
    public static void Read(this RobotInfo[] v, BinaryReader r, Version version)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r, version);
    }
    public static void Read(this JointPos[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r);
    }
    public static void Read(this WeaponInfo[] v, BinaryReader r, Version version)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r, version);
    }
    public static void Read(this PowerupInfo[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r);
    }
    public static void Read(this PolyModel[] v, BinaryReader r)
    {
        for (int i = 0, l = v.Length; i < l; i++)
            v[i].Read(r);
    }
    public static string ReadCString(this BinaryReader r)
    {
        byte b;
        var bs = new List<byte>();
        while ((b = r.ReadByte()) != 0)
           bs.Add(b);
        return Encoding.UTF8.GetString(bs.ToArray());
    }
}

class Pig
{
    public Pig(string filename)
    {
        this.filename = filename;
        data = File.ReadAllBytes(filename);
        int ofs2 = Bits.GetInt32(data, 0);
        int ofs = 4;
        bool d2 = false;
        if (ofs2 == 0x47495050) // descent 2 pig
        {
            d2 = true;
            ofs = 8; // skip version
        }
        else if (ofs2 >= 65536) // descent reg 1.4+ pig, first int is offset
        {
            ofs = ofs2;
        }
        else // descent 1 sw / pre 1.4 pig: first int is count
        {
            ofs = 0;
        }
        /*
        int lvlTexCount = Bits.GetInt32(data, ofs);
        ofs += 4;
        lvlTexIdx = new int[lvlTexCount];
        for (int i = 0; i < lvlTexCount; i++)
        {
            lvlTexIdx[i] = Bits.GetUInt16(data, ofs);
            ofs += 2;
        }
        ofs = ofs2;
        */
        bitmapCount = Bits.GetInt32(data, ofs);
        //Debug.WriteLine("bitmapCount " + bitmapCount);
        //Debug.Log("pig bitmapCount " + bitmapCount);
        if (d2)
        {
            soundCount = 0;
            soundData = null;
            ofs += 4;
        }
        else
        {
            soundCount = Bits.GetInt32(data, ofs + 4);
            soundData = data;
            ofs += 8;
        }
        int bitmapHdrSize = d2 ? 18 : 17;
        dataOfs = ofs + bitmapCount * bitmapHdrSize + soundCount * 20;
        if (!d2)
            soundDataOfs = dataOfs;
        bitmaps = new PigBitmap[bitmapCount];
        bitmapIdxByName = new Dictionary<string, int>();
        for (int i = 0; i < bitmapCount; i++)
        {
            PigBitmap bitmap = new PigBitmap(data, ofs, d2);
            ofs += bitmapHdrSize;
            bitmaps[i] = bitmap;
            //Debug.Log("bitmap " + i + " ofs " + bitmap.ofs);
            //if ((bitmap.frame & 31) == 0)
            //    bitmapIdxByName.Add(bitmap.name, i);
        }
        if (!d2)
        {
            sounds = new PigSound[soundCount];
            soundIdxByName = new Dictionary<string, int>();
            for (int i = 0; i < soundCount; i++)
            {
                PigSound sound = new PigSound(data, ofs);
                ofs += 20;
                sounds[i] = sound;
            }
        }
    }

    public byte[] GetBitmap(int idx, out int width, out int height)
    {
        var bmp = bitmaps[idx];
        width = bmp.width;
        height = bmp.height;
        return GetBitmap(bmp);
    }

    public byte[] GetBitmap(PigBitmap bmp)
    {
        int ofs = dataOfs + bmp.ofs;
        byte[] ret = new byte[bmp.width * bmp.height];
        //    if (bmp.name.Equals("exit01"))
        //        Debug.Assert(false);
        if ((bmp.flags & (PigFlag.RLE | PigFlag.RLEBig)) != 0)
        {
            int size = Bits.GetInt32(data, ofs);
            int ofsEnd = ofs + size;
            ofs += 4;
            ofs += (bmp.flags & PigFlag.RLEBig) != 0 ? bmp.height * 2 : bmp.height;
            int retOfs = 0;
            while (ofs < ofsEnd)
            {
                byte b = data[ofs++];
                if ((b & 0xe0) == 0xe0)
                {
                    int c = b & 0x1f;
                    if (c == 0)
                        continue;
                    b = data[ofs++];
                    for (int i = 0; i < c; i++)
                        ret[retOfs++] = b;
                }
                else
                    ret[retOfs++] = b;
            }
        }
        else
        {
            Array.Copy(data, ofs, ret, 0, ret.Length);
        }
        return ret;
    }

    public void CopySound(PigSound sound, byte[] dest, int destOfs)
    {
        Array.Copy(data, dataOfs + sound.offset, dest, destOfs, sound.length);
    }

    public void ReadTableData(out ClassicData v)
    {
        v = new ClassicData();
        v.Read(new BinaryReader(new MemoryStream(data, 4, Bits.GetInt32(data, 0) - 4)), Version.D1);
    }

    /*
    public byte[] GetBitmap(string name, out int width, out int height) {
        return GetBitmap(bitmapIdxByName[name], width, height);
    }
    */

    public void ReadD2Sound(string filename)
    {
        soundData = File.ReadAllBytes(filename);
        int ofs = 8;
        soundCount = Bits.GetInt32(soundData, ofs);
        ofs += 4;
        soundDataOfs = ofs + 20 * soundCount;
        sounds = new PigSound[soundCount];
        for (int i = 0; i < soundCount; i++)
        {
            sounds[i] = new PigSound(soundData, ofs);
            ofs += 20;
        }
    }

    public string filename;
    public byte[] data;
    public byte[] soundData;
    public int dataOfs;
    public int soundDataOfs;
    public int bitmapCount;
    public int soundCount;
    public PigBitmap[] bitmaps;
    public PigSound[] sounds;
    public Dictionary<string, int> bitmapIdxByName;
    public Dictionary<string, int> soundIdxByName;
    //public int[] lvlTexIdx;
}

/*
public class Vector3
{
    public double x, y, z;
    public Vector3(byte[] data, int ofs)
    {
        x = Bits.GetFixed(data, ofs);
        y = Bits.GetFixed(data, ofs + 4);
        z = Bits.GetFixed(data, ofs + 8);
    }

    public override string ToString()
    {
        return String.Format("{{{0},{1},{2}}}", x, y, z);
    }
}

public class Vector2
{
    public double x, y;
    public Vector2(byte[] data, int ofs)
    {
        x = Bits.GetShortFixed(data, ofs);
        y = Bits.GetShortFixed(data, ofs + 2);
    }
    public override string ToString()
    {
        return String.Format("{{{0},{1}}}", x, y);
    }
}
*/

public struct Fix
{
    public static Fix F1_0 = new Fix() { n = 65536 };
    public int n;
    public void Read(BinaryReader r)
    {
        n = r.ReadInt32();
    }
    public float ToFloat()
    {
        return n / 65536f;
    }
    public override string ToString()
    {
        return ToFloat().ToString("N3");
    }
}

// 0x4000 = 90deg
public struct FixAng
{
    public int n;
    public void Read(BinaryReader r)
    {
        n = r.ReadInt16();
    }
    public float ToFloat()
    {
        return n / 16384f * 90f;
    }
    public override string ToString()
    {
        return ToFloat().ToString("N1");
    }
}

public struct vms_vector
{
    public Fix x, y, z;

    public static vms_vector operator+(vms_vector a, vms_vector b) {
        vms_vector ret = a;
        ret.x.n += b.x.n;
        ret.y.n += b.y.n;
        ret.z.n += b.z.n;
        return ret;
    }
    public static vms_vector operator -(vms_vector a, vms_vector b)
    {
        vms_vector ret = a;
        ret.x.n -= b.x.n;
        ret.y.n -= b.y.n;
        ret.z.n -= b.z.n;
        return ret;
    }
    public void Read(BinaryReader r)
    {
        x.Read(r);
        y.Read(r);
        z.Read(r);
    }
    public override string ToString()
    {
        return "[" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + "]";
    }
}

public struct g3s_uvl
{
    public Fix u, v, l;
    public void Read(BinaryReader r)
    {
        u.Read(r);
        v.Read(r);
        l.Read(r);
    }
    public override string ToString()
    {
        return "[" + u.ToString() + ", " + v.ToString() + ", " + l.ToString() + "]";
    }
}

public struct vms_angvec
{
    public FixAng p, b, h;
    public void Read(BinaryReader r)
    {
        p.Read(r);
        b.Read(r);
        h.Read(r);
    }
    public override string ToString()
    {
        return "[" + p.ToString() + ", " + b.ToString() + ", " + h.ToString() + "]";
    }
}

public enum Version
{
    UNKNOWN,
    D1,
    D2
}

public struct TmapInfo
{
    public string filename;
    public byte flags, pad0, pad1, pad2;
    public Fix lighting;
    public Fix damage;
    public int eclip_num;
    public int destroyed, slide_u, slide_v;
    public void Read(BinaryReader r, Version version)
    {
        if (version == Version.D1)
            filename = UTF8Encoding.UTF8.GetString(r.ReadBytes(13));
        r.Read(out flags);
        if (version != Version.D1)
        {
            r.Read(out pad0);
            r.Read(out pad1);
            r.Read(out pad2);
        }
        lighting.Read(r);
        damage.Read(r);
        if (version == Version.D1)
        {
            r.Read(out eclip_num);
        }
        else
        {
            eclip_num = r.ReadInt16();
            destroyed = r.ReadInt16();
            slide_u = r.ReadInt16();
            slide_v = r.ReadInt16();
        }
    }
}

public struct Vclip
{
    const int VCLIP_MAX_FRAMES = 30;
    public Fix play_time;
    public int num_frames;
    public Fix frame_time;
    public int flags;
    public short sound_num;
    public BitmapIndex[] frames;
    public Fix light_value;

    public void Read(BinaryReader r, Version version)
    {
        play_time.Read(r);
        num_frames = r.ReadInt32();
        frame_time.Read(r);
        flags = r.ReadInt32();
        sound_num = r.ReadInt16();
        frames = new BitmapIndex[VCLIP_MAX_FRAMES];
        for (int i = 0; i < VCLIP_MAX_FRAMES; i++)
            frames[i].index = r.ReadUInt16();
        light_value.Read(r);
    }
}

public struct Eclip
{
    Vclip vc;
    Fix time_left;
    int frame_count;
    short changing_wall_texture;
    short changing_object_texture;
    int flags;
    int crit_clip;
    int dest_bm_num;
    int dest_vclip;
    int dest_eclip;
    Fix dest_size;
    int sound_num;
    int segnum, sidenum;

    public void Read(BinaryReader r, Version version)
    {
        vc.Read(r, version);
        time_left.Read(r);
        r.Read(out frame_count);
        r.Read(out changing_wall_texture);
        r.Read(out changing_object_texture);
        r.Read(out flags);
        r.Read(out crit_clip);
        r.Read(out dest_bm_num);
        r.Read(out dest_vclip);
        r.Read(out dest_eclip);
        dest_size.Read(r);
        r.Read(out sound_num);
        r.Read(out segnum);
        r.Read(out sidenum);
    }
}

public struct Wclip
{
    public const int MAX_CLIP_FRAMES_D1 = 20;
    public const int MAX_CLIP_FRAMES_D2 = 50;
    public Fix play_time;
    public short num_frames;
    public short[] frames;
    public short open_sound;
    public short close_sound;
    public short flags;
    public string filename;

    public void Read(BinaryReader r, Version version)
    {
        play_time.Read(r);
        r.Read(out num_frames);
        int n = version == Version.D1 ? MAX_CLIP_FRAMES_D1 : MAX_CLIP_FRAMES_D2;
        frames = new short[n];
        for (int fi = 0; fi < n; fi++)
            r.Read(out frames[fi]);
        r.Read(out open_sound);
        r.Read(out close_sound);
        r.Read(out flags);
        byte[] namebuf = r.ReadBytes(14);
        int i = 0;
        while (i < 13 && namebuf[i] != 0)
            i++;
        filename = Encoding.UTF8.GetString(namebuf, 0, i);
    }
}

public struct JointList
{
    short n_joints;
    short offset;
    public void Read(BinaryReader r)
    {
        r.Read(out n_joints);
        r.Read(out offset);
    }
}

public struct RobotInfo
{
    public const int MAX_GUNS = 8;
    public const int NDL = 5;
    public const int N_ANIM_STATES = 5;
    public int model_num;
    public int n_guns;
    public vms_vector[] gun_points;
    public byte[] gun_submodels;
    public short exp1_vclip_num;
    public short exp1_sound_num;
    public short exp2_vclip_num;
    public short exp2_sound_num;
    public int weapon_type, weapon_type2;
    public sbyte contains_id;
    public sbyte contains_count;
    public sbyte contains_prob;
    public sbyte contains_type;
    public sbyte kamikaze, badass, energy_drain;
    public int score_value;
    public Fix lighting;
    public Fix strength;
    public Fix mass;
    public Fix drag;
    public Fix[] field_of_view;
    public Fix[] firing_wait;
    public Fix[] firing_wait2;
    public Fix[] turn_time;
    public Fix[] fire_power;
    public Fix[] shield;
    public Fix[] max_speed;
    public Fix[] circle_distance;
    public sbyte[] rapidfire_count;
    public sbyte[] evade_speed;
    public sbyte cloak_type;
    public sbyte attack_type;
    public sbyte boss_flag;
    public byte see_sound;
    public byte attack_sound;
    public byte claw_sound;
    public sbyte taunt_sound;
    public sbyte companion, smart_blobs, energy_blobs, thief, pursuit, lightcast, death_roll;
    public sbyte flags, pad0, pad1, pad2, deathroll_sound, glow, behaviour, aim;
    public JointList[,] anim_states;
    public int sig;

    public void Read(BinaryReader r, Version version)
    {
        model_num = r.ReadInt32();
        if (version == Version.D1)
            n_guns = r.ReadInt32();
        (gun_points = new vms_vector[MAX_GUNS]).Read(r);
        (gun_submodels = new byte[MAX_GUNS]).Read(r);
        r.Read(out exp1_vclip_num);
        r.Read(out exp1_sound_num);
        r.Read(out exp2_vclip_num);
        r.Read(out exp2_sound_num);
        if (version == Version.D1)
        {
            weapon_type = r.ReadInt16();
            weapon_type2 = -1;
        }
        else
        {
            weapon_type = r.ReadSByte();
            weapon_type2 = r.ReadSByte();
            n_guns = r.ReadSByte();
        }
        r.Read(out contains_id);
        r.Read(out contains_count);
        r.Read(out contains_prob);
        r.Read(out contains_type);
        if (version == Version.D1)
        {
            score_value = r.ReadInt32();
        }
        else
        {
            r.Read(out kamikaze);
            score_value = r.ReadInt16();
            r.Read(out badass);
            r.Read(out energy_drain);
        }
        lighting.Read(r);
        strength.Read(r);
        mass.Read(r);
        drag.Read(r);
        (field_of_view = new Fix[NDL]).Read(r);
        (firing_wait = new Fix[NDL]).Read(r);
        if (version != Version.D1)
            (firing_wait2 = new Fix[NDL]).Read(r);
        (turn_time = new Fix[NDL]).Read(r);
        if (version == Version.D1)
        {
            (fire_power = new Fix[NDL]).Read(r);
            (shield = new Fix[NDL]).Read(r);
        }
        (max_speed = new Fix[NDL]).Read(r);
        (circle_distance = new Fix[NDL]).Read(r);
        (rapidfire_count = new sbyte[NDL]).Read(r);
        (evade_speed = new sbyte[NDL]).Read(r);
        r.Read(out cloak_type);
        r.Read(out attack_type);
        if (version == Version.D1)
            r.Read(out boss_flag);
        r.Read(out see_sound);
        r.Read(out attack_sound);
        r.Read(out claw_sound);
        if (version != Version.D1)
        {
            r.Read(out taunt_sound);
            r.Read(out boss_flag);
            r.Read(out companion);
            r.Read(out smart_blobs);
            r.Read(out energy_blobs);
            r.Read(out thief);
            r.Read(out pursuit);
            r.Read(out lightcast);
            r.Read(out death_roll);
            r.Read(out flags);
            r.Read(out pad0);
            r.Read(out pad1);
            r.Read(out pad2);
            r.Read(out deathroll_sound);
            r.Read(out glow);
            r.Read(out behaviour);
            r.Read(out aim);
        }
        anim_states = new JointList[MAX_GUNS + 1, N_ANIM_STATES];
        for (int i = 0; i < MAX_GUNS + 1; i++)
            for (int j = 0; j < N_ANIM_STATES; j++)
                anim_states[i, j].Read(r);
        r.Read(out sig);
    }
}

public struct JointPos
{
    public short jointnum;
    public vms_angvec angles;
    public void Read(BinaryReader r)
    {
        r.Read(out jointnum);
        angles.Read(r);
    }
}

public struct WeaponInfo
{
    public const int NDL = 5;
    public sbyte render_type;
    public sbyte persistent;
    public int model_num;
    public int model_num_inner;

    public sbyte flash_vclip;
    public short flash_sound;
    public sbyte robot_hit_vclip;
    public short robot_hit_sound;

    public sbyte wall_hit_vclip;
    public short wall_hit_sound;
    public sbyte fire_count;
    public sbyte ammo_usage;

    public sbyte weapon_vclip;
    public sbyte destroyable;
    public sbyte matter;
    public sbyte bounce;

    public sbyte homing_flag;
    public sbyte dum1, dum2, dum3;
    public byte speedvar, flags;
    public sbyte flash, afterburner_size, children;

    public Fix energy_usage;
    public Fix fire_wait;

    BitmapIndex bitmap;

    Fix blob_size;
    Fix flash_size;
    Fix impact_size;
    Fix[] strength;
    Fix[] speed;
    Fix mass;
    Fix drag;
    Fix thrust;
    Fix po_len_to_width_ratio;
    Fix light;
    Fix lifetime;
    Fix damage_radius;
    BitmapIndex picture, hires_picture;
    Fix multi_damage_scale;

    public void Read(BinaryReader r, Version version)
    {
        r.Read(out render_type);
        if (version == Version.D1)
        {
            model_num = r.ReadSByte();
            model_num_inner = r.ReadSByte();
            r.Read(out persistent);
        }
        else
        {
            r.Read(out persistent);
            model_num = r.ReadInt16();
            model_num_inner = r.ReadInt16();
        }

        r.Read(out flash_vclip);
        if (version == Version.D1)
            r.Read(out flash_sound);
        r.Read(out robot_hit_vclip);
        if (version == Version.D1)
            r.Read(out robot_hit_sound);
        else
            r.Read(out flash_sound);

        r.Read(out wall_hit_vclip);
        if (version == Version.D1)
            r.Read(out wall_hit_sound);
        r.Read(out fire_count);
        if (version != Version.D1)
            r.Read(out robot_hit_sound);
        r.Read(out ammo_usage);

        r.Read(out weapon_vclip);
        if (version != Version.D1)
            r.Read(out wall_hit_sound);

        r.Read(out destroyable);
        r.Read(out matter);
        r.Read(out bounce);

        r.Read(out homing_flag);
        if (version == Version.D1)
        {
            r.Read(out dum1); r.Read(out dum2); r.Read(out dum3);
            speedvar = 128;
            children = -1;
        }
        else
        {
            r.Read(out speedvar);
            r.Read(out flags);
            r.Read(out flash);
            r.Read(out afterburner_size);
            r.Read(out children);
        }

        energy_usage.Read(r);
        fire_wait.Read(r);

        if (version == Version.D2)
            multi_damage_scale.Read(r);
        else
            multi_damage_scale = Fix.F1_0;

        bitmap.Read(r);

        blob_size.Read(r);
        flash_size.Read(r);
        impact_size.Read(r);
        (strength = new Fix[NDL]).Read(r);
        (speed = new Fix[NDL]).Read(r);
        mass.Read(r);
        drag.Read(r);
        thrust.Read(r);
        po_len_to_width_ratio.Read(r);
        light.Read(r);
        lifetime.Read(r);
        damage_radius.Read(r);
        picture.Read(r);
        if (version != Version.D1)
            hires_picture.Read(r);
    }
}

public struct PowerupInfo
{
    int vclip_num;
    int hitsound;
    Fix size;
    Fix light;
    public void Read(BinaryReader r)
    {
        r.Read(out vclip_num);
        r.Read(out hitsound);
        size.Read(r);
        light.Read(r);
    }
}

public struct PolyModel
{
    public const int MAX_SUBMODELS = 10;
    public int n_models;
    public int model_data_size;
    public byte[] data;
    public int[] submodel_ptrs;
    public vms_vector[] submodel_offsets;
    public vms_vector[] submodel_norms;               //norm for sep plane
    public vms_vector[] submodel_pnts;                //point on sep plane
    public Fix[] submodel_rads;                               //radius for each submodel
    public byte[] submodel_parents;          //what is parent for each submodel
    public vms_vector[] submodel_mins;
    public vms_vector[] submodel_maxs;
    public vms_vector mins, maxs;
    public Fix rad;
    public byte n_textures;
    public ushort first_texture;
    public byte simpler_model;

    public void Read(BinaryReader r)
    {
        r.Read(out n_models);
        r.Read(out model_data_size);
        r.ReadInt32();
        (submodel_ptrs = new int[MAX_SUBMODELS]).Read(r);
        (submodel_offsets = new vms_vector[MAX_SUBMODELS]).Read(r);
        (submodel_norms = new vms_vector[MAX_SUBMODELS]).Read(r);
        (submodel_pnts = new vms_vector[MAX_SUBMODELS]).Read(r);
        (submodel_rads = new Fix[MAX_SUBMODELS]).Read(r);
        (submodel_parents = new byte[MAX_SUBMODELS]).Read(r);
        (submodel_mins = new vms_vector[MAX_SUBMODELS]).Read(r);
        (submodel_maxs = new vms_vector[MAX_SUBMODELS]).Read(r);
        mins.Read(r);
        maxs.Read(r);
        rad.Read(r);
        r.Read(out n_textures);
        r.Read(out first_texture);
        r.Read(out simpler_model);
    }
}


public struct BitmapIndex
{
    public ushort index;
    public void Read(BinaryReader r)
    {
        r.Read(out index);
    }
}

public struct PlayerShip
{
    const int N_PLAYER_GUNS = 8;
    public int model_num;
    public int expl_vclip_num;
    public Fix mass, drag;
    public Fix max_thrust;
    public Fix reverse_thrust;
    public Fix brakes;
    public Fix wiggle;
    public Fix max_rotthrust;
    vms_vector[] gun_points;

    public void Read(BinaryReader r)
    {
        r.Read(out model_num);
        r.Read(out expl_vclip_num);
        mass.Read(r);
        drag.Read(r);
        max_thrust.Read(r);
        reverse_thrust.Read(r);
        brakes.Read(r);
        wiggle.Read(r);
        max_rotthrust.Read(r);
        (gun_points = new vms_vector[N_PLAYER_GUNS]).Read(r);
    }
}

public class ClassicData
{
    const int MAX_TEXTURES = 800;
    const int MAX_SOUNDS = 250;
    const int VCLIP_MAXNUM = 70;
    const int MAX_EFFECTS = 60;
    const int MAX_WALL_ANIMS = 30;
    const int N_COCKPIT_BITMAPS = 4;
    const int MAX_CONTROLCEN_GUNS = 4;
    const int MAX_ROBOT_TYPES = 30;
    const int MAX_OBJTYPE = 100;
    const int MAX_GAUGE_BMS = 80;
    const int MAX_OBJ_BITMAPS = 210;
    const int MAX_POLYGON_MODELS = 85;
    const int MAX_ROBOT_JOINTS = 600;
    const int MAX_WEAPON_TYPES = 30;
    const int MAX_POWERUP_TYPES = 29;

    public int NumTextures;
    public BitmapIndex[] Textures;
    public TmapInfo[] TmapInfo;
    public byte[] Sounds;
    public byte[] AltSounds;
    public int Num_vclips;
    public Vclip[] Vclip;
    public int Num_effects;
    public Eclip[] Effects;
    public int Num_wall_anims;
    public Wclip[] WallAnims;
    public int N_robot_types;
    public RobotInfo[] RobotInfo;
    public int N_robot_joints;
    public JointPos[] RobotJoints;
    public int N_weapon_types;
    public WeaponInfo[] WeaponInfo;
    public int N_powerup_types;
    public PowerupInfo[] PowerupInfo;
    public int N_polygon_models;
    public PolyModel[] PolygonModels;

    public BitmapIndex[] Gauges, Gauges_hires;
    public int[] DyingModelnums;
    public int[] DeadModelnums;
    public BitmapIndex[] ObjBitmaps;
    public ushort[] ObjBitmapPtrs;
    public PlayerShip PlayerShip;
    int Num_cockpits;
    public BitmapIndex[] CockpitBitmaps;

    int Num_total_object_types;
    public sbyte[] ObjType;
    public sbyte[] ObjId;
    public Fix[] ObjStrength;

    public int First_multi_bitmap_num;

    int N_controlcen_guns;
    public vms_vector[] ControlcenGunPoints;
    public vms_vector[] ControlcenGunDirs;
    public int exit_modelnum;
    public int destroyed_exit_modelnum;

    public void Read(BinaryReader r, Version version)
    {
        bool d1 = version == Version.D1;
        r.Read(out NumTextures);
        (Textures = new BitmapIndex[d1 ? MAX_TEXTURES : NumTextures]).Read(r);
        (TmapInfo = new TmapInfo[d1 ? MAX_TEXTURES : NumTextures]).Read(r, version);
        int NumSounds = d1 ? MAX_SOUNDS : r.ReadInt32();
        Debug.Log("NumSounds " + NumSounds);
        (Sounds = new byte[NumSounds]).Read(r);
        (AltSounds = new byte[NumSounds]).Read(r);
        r.Read(out Num_vclips);
        (Vclip = new Vclip[d1 ? VCLIP_MAXNUM : Num_vclips]).Read(r, version);
        r.Read(out Num_effects);
        (Effects = new Eclip[d1 ? MAX_EFFECTS : Num_effects]).Read(r, version);
        r.Read(out Num_wall_anims);
        (WallAnims = new Wclip[d1 ? MAX_WALL_ANIMS : Num_wall_anims]).Read(r, version);
        r.Read(out N_robot_types);
        (RobotInfo = new RobotInfo[d1 ? MAX_ROBOT_TYPES : N_robot_types]).Read(r, version);
        r.Read(out N_robot_joints);
        (RobotJoints = new JointPos[d1 ? MAX_ROBOT_JOINTS : N_robot_joints]).Read(r);
        r.Read(out N_weapon_types);
        (WeaponInfo = new WeaponInfo[d1 ? MAX_WEAPON_TYPES : N_weapon_types]).Read(r, version);
        r.Read(out N_powerup_types);
        (PowerupInfo = new PowerupInfo[d1 ? MAX_POWERUP_TYPES : N_powerup_types]).Read(r);
        r.Read(out N_polygon_models);
        (PolygonModels = new PolyModel[N_polygon_models]).Read(r);
        for (int i = 0; i < N_polygon_models; i++)
            PolygonModels[i].data = r.ReadBytes(PolygonModels[i].model_data_size);
        if (d1)
            (Gauges = new BitmapIndex[MAX_GAUGE_BMS]).Read(r);
        (DyingModelnums = new int[d1 ? MAX_POLYGON_MODELS : N_polygon_models]).Read(r);
        (DeadModelnums = new int[d1 ? MAX_POLYGON_MODELS : N_polygon_models]).Read(r);
        if (!d1)
        {
            int n = r.ReadInt32();
            (Gauges = new BitmapIndex[n]).Read(r);
            (Gauges_hires = new BitmapIndex[n]).Read(r);
        }
        int NumObjBitmaps = d1 ? MAX_OBJ_BITMAPS : r.ReadInt32();
        (ObjBitmaps = new BitmapIndex[NumObjBitmaps]).Read(r);
        (ObjBitmapPtrs = new ushort[NumObjBitmaps]).Read(r);
        PlayerShip.Read(r);

        r.Read(out Num_cockpits);
        (CockpitBitmaps = new BitmapIndex[d1 ? N_COCKPIT_BITMAPS : Num_cockpits]).Read(r);

        if (d1)
        {
            (Sounds = new byte[MAX_SOUNDS]).Read(r);
            (AltSounds = new byte[MAX_SOUNDS]).Read(r);

            r.Read(out Num_total_object_types);
            (ObjType = new sbyte[MAX_OBJTYPE]).Read(r);
            (ObjId = new sbyte[MAX_OBJTYPE]).Read(r);
            (ObjStrength = new Fix[MAX_OBJTYPE]).Read(r);
       }

        r.Read(out First_multi_bitmap_num);

        if (d1)
        {
            r.Read(out N_controlcen_guns);
            (ControlcenGunPoints = new vms_vector[MAX_CONTROLCEN_GUNS]).Read(r);
            (ControlcenGunDirs = new vms_vector[MAX_CONTROLCEN_GUNS]).Read(r);
            r.Read(out exit_modelnum);
            r.Read(out destroyed_exit_modelnum);
        }
    }
}

/*
class Side
{
    public int bitmap;
    public int bitmap2;
    public Vector2[] uvs;
    public double[] lights;

    public Side(byte[] data, ref int ofs)
    {
        bitmap = Bits.GetUInt16(data, ofs);
        ofs += 2;
        if ((bitmap & 0x8000) != 0)
        {
            bitmap = bitmap & 0x7fff;
            bitmap2 = Bits.GetUInt16(data, ofs);
            ofs += 2;
        }
        else
            bitmap2 = -1;
        uvs = new Vector2[4];
        lights = new double[4];
        for (int j = 0; j < 4; j++)
        {
            uvs[j] = new Vector2(data, ofs);
            ofs += 4;
            lights[j] = Bits.GetShortFixed(data, ofs);
            ofs += 2;
        }
    }
    public override string ToString()
    {
        return String.Format("[t: {0} t2: {1} uv: {2} l: {3}]", bitmap, bitmap2,
                string.Join(",", Array.ConvertAll(uvs, x => x.ToString())),
                string.Join(",", Array.ConvertAll(lights, x => x.ToString())));
    }
}

class Segment
{
    public int[] childIdx; // left, top, right, bottom, back, front
    public int[] vertIdx;
    public byte[] wallIds;
    public bool isSpecial;
    public byte special;
    public byte ecNum;
    public int value;
    public double staticLight;
    public Side[] sides;

    public Segment(byte[] data, int ofs)
    {
        childIdx = new int[6];
        byte mask = data[ofs++];
        for (int i = 0; i < 6; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                childIdx[i] = Bits.GetInt16(data, ofs);
                ofs += 2;
            }
            else
            {
                childIdx[i] = -1;
            }
        }
        vertIdx = new int[8];
        for (int i = 0; i < 8; i++)
        {
            vertIdx[i] = Bits.GetInt16(data, ofs);
            ofs += 2;
        }
        isSpecial = (mask & 64) != 0;
        if (isSpecial)
        {
            special = data[ofs++];
            ecNum = data[ofs++];
            value = Bits.GetInt16(data, ofs);
            ofs += 2;
        }
        staticLight = Bits.GetShortFixed(data, ofs);
        ofs += 2;
        byte wallMask = data[ofs++];
        wallIds = new byte[6];
        for (int i = 0; i < 6; i++)
            if ((wallMask & (1 << i)) != 0)
                wallIds[i] = data[ofs++];
            else
                wallIds[i] = 255;
        sides = new Side[6];
        for (int i = 0; i < 6; i++)
            if (childIdx[i] == -1 || wallIds[i] != 255)
                sides[i] = new Side(data, ref ofs);
    }
    public override string ToString()
    {
        return String.Format("v: {0} w: {1} l: {2} {3}",
            string.Join(",", Array.ConvertAll(vertIdx, x => x.ToString())),
            string.Join(",", Array.ConvertAll(wallIds, x => x.ToString())),
            staticLight,
            string.Join(",", Array.ConvertAll(sides, x => x == null ? "-" : x.ToString())));
    }
}

class Level
{
    public Level(byte[] data, int ofs = 0, int size = 0)
    {
        int ofsStart = ofs;
        int ofsEnd = size != 0 ? ofs + size : data.Length;
        if (data[ofs] != (byte)'L' || data[ofs + 1] != (byte)'V' ||
            data[ofs + 2] != (byte)'L' || data[ofs + 3] != (byte)'P')
            throw new Exception("wrong signature");
        if (Bits.GetInt32(data, ofs + 4) != 1)
            throw new Exception("wrong version");
        int geoOfs = ofsStart + Bits.GetInt32(data, ofs + 8) + 1;
        int vertCount = Bits.GetInt16(data, geoOfs);
        int segCount = Bits.GetInt16(data, geoOfs + 2);
        Debug.WriteLine(String.Format("#v {0} #s {1}", vertCount, segCount));
        ofs = geoOfs + 4;
        Vector3[] verts = new Vector3[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            verts[i] = new Vector3(data, ofs);
            ofs += 12;
        }
        Segment[] segs = new Segment[vertCount];
        for (int i = 0; i < segCount; i++)
        {
            segs[i] = new Segment(data, ofs);
            Debug.WriteLine(segs[i]);
            if (i == 1)
                break;
        }
    }
}
*/

class ClassicLoader
{

    private static byte VgaPalExt(byte b)
    {
        return (byte)((b << 2) | (b >> 4));
    }

    public static byte[] VgaPalConv(byte[] src)
    {
        var ret = new byte[768];
        for (int i = 0; i < 3 * 256; i++)
            ret[i] = VgaPalExt(src[i]);
        return ret;
    }

    static void DumpBitmapList(Pig pig, byte[] pal, string outdir, List<PigBitmap> texs)
    {

        //string s = hog.itemString("descent.sng");
        //Debug.WriteLine(s);
        /*
        int w = texs.First().width, h = texs.Sum(x => x.height);
        Rectangle rect = new Rectangle(0, 0, w, h);
        PixelFormat fmt = PixelFormat.Format32bppArgb;

        byte[] img = new byte[w * h * 4];
        int dstOfs = 0;

        foreach (PigBitmap bmp in texs)
        {
            byte[] img8 = pig.GetBitmap(bmp);
            int srcOfs = 0;
            int th = bmp.height;
            Debug.Assert(bmp.width == w);
            for (int y = 0; y < th; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = img8[srcOfs++] * 3;
                    img[dstOfs++] = pal[i + 2];
                    img[dstOfs++] = pal[i + 1];
                    img[dstOfs++] = pal[i];
                    img[dstOfs++] = (i >= 254 * 3 ? (byte)0 : (byte)255);
                }
        }

        using (Bitmap b = new Bitmap(w, h, fmt))
        {
            BitmapData d = b.LockBits(rect, ImageLockMode.ReadWrite, fmt);
            //Debug.WriteLine(d.Stride);
            //Debug.WriteLine(img[0]);
            //Debug.WriteLine(img[4]);
            Marshal.Copy(img, 0, d.Scan0, img.Length);
            b.UnlockBits(d);
            string fn = String.Format("{0}\\d1_{1}.png", outdir, texs.First().name);
            Debug.WriteLine(fn);
            b.Save(fn, ImageFormat.Png);
        }
        */
    }

    /*
    static void DumpTextures(Pig pig, byte[] pal, string outdir)
    {
        var seen = new HashSet<int>();
        foreach (int idx in pig.lvlTexIdx)
            seen.Add(idx);
        var idxs = seen.ToArray();
        Array.Sort(idxs);

        foreach (var idx in idxs)
        {
            if (idx == 0)
                continue;
            var bmp = pig.bitmaps[idx - 1];
            //Debug.WriteLine(String.Format("{0} {1}x{2} {3} {4}", bmp.name, bmp.width, bmp.height, bmp.frame & 0xff, bmp.flags));
            if ((bmp.frame & 0x9f) != 0)
                continue;
            DumpBitmapList(pig, pal, outdir, new List<PigBitmap> { bmp });
        }
    }
    */

    static void DumpSounds(Pig pig, string outdir)
    {
        foreach (var sound in pig.sounds)
        {
            var data = new byte[44 + sound.length];
            Array.Copy(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }, 0, data, 0, 4);
            Bits.SetInt32(data, 4, sound.length + 36);
            Array.Copy(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' }, 0, data, 8, 4);
            Array.Copy(new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' }, 0, data, 12, 4);
            Bits.SetInt32(data, 16, 16); // fmt size
            Bits.SetInt16(data, 20, 1); // pcm
            Bits.SetInt16(data, 22, 1); // mono
            Bits.SetInt32(data, 24, 11025); // sample rate
            Bits.SetInt32(data, 28, 11025); // byte rate
            Bits.SetInt16(data, 32, 1); // align
            Bits.SetInt16(data, 34, 8); // bits per sample
            Array.Copy(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' }, 0, data, 36, 4);
            Bits.SetInt32(data, 40, sound.length);
            pig.CopySound(sound, data, 44);
            using (var f = new FileStream(outdir + Path.DirectorySeparatorChar + sound.name + ".wav", FileMode.CreateNew))
                f.Write(data, 0, data.Length);
        }
    }


    #if false
    static void Main(string[] args)
    {

        //Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

#if false
        string dir = "c:\\games\\d1x-rebirth\\data";
        string outdir = "c:\\temp\\d1tex";
        Hog hog = new Hog(dir + "\\descent.hog");
        byte[] vgapal = hog.ItemData("palette.256");
        byte[] pal = PalConv(vgapal);

        /*
        if (args.Length == 0) {
            Console.WriteLine("Missing pig file argument");
            return;
        }

        var pigFile = args[0];
        if (!File.Exists(pigFile)) {
            Console.WriteLine(String.Format("Pig file {0} does not exist", pigFile));
            return;
        }
        */
        var pigFile = dir + "\\descent.pig";

        Pig pig;
        try
        {
            pig = new Pig(pigFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine(String.Format("Cannot read pig file {0}: {1}", pigFile, ex.Message));
            return;
        }

        TableData d;
        pig.ReadTableData(out d);
        /*
        Debug.WriteLine(d.destroyed_exit_modelnum);
        Debug.WriteLine(d.NumTextures);
        Debug.WriteLine(d.N_polygon_models);
        Debug.WriteLine(d.N_robot_types);
        Debug.WriteLine(d.N_robot_joints);
        */

        int mi = 0;
        foreach (var model in d.PolygonModels)
        {
            var r = new BinaryReader(new MemoryStream(model.data));
            //var x = r.ReadUInt32();
            //Debug.WriteLine("1: " + x.ToString("X"));
            //break;
            Debug.WriteLine("--- model " + mi);
            DumpModelData(r);
            mi++;
            if (mi == 5)
                break;
        }

        //DumpBitmapList(pig, pal, outdir, pig.bitmaps.Where(bmp => bmp.name == "rbot066").ToList());
        //DumpTextures(pig, pal, outdir);
#else
        var pigFile = @"c:\temp\d1sw14\descent.pig";
        var pig = new Pig(pigFile);
        DumpSounds(pig, @"c:\temp\d1sndsw");
        Console.WriteLine(String.Format("Wrote {0} files to current directory.", pig.soundCount));
#endif

        /*
        var texs = new List<PigBitmap>();
        string name = "eye01";
        foreach (var bmp in pig.bitmaps) {
            Debug.WriteLine(bmp.name);
            if (bmp.name.Equals(name))
                texs.Add(bmp);
        }
            
        DumpBitmapList(pig, pal, outdir, texs);
        */


        //Level lvl = new Level(hog.ItemData("level01.rdl"));
    }
#endif
}
}
