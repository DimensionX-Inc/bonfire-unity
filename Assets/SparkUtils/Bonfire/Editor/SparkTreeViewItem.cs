using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DimX.Common.Assets.Types.Common;
using DimX.Common.Utilities;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DimX.SparkUtils
{
    public class SparkTreeViewItem: TreeViewItem
    {
        private Metadata _metadata;
        
        public Texture2D Preview { get; private set; }

        public string Path { get; }

        public string Name { get; private set; }

        public string Type { get; private set; }
        
        public Guid Guid { get; private set; }

        
        public SparkTreeViewItem(int id, int depth, string displayName, string path)
                        : base(id, depth, displayName)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            Path = path;
            Preview = new Texture2D(2, 2);
            
            Load();
        }

        private void Load()
        {
            using var file = File.OpenRead(Path);
            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
            
            ZipArchiveEntry entry = zip.Entries.FirstOrDefault(x => x.Name.ToLower() == "metadata.txt");
            if (entry == null)
            {
                return;
            }

            using StreamReader s = new StreamReader(entry.Open());
            string json = s.ReadToEnd();
            _metadata = Json.Deserialize<Metadata>(json);
            Name = _metadata.Name;
            Guid = _metadata.Guid;
            Type = _metadata.Type;
            
            entry = zip.Entries.FirstOrDefault(x => x.Name.ToLower() == "preview.png");
            if (entry == null)
            {
                return;
            }

            using StreamReader reader = new StreamReader(entry.Open());
            using MemoryStream memoryStream = new MemoryStream();
            reader.BaseStream.CopyTo(memoryStream);
            Preview.LoadImage(memoryStream.ToArray());
        }
    }
}