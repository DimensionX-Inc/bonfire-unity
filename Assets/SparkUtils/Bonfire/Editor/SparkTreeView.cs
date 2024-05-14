using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DimX.SparkUtils
{
    public class SparkTreeView : TreeView
    {
        private string[] _filePaths;
        public static event Action<IList<TreeViewItem>> OnSelectionChanged;
        public static event Action<SparkTreeViewItem> OnItemSelected;
        
        public string RootFolder { get; set; }

        public string[] Files
        {
            get => _filePaths;
            set
            {
                // Clear results
                _filePaths = Array.Empty<string>();
                Reload();
                
                _filePaths = value;
                
                if(_filePaths.Length > 0)
                {
                    Reload();
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            IList<TreeViewItem> selection = FindRows(selectedIds);
            OnSelectionChanged?.Invoke(selection);
        }

        protected override void DoubleClickedItem(int id)
        {
            SparkTreeViewItem treeViewItem = (SparkTreeViewItem)GetRows().First(x => x.id == id);
            OnItemSelected?.Invoke(treeViewItem);
        }

        public SparkTreeView(TreeViewState state, 
                             MultiColumnHeader multiColumnHeader, 
                             string rootFolder,
                             IEnumerable<string> filePaths) 
            : base(state, multiColumnHeader)
        {
            RootFolder = rootFolder;
            Files = filePaths.ToArray();
            rowHeight = 75;
            
            multiColumnHeader.sortingChanged += HandleSortingChanged;
        }

        private void HandleSortingChanged(MultiColumnHeader multicolumnheader)
        {
            if (_filePaths.Length <= 1)
                return;

            int sortedColumnIndex = multicolumnheader.sortedColumnIndex;
            
            if (sortedColumnIndex == -1)
            {
                return;
            }

            bool ascending = multiColumnHeader.IsSortedAscending(sortedColumnIndex);
            IList<SparkTreeViewItem> rows = GetRows().Cast<SparkTreeViewItem>().ToList();
            
            switch (sortedColumnIndex)
            {
               case 1: // Spark Name
                   SortRows(rows, x => x.Name, ascending);
                   break;
               case 2: // Type
                   SortRows(rows, x => x.Type, ascending);
                   break;
               case 3: // Spark Guid
                   SortRows(rows, x => x.Guid.ToString(), ascending);
                   break;
               case 4: // Spark Path
                   SortRows(rows, x => x.Path, ascending);
                   break;
            }
        }

        private void SortRows(IList<SparkTreeViewItem> rows, Func<SparkTreeViewItem, string> predicate, bool isAscending)
        {
            IOrderedEnumerable<SparkTreeViewItem> results = isAscending ? 
                                                            rows.OrderBy(predicate) : 
                                                            rows.OrderByDescending(predicate);
            
            Files = results.Select(x => x.Path).ToArray();
            DeselectAll();
        }
        
        public void DeselectAll()
        {
            SetSelection(new List<int>());
            OnSelectionChanged?.Invoke(new List<TreeViewItem>());
        }
        
        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem {id = -1, depth = -1, displayName = "Root"};

            if (!_filePaths.Any())
            {
                root.AddChild(new TreeViewItem());
                return root;
            }
            
            for (int i = 0; i < _filePaths.Count(); i++)
            {
                root.AddChild(new SparkTreeViewItem(i, 0, string.Empty, _filePaths[i]));
            }
            
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                Rect cellRect = args.GetCellRect(i);
                SparkTreeViewItem row = (SparkTreeViewItem)args.item;
            
                switch (args.GetColumn(i))
                {
                    case 0: // Preview
                        GUI.DrawTexture(cellRect, row.Preview, ScaleMode.ScaleToFit);
                        break;
                    case 1: // Spark Name
                        EditorGUI.LabelField(cellRect, row.Name);
                        break;
                    case 2: // Type
                        EditorGUI.LabelField(cellRect, row.Type);
                        break;
                    case 3: // Spark Guid
                        EditorGUI.LabelField(cellRect, row.Guid.ToString());
                        break;
                    case 4: // Spark Path
                        string path = row.Path.Substring(RootFolder.Length, (row.Path.Length - (RootFolder.Length - 1)) - 1);
                        EditorGUI.LabelField(cellRect, path);
                        break;
                }
            }
        }

        public void Resort()
        {
            HandleSortingChanged(multiColumnHeader);
        }
    }
}