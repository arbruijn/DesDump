using Classic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesDump
{
    class ModelReader
    {
        enum PolyOp
        {
            EOF = 0,       //eof
            DEFPOINTS = 1,       //defpoints
            FLATPOLY = 2,       //flat-shaded polygon
            TMAPPOLY = 3,       //texture-mapped polygon
            SORTNORM = 4,       //sort by normal
            RODBM = 5,       //rod bitmap
            SUBCALL = 6,       //call a subobject
            DEFP_START = 7,       //defpoints with start
            GLOW = 8       //glow value for next poly
        }

        readonly vms_vector[] points = new vms_vector[1000];
        vms_vector pointPos = default(vms_vector);

        public class DedupList<T>
        {
            public List<T> ItemList = new List<T>();
            Dictionary<T, int> ItemDict = new Dictionary<T, int>();
            public int Add(T item)
            {
                if (ItemDict.TryGetValue(item, out int idx))
                    return idx;
                idx = ItemList.Count;
                ItemList.Add(item);
                ItemDict.Add(item, idx);
                return idx;
            }
        }

        /*
        // dictionary key for Unity point uniqueness
        struct UPoint
        {
            //Fix nx, ny, nz, x, y, z, u, v;
            public vms_vector norm;
            public vms_vector p;
            public g3s_uvl uvl;
        };

        Dictionary<UPoint, int> uPointIdx = new Dictionary<UPoint, int>();
        List<UPoint> uPoints = new List<UPoint>();
        public List<int>[] tris;

        public void Reset(int texCount)
        {
            uPointIdx = new Dictionary<UPoint, int>();
            uPoints = new List<UPoint>();
            tris = new List<int>[texCount];
        }

        void AddPoint(int bitmap, ref vms_vector norm, ref vms_vector p, ref g3s_uvl uvl)
        {
            var uPoint = new UPoint { norm = norm, p = p, uvl = uvl };
            int idx;
            if (!uPointIdx.TryGetValue(uPoint, out idx))
            {
                uPointIdx[uPoint] = idx = uPoints.Count;
                uPoints.Add(uPoint);
            }
            tris[bitmap].Add(idx);
        }
        */

        public DedupList<vms_vector> Verts;
        public DedupList<vms_vector> Norms;
        public DedupList<g3s_uvl> UVs;
        public List<int[][]>[] faces;

        public void Reset(int matCount)
        {
            Verts = new DedupList<vms_vector>();
            Norms = new DedupList<vms_vector>();
            UVs = new DedupList<g3s_uvl>();
            faces = new List<int[][]>[matCount].Select(_ => new List<int[][]>()).ToArray();
        }

        private void AddFace(int matIdx, int n, vms_vector norm, ushort[] pointIdxs, g3s_uvl[] uvl)
        {
            var normIdx = Norms.Add(norm);
            var face = new int[n][];
            for (var i = 0; i < n; i++)
                face[i] = new int[3] { Verts.Add(points[pointIdxs[i]]), normIdx, uvl == null ? -1 : UVs.Add(uvl[i]) };
            faces[matIdx].Add(face);
        }

        public void ReadModelData(BinaryReader r, List<int> flatColors, int texCount)
        {
            //Debug.Log("Sub start " + r.BaseStream.Position);
            int glow_idx = -1; // only 0 is actually used
            for (PolyOp op; (op = (PolyOp)r.ReadInt16()) != PolyOp.EOF;)
            {
                int n, s, i, color, bitmap, i1, i2;
                ushort[] pointIdx;
                g3s_uvl[] uvls;
                vms_vector v = default(vms_vector), norm = default(vms_vector);
                //vms_vector[] pvs;
                vms_vector oldPointPos;
                //g3s_uvl uvl = default(g3s_uvl);
                var startPos = r.BaseStream.Position - 2;
                //Debug.Log(startPos + ": " + op + " ");
                switch (op)
                {
                    case PolyOp.DEFPOINTS:
                        n = r.ReadInt16();
                        //pvs = new vms_vector[n];
                        //pvs.Read(r);
                        for (i = 0; i < n; i++)
                        {
                            points[i].Read(r);
                            points[i] += pointPos;
                        }
                        //Debug.Log(n + " " + string.Join(", ", pvs.Select(x => x.ToString()).ToArray()));
                        break;
                    case PolyOp.DEFP_START:
                        n = r.ReadInt16();
                        s = r.ReadInt16();
                        r.ReadInt16(); // align
                                       //pvs = new vms_vector[n];
                                       //pvs.Read(r);
                        for (i = 0; i < n; i++)
                        {
                            points[i + s].Read(r);
                            points[i + s] += pointPos;
                        }
                        //Debug.Log(s + " " + n + " " + string.Join(", ", pvs.Select(x => x.ToString()).ToArray()));
                        break;
                    case PolyOp.FLATPOLY:
                        n = r.ReadInt16();
                        v.Read(r);
                        norm.Read(r);
                        color = r.ReadInt16();
                        pointIdx = new ushort[n];
                        pointIdx.Read(r);
                        if ((n & 1) == 0)
                            r.ReadInt16(); // align
                        //Debug.Log(v + " " + norm + " c=" + color + " " + n + " " + string.Join(", ", pointIdx.Select(x => x.ToString()).ToArray()));
                        int colIdx = flatColors.IndexOf(color);
                        if (colIdx == -1)
                        {
                            colIdx = flatColors.Count;
                            flatColors.Add(color);
                        }
                        AddFace(texCount + colIdx, n, norm, pointIdx, null);
                        /*
                        for (i = 2; i < n; i++)
                        {
                            AddPoint(texCount + colIdx, ref norm, ref points[pointIdx[0]], ref uvl);
                            AddPoint(texCount + colIdx, ref norm, ref points[pointIdx[i - 1]], ref uvl);
                            AddPoint(texCount + colIdx, ref norm, ref points[pointIdx[i]], ref uvl);
                        }
                        */
                        break;
                    case PolyOp.TMAPPOLY:
                        n = r.ReadInt16();
                        v.Read(r);
                        norm.Read(r);
                        bitmap = r.ReadInt16();
                        pointIdx = new ushort[n];
                        pointIdx.Read(r);
                        if ((n & 1) == 0)
                            r.ReadInt16(); // align
                        uvls = new g3s_uvl[n];
                        uvls.Read(r);
                        //Debug.Log(v + " " + norm + " bm=" + bitmap + " " + n + " " + string.Join(", ", pointIdx.Select(x => x.ToString()).ToArray()) + " " + string.Join(", ", uvls.Select(x => x.ToString()).ToArray()));
                        AddFace(bitmap, n, norm, pointIdx, uvls);
                        /*
                        for (i = 2; i < n; i++)
                        {
                            AddPoint(bitmap, ref norm, ref points[pointIdx[0]], ref uvls[0]);
                            AddPoint(bitmap, ref norm, ref points[pointIdx[i - 1]], ref uvls[i - 1]);
                            AddPoint(bitmap, ref norm, ref points[pointIdx[i]], ref uvls[i]);
                        }
                        */
                        glow_idx = -1;
                        break;
                    case PolyOp.SORTNORM:
                        r.ReadInt16(); // align
                        v.Read(r);
                        norm.Read(r);
                        i1 = r.ReadInt16();
                        i2 = r.ReadInt16();
                        //Debug.Log(v + " " + norm + " " + i1 + " " + i2);
                        var pos = r.BaseStream.Position;
                        r.BaseStream.Position = startPos + i1;
                        ReadModelData(r, flatColors, texCount);
                        r.BaseStream.Position = startPos + i2;
                        ReadModelData(r, flatColors, texCount);
                        r.BaseStream.Position = pos;
                        break;
                    case PolyOp.RODBM:
                        bitmap = r.ReadInt16(); // align
                        v.Read(r);
                        i1 = r.ReadInt16();
                        r.ReadInt16(); // align
                        norm.Read(r);
                        i2 = r.ReadInt16();
                        r.ReadInt16(); // align
                        //Debug.Log(v + " " + norm + " " + i1 + " " + i2);
                        break;
                    case PolyOp.SUBCALL:
                        i2 = r.ReadInt16();
                        v.Read(r);
                        i1 = r.ReadInt16();
                        r.ReadInt16(); // align
                        //Debug.Log("anim=" + i2 + " " + v + " " + i1);
                        pos = r.BaseStream.Position;
                        r.BaseStream.Position = startPos + i1;
                        oldPointPos = pointPos;
                        pointPos += v;
                        ReadModelData(r, flatColors, texCount);
                        pointPos = oldPointPos;
                        r.BaseStream.Position = pos;
                        break;
                    case PolyOp.GLOW:
                        glow_idx = r.ReadInt16();
                        //Debug.Log(i1.ToString());
                        break;
                    default:
                        throw new Exception("Unknown model op " + op);
                }
            }
            //Debug.Log("Sub done");
        }

    }
}
