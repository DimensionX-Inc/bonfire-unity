using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DimX.Common.Assets.Types.Common;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DimX.SparkUtils
{
    public class SparkBrowserWindow : EditorWindow
    {
        private SparkTreeView _sparkTreeView;
        private string _path;
        private string[] _files;
        private IList<TreeViewItem> _selectedSparks = new List<TreeViewItem>();
        private bool _isInitializing;

        private void RegisterCallbacks()
        {
            UnityEditor.Compilation.CompilationPipeline.compilationFinished += HandleCompilationFinished;
            SparkTreeView.OnSelectionChanged += HandleSelectionChanged;
            SparkTreeView.OnItemSelected += HandleOnItemSelected;
        }

        private void DeRegisterCallbacks()
        {
            UnityEditor.Compilation.CompilationPipeline.compilationFinished -= HandleCompilationFinished;
            SparkTreeView.OnItemSelected -= HandleOnItemSelected;
            SparkTreeView.OnSelectionChanged -= HandleSelectionChanged;
        }
            
        private void HandleCompilationFinished(object obj)
        {
            DeRegisterCallbacks();
            Close();
        }

        private void OnDestroy()
        {
            DeRegisterCallbacks();
        }

        public static void ShowWindow()
        {
            SparkBrowserWindow window = CreateInstance<SparkBrowserWindow>();
            window.titleContent = new GUIContent("Spark Browser");
            window.RegisterCallbacks();
            window.Show();
        }
        
        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_path))
            {
                _path = Path.Combine(Constants.AssetRoot, "Sparks");
                Directory.CreateDirectory(_path); // Just in case...
            }
            
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                _path = EditorGUILayout.TextField(_path);
                GUI.enabled = true;

                if (GUILayout.Button("Browse", GUILayout.Width(75)))
                {
                    string selected = EditorUtility.OpenFolderPanel("Select Folder", _path, "string.Empty");
                    if (!string.IsNullOrEmpty(selected))
                    {
                        _path = selected;
                        _selectedSparks = new List<TreeViewItem>();
                        RefreshTreeView();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
            
            if(_selectedSparks.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Delete", GUILayout.Width(75)))
                    {
                        bool canDelete = EditorUtility.DisplayDialog("Confirm Delete",
                                                                     "Are you sure you want to delete the selected spark(s)?",
                                                                     "Ok", 
                                                                     "Cancel");

                        if (canDelete)
                        {
                            DeleteSelected();
                        }
                    }

                    if (GUILayout.Button("Move", GUILayout.Width(75)))
                    {
                        string destination = EditorUtility.OpenFolderPanel("Select Folder", _path, "string.Empty");
                        
                        if (!string.IsNullOrEmpty(destination))
                        {
                            bool canMove = EditorUtility.DisplayDialog("Confirm Move",
                                                                       "Are you sure you want to move the selected spark(s)?",
                                                                       "Ok",
                                                                       "Cancel");

                            if (canMove)
                            {
                                MoveSelected(destination);
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            if(_sparkTreeView == null)
            {
                RefreshTreeView();
            }
            else
            {
                if (_sparkTreeView.Files.Length > 0)
                {
                    _sparkTreeView.OnGUI(new Rect(5, 70, position.width - 10, position.height - 75));
                }
            }

            if (_sparkTreeView.Files.Length == 0)
            {
                GUILayout.Label("No Sparks found");
            }
        }


        private void SaveSelected(string path, string name, string type, Guid guid, byte[] preview)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogError("{SaveSelected} path missing.");    
            }
            
            try  
            {  
                // Extract original spark using the new/unchanged guid
                var extractionFolder = Path.Combine(Application.temporaryCachePath, guid.ToString());  
  
                if (Directory.Exists(extractionFolder))  
                {  
                    Directory.Delete(extractionFolder,true);  
                }  
                
                ZipFile.ExtractToDirectory(path, extractionFolder);
                
                // Replace metafile.txt
                Metadata metadata = new Metadata
                {
                    Guid = guid,
                    Name = name,
                    Type = type
                };

                string metadataFilePath = Path.Combine(extractionFolder, "Metadata.txt");
                if (File.Exists(metadataFilePath))
                {
                    File.Delete(metadataFilePath);
                }
                
                string text = JsonUtility.ToJson(metadata, true);
                File.WriteAllText(metadataFilePath, text);
                
                // Write preview.png
                string previewFilePath = Path.Combine(extractionFolder, "Preview.png");
                File.WriteAllBytes(previewFilePath, preview);
                
                // Delete original file
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                
                // Compress and place in original directory
                string fileOut = Path.Combine(Path.GetDirectoryName(path), $"{guid}.dimxs");
                BuildUtilities.Compress(extractionFolder, fileOut);
            }  
            catch (Exception ex)  
            {  
                Debug.LogError(ex.Message);
            }  

            RefreshTreeView();
        }
        
        private void DeleteSelected()
        {
            foreach (TreeViewItem selected in _selectedSparks)
            {
                SparkTreeViewItem item = (SparkTreeViewItem)selected;

                if (!File.Exists(item.Path))
                {
                    continue;
                }
                
                File.Delete(item.Path);
            }
            
            RefreshTreeView();
        }

        private void MoveSelected(string destinationPath)
        {
            foreach (TreeViewItem selected in _selectedSparks)
            {
                SparkTreeViewItem item = (SparkTreeViewItem)selected;

                if (!File.Exists(item.Path))
                {
                    continue;
                }

                string destination = Path.Combine(destinationPath, String.Concat(item.Guid, ".dimxs"));

                if (File.Exists(destination))
                {
                    continue;
                }
                
                File.Move(item.Path, destination);
            }
            
            RefreshTreeView();
        }

        private void RefreshTreeView()
        {
            if (_isInitializing || string.IsNullOrWhiteSpace(_path))
            {
                return;
            }
            
            _isInitializing = true;
            
           IEnumerable<string> filesInDirectory = Directory.EnumerateFiles(_path, "*.dimxs", SearchOption.AllDirectories);
            
            string[] files = filesInDirectory as string[] ?? filesInDirectory.ToArray();
            
            if (_sparkTreeView == null)
            {
                _sparkTreeView = new SparkTreeView(new TreeViewState(), CreateMultiColumnHeader(), _path, files);
            }

            _sparkTreeView.RootFolder = _path;
            _sparkTreeView.Files = files;
            
            if(files.Length > 0 )
            {
                _sparkTreeView.DeselectAll();
                _sparkTreeView.Resort();
            }
            
            _isInitializing = false;
        }

        private void HandleSelectionChanged(IList<TreeViewItem> items)
        {
            _selectedSparks = items;
        }

        private void HandleOnItemSelected(TreeViewItem item)
        {
            SparkEditWindow.ShowWindow((SparkTreeViewItem)item, SaveSelected);
        }

        private MultiColumnHeader CreateMultiColumnHeader()
        {
            MultiColumnHeaderState.Column[] columns = {
                new()
                {
                    headerContent = new GUIContent("Preview"),
                    width = 150,
                    minWidth = 100,
                    maxWidth = 500,
                    autoResize = true,
                    canSort = false,
                    headerTextAlignment = TextAlignment.Center
                },
                new()
                {
                    headerContent = new GUIContent("Name"),
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 300,
                    minWidth = 300,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
                new()
                {
                    headerContent = new GUIContent("Type"),
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 300,
                    minWidth = 300,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
                new()
                {
                    headerContent = new GUIContent("GUID"),
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 300,
                    minWidth = 300,
                    maxWidth = 500,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
                new()
                {
                    headerContent = new GUIContent("Path"),
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 600,
                    minWidth = 400,
                    autoResize = true,
                    headerTextAlignment = TextAlignment.Center
                },
            };

            MultiColumnHeader columnHeader = new MultiColumnHeader(new MultiColumnHeaderState(columns));
            columnHeader.height = 25;
            columnHeader.ResizeToFit();
            return columnHeader;
        }
    }
}