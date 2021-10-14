using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DxfMerge
{
    class Program
    {
        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
        static Block ImportDxfAsBlock(string dxfFile)
        {
            DxfDocument doc = DxfDocument.Load(dxfFile);
            Block block = new Block(Hash(dxfFile));
            foreach (Layout layout in doc.Layouts)
            {
                block.Entities.AddRange(doc.Layouts.GetReferences(layout).OfType<EntityObject>().Select(x => x.Clone() as EntityObject));
            }
            return block;
        }
        static void Main(string[] args)
        {
            string[] inputs = new string[] { "../../../1.dxf", "../../../2.dxf" };
            double y = 0;
            DxfDocument doc = new DxfDocument(netDxf.Header.DxfVersion.AutoCad2018);
            foreach (string input in inputs)
            {
                doc.Entities.Add(new Insert(ImportDxfAsBlock(input), new Vector2(0, y)));
                y += 5000;
            }
            doc.Save("../../../merged.dxf");
        }
    }
}
