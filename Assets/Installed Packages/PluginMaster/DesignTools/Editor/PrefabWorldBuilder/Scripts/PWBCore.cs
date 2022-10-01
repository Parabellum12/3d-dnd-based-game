/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PluginMaster
{
    #region CORE
    public static class PWBCore
    {
        public const string PARENT_COLLIDER_NAME = "PluginMasterPrefabPaintTempMeshColliders";
        private static GameObject _parentCollider = null;
        private static GameObject parentCollider
        {
            get
            {
                if (_parentCollider == null)
                {
                    _parentCollider = new GameObject(PWBCore.PARENT_COLLIDER_NAME);
                    _parentColliderId = _parentCollider.GetInstanceID();
                    _parentCollider.hideFlags = HideFlags.HideAndDontSave;
                }
                return _parentCollider;
            }
        }
        private static int _parentColliderId = -1;
        public static int parentColliderId => _parentColliderId;
        #region DATA
        private static PWBData _staticData = null;
        public static bool staticDataWasInitialized => _staticData != null;
        public static PWBData staticData
        {
            get
            {
                if (_staticData != null) return _staticData;
                LoadFromFile();
                return _staticData;
            }
        }

        public static void LoadFromFile()
        {
            var text = PWBData.LoadText();
            if (text == null)
            {
                _staticData = new PWBData();
                _staticData.Save();
            }
            else
            {
                _staticData = JsonUtility.FromJson<PWBData>(text);
                foreach (var palette in PaletteManager.paletteData)
                    foreach (var brush in palette.brushes)
                        foreach (var item in brush.items) item.InitializeParentSettings(brush);
            }
        }

        public static void SetSavePending()
        {
            AutoSave.QuickSave();
            staticData.SetSavePending();
        }

        #endregion
        #region TEMP COLLIDERS
        private static Dictionary<int, GameObject> _tempCollidersIds = new Dictionary<int, GameObject>();
        private static Dictionary<int, GameObject> _tempCollidersTargets = new Dictionary<int, GameObject>();
        private static Dictionary<int, List<int>> _tempCollidersTargetParentsIds = new Dictionary<int, List<int>>();
        private static Dictionary<int, List<int>> _tempCollidersTargetChildrenIds = new Dictionary<int, List<int>>();
        public static bool CollidersContains(GameObject[] selection, string colliderName)
        {
            int objId;
            if (!int.TryParse(colliderName, out objId)) return false;
            foreach (var obj in selection)
                if (obj.GetInstanceID() == objId)
                    return true;
            return false;
        }

        public static bool IsTempCollider(int instanceId) => _tempCollidersIds.ContainsKey(instanceId);

        public static GameObject GetGameObjectFromTempColliderId(int instanceId) => _tempCollidersIds[instanceId];

        public static void UpdateTempColliders()
        {
            DestroyTempColliders();
            PWBIO.UpdateOctree();
            var allTransforms = GameObject.FindObjectsOfType<Transform>();
            foreach (var transform in allTransforms)
            {
                if (!transform.gameObject.activeInHierarchy) continue;
                if (transform.parent != null) continue;
                AddTempCollider(transform.gameObject);
            }
        }

        public static void AddTempCollider(GameObject obj)
        {
            bool IsPrimitive(Mesh mesh)
            {
                var assetPath = AssetDatabase.GetAssetPath(mesh);
                return assetPath.Length < 6 ? false : assetPath.Substring(0, 6) != "Assets";
            }

            void AddParentsIds(GameObject target)
            {
                var parents = target.GetComponentsInParent<Transform>();
                foreach (var parent in parents)
                {
                    if (!_tempCollidersTargetParentsIds.ContainsKey(target.GetInstanceID()))
                        _tempCollidersTargetParentsIds.Add(target.GetInstanceID(), new List<int>());
                    _tempCollidersTargetParentsIds[target.GetInstanceID()].Add(parent.gameObject.GetInstanceID());
                    if (!_tempCollidersTargetChildrenIds.ContainsKey(parent.gameObject.GetInstanceID()))
                        _tempCollidersTargetChildrenIds.Add(parent.gameObject.GetInstanceID(), new List<int>());
                    _tempCollidersTargetChildrenIds[parent.gameObject.GetInstanceID()].Add(target.GetInstanceID());
                }
            }

            void CreateTempCollider(GameObject target, Mesh mesh)
            {
                var differentVertices = new List<Vector3>();
                foreach (var vertex in mesh.vertices)
                {
                    if (!differentVertices.Contains(vertex)) differentVertices.Add(vertex);
                    if (differentVertices.Count >= 3) break;
                }
                if (differentVertices.Count < 3) return;
                if (_tempCollidersTargets.ContainsKey(target.GetInstanceID())) return;
                var name = target.GetInstanceID().ToString();
                var tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), target);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = target.transform.position;
                tempObj.transform.rotation = target.transform.rotation;
                tempObj.transform.localScale = target.transform.lossyScale;
                _tempCollidersTargets.Add(target.GetInstanceID(), tempObj);
                AddParentsIds(target);

                if (IsPrimitive(mesh))
                {
                    if (mesh.name == "Sphere") tempObj.AddComponent<SphereCollider>();
                    else if (mesh.name == "Capsule") tempObj.AddComponent<CapsuleCollider>();
                    else if (mesh.name == "Cube") tempObj.AddComponent<BoxCollider>();
                }
                else
                {
                    var meshCollider = tempObj.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                }
            }

            bool ObjectIsActiveAndWithoutCollider(GameObject go)
            {
                if (!go.activeInHierarchy) return false;
                var collider = go.GetComponent<Collider>();
                if (collider == null) return true;
                if (collider is MeshCollider)
                {
                    var meshCollider = collider as MeshCollider;
                    if (meshCollider.sharedMesh == null) return true;
                }
                return collider.isTrigger;
            }

            var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (!ObjectIsActiveAndWithoutCollider(meshFilter.gameObject)) continue;
                CreateTempCollider(meshFilter.gameObject, meshFilter.sharedMesh);
            }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (!ObjectIsActiveAndWithoutCollider(renderer.gameObject)) continue;
                CreateTempCollider(renderer.gameObject, renderer.sharedMesh);
            }

            var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                var target = spriteRenderer.gameObject;
                if (!target.activeInHierarchy) continue;
                if (_tempCollidersTargets.ContainsKey(target.GetInstanceID())) return;
                var name = spriteRenderer.gameObject.GetInstanceID().ToString();
                var tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), spriteRenderer.gameObject);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = spriteRenderer.transform.position;
                tempObj.transform.rotation = spriteRenderer.transform.rotation;
                tempObj.transform.localScale = spriteRenderer.transform.lossyScale;
                _tempCollidersTargets.Add(target.GetInstanceID(), tempObj);
                AddParentsIds(target);
                var boxCollider = tempObj.AddComponent<BoxCollider>();
                boxCollider.size = (Vector3)(spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit)
                    + new Vector3(0f, 0f, 0.01f);
                var collider = spriteRenderer.GetComponent<Collider2D>();
                if (collider != null && !collider.isTrigger) continue;
                tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), spriteRenderer.gameObject);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = spriteRenderer.transform.position;
                tempObj.transform.rotation = spriteRenderer.transform.rotation;
                tempObj.transform.localScale = spriteRenderer.transform.lossyScale;
                var boxCollider2D = tempObj.AddComponent<BoxCollider2D>();
                boxCollider2D.size = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
            }
        }

        public static void DestroyTempColliders()
        {
            _tempCollidersIds.Clear();
            _tempCollidersTargets.Clear();
            _tempCollidersTargetParentsIds.Clear();
            _tempCollidersTargetChildrenIds.Clear();
            var parentObj = GameObject.Find(PWBCore.PARENT_COLLIDER_NAME);
            if (parentObj != null) GameObject.DestroyImmediate(parentObj);
            _parentColliderId = -1;
        }


        public static void UpdateTempCollidersTransforms(GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                var parentId = obj.GetInstanceID();
                bool isParent = false;
                foreach (var childId in _tempCollidersTargetParentsIds.Keys)
                {
                    var parentsIds = _tempCollidersTargetParentsIds[childId];
                    if (parentsIds.Contains(parentId))
                    {
                        isParent = true;
                        break;
                    }
                }
                if (!isParent) continue;
                foreach (var id in _tempCollidersTargetChildrenIds[parentId])
                {
                    var tempCollider = _tempCollidersTargets[id];
                    if (tempCollider == null) continue;
                    var childObj = (GameObject)EditorUtility.InstanceIDToObject(id);
                    if (childObj == null) continue;
                    tempCollider.transform.position = childObj.transform.position;
                    tempCollider.transform.rotation = childObj.transform.rotation;
                    tempCollider.transform.localScale = childObj.transform.lossyScale;
                }
            }
        }

        public static void SetActiveTempColliders(GameObject[] objects, bool value)
        {
            foreach (var obj in objects)
            {
                var parentId = obj.GetInstanceID();
                bool isParent = false;
                foreach (var childId in _tempCollidersTargetParentsIds.Keys)
                {
                    var parentsIds = _tempCollidersTargetParentsIds[childId];
                    if (parentsIds.Contains(parentId))
                    {
                        isParent = true;
                        break;
                    }
                }
                if (!isParent) continue;
                foreach (var id in _tempCollidersTargetChildrenIds[parentId])
                {
                    var tempCollider = _tempCollidersTargets[id];
                    if (tempCollider == null) continue;
                    var childObj = (GameObject)EditorUtility.InstanceIDToObject(id);
                    if (childObj == null) continue;
                    tempCollider.SetActive(value);
                    tempCollider.transform.position = childObj.transform.position;
                    tempCollider.transform.rotation = childObj.transform.rotation;
                    tempCollider.transform.localScale = childObj.transform.lossyScale;
                }
            }
        }

        public static GameObject[] GetTempColliders(GameObject obj)
        {
            var parentId = obj.GetInstanceID();
            bool isParent = false;
            foreach (var childId in _tempCollidersTargetParentsIds.Keys)
            {
                var parentsIds = _tempCollidersTargetParentsIds[childId];
                if (parentsIds.Contains(parentId))
                {
                    isParent = true;
                    break;
                }
            }
            if (!isParent) return null;
            var tempColliders = new List<GameObject>();
            foreach (var id in _tempCollidersTargetChildrenIds[parentId])
            {
                var tempCollider = _tempCollidersTargets[id];
                if (tempCollider == null) continue;
                tempColliders.Add(tempCollider);
            }
            return tempColliders.ToArray();
        }
        #endregion
    }
    #endregion


    [Serializable]
    public class PWBData
    {
        private const string FILE_NAME = "PWBData";
        public const string RELATIVE_TOOL_DIR = "/PluginMaster/DesignTools/Editor/PrefabWorldBuilder/";
        public const string RELATIVE_RESOURCES_DIR = RELATIVE_TOOL_DIR + "Resources/";
        private const string VERSION = "2.4";
        [SerializeField] private string _version = VERSION;
        [SerializeField] private string _rootDirectory = null;
        [SerializeField] private int _autoSavePeriodMinutes = 1;
        [SerializeField] private bool _undoBrushProperties = true;
        [SerializeField] private bool _undoPalette = true;
        [SerializeField] private int _controlPointSize = 1;
        [SerializeField] private bool _closeAllWindowsWhenClosingTheToolbar = false;
        [SerializeField] private int _thumbnailLayer = 7;
        public enum UnsavedChangesAction { ASK, SAVE, DISCARD }
        [SerializeField] private UnsavedChangesAction _unsavedChangesAction = UnsavedChangesAction.ASK;
        [SerializeField] private PaletteManager _paletteManager = PaletteManager.instance;

        [SerializeField] private PinManager pinManager = PinManager.instance as PinManager;
        [SerializeField] private BrushManager _brushManager = BrushManager.instance as BrushManager;
        [SerializeField] private GravityToolManager _gravityToolManager = GravityToolManager.instance as GravityToolManager;
        [SerializeField] private LineManager _lineManager = LineManager.instance as LineManager;
        [SerializeField] private ShapeManager _shapeManager = ShapeManager.instance as ShapeManager;
        [SerializeField] private TilingManager _tilingManager = TilingManager.instance as TilingManager;
        [SerializeField] private ReplacerManager _replacerManager = ReplacerManager.instance as ReplacerManager;
        [SerializeField] private EraserManager _eraserManager = EraserManager.instance as EraserManager;

        [SerializeField]
        private SelectionToolManager _selectionToolManager = SelectionToolManager.instance as SelectionToolManager;
        [SerializeField] private ExtrudeManager _extrudeSettings = ExtrudeManager.instance as ExtrudeManager;
        [SerializeField] private MirrorManager _mirrorManager = MirrorManager.instance as MirrorManager;

        [SerializeField] private SnapManager _snapManager = new SnapManager();
        private bool _savePending = false;

        public string version => _version;
        public int autoSavePeriodMinutes
        {
            get => _autoSavePeriodMinutes;
            set
            {
                value = Mathf.Clamp(value, 1, 10);
                if (_autoSavePeriodMinutes == value) return;
                _autoSavePeriodMinutes = value;
                Save();
            }
        }

        public bool undoBrushProperties
        {
            get => _undoBrushProperties;
            set
            {
                if (_undoBrushProperties == value) return;
                _undoBrushProperties = value;
                Save();
            }
        }

        public bool undoPalette
        {
            get => _undoPalette;
            set
            {
                if (_undoPalette == value) return;
                _undoPalette = value;
                Save();
            }
        }

        public int controPointSize
        {
            get => _controlPointSize;
            set
            {
                if (_controlPointSize == value) return;
                _controlPointSize = value;
                Save();
            }
        }

        public bool closeAllWindowsWhenClosingTheToolbar
        {
            get => _closeAllWindowsWhenClosingTheToolbar;
            set
            {
                if (_closeAllWindowsWhenClosingTheToolbar == value) return;
                _closeAllWindowsWhenClosingTheToolbar = value;
                Save();
            }
        }

        public int thumbnailLayer
        {
            get => _thumbnailLayer;
            set
            {
                value = Mathf.Clamp(value, 0, 31);
                if (_thumbnailLayer == value) return;
                _thumbnailLayer = value;
                Save();
            }
        }

        public UnsavedChangesAction unsavedChangesAction
        {
            get => _unsavedChangesAction;
            set
            {
                if (_unsavedChangesAction == value) return;
                _unsavedChangesAction = value;
                Save();
            }
        }
        public void SetSavePending() => _savePending = true;

        public void VersionUpdate()
        {
            var currentText = LoadText();
            if (currentText == null) return;
            var dataVersion = JsonUtility.FromJson<PWBDataVersion>(currentText);
            if (dataVersion.IsOlderThan("1.10"))
            {
                var v1_9_data = JsonUtility.FromJson<V1_9_PWBData>(currentText);
                var v1_9_sceneItems = v1_9_data._lineManager._unsavedProfile._sceneLines;
                if (v1_9_sceneItems == null || v1_9_sceneItems.Length == 0) return;
                foreach (var v1_9_sceneData in v1_9_sceneItems)
                {
                    var v1_9_sceneLines = v1_9_sceneData._lines;
                    if (v1_9_sceneItems == null || v1_9_sceneItems.Length == 0) return;
                    foreach (var v1_9_sceneLine in v1_9_sceneLines)
                    {
                        if (v1_9_sceneLines == null || v1_9_sceneLines.Length == 0) return;
                        var lineData = new LineData(v1_9_sceneLine._id,  v1_9_sceneLine._data._controlPoints,
                            v1_9_sceneLine._objectPoses, v1_9_sceneLine._initialBrushId,
                            v1_9_sceneLine._data._closed, v1_9_sceneLine._settings);
                        LineManager.instance.AddPersistentItem(v1_9_sceneData._sceneGUID, lineData);
                    }
                }
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_rootDirectory)) _rootDirectory = Application.dataPath;
            var fullDirectoryPath = _rootDirectory + RELATIVE_RESOURCES_DIR;
            var fileName = FILE_NAME + ".txt";
            var fullFilePath = fullDirectoryPath + fileName;
            if (!File.Exists(fullFilePath))
            {
                var directories = Directory.GetDirectories(Application.dataPath, "PluginMaster", SearchOption.AllDirectories);
                if (directories.Length == 0) Directory.CreateDirectory(fullDirectoryPath);
                else
                {
                    _rootDirectory = Directory.GetParent(directories[0]).FullName;
                    fullDirectoryPath = _rootDirectory + RELATIVE_RESOURCES_DIR;
                }
                if (!Directory.Exists(fullDirectoryPath)) Directory.CreateDirectory(fullDirectoryPath);
            }
            fullFilePath = fullDirectoryPath + fileName;
            VersionUpdate();
            _version = VERSION;
            var jsonString = JsonUtility.ToJson(this);
            File.WriteAllText(fullFilePath, jsonString);
            AssetDatabase.Refresh();
            _savePending = false;
        }

        public static string LoadText()
        {
            var textAssets = Resources.LoadAll<TextAsset>(FILE_NAME);
            string loadedText = null;
            var dPath = PWBData.dataPath;
            bool IsDataPath(TextAsset textAsset, out string assetPath)
            {
                assetPath = AssetDatabase.GetAssetPath(textAsset);
                return dPath.Contains(assetPath);
            }

            if (textAssets.Length == 0) return null;
            else if (textAssets.Length == 1)
            {
                loadedText = textAssets[0].text;
                if (!IsDataPath(textAssets[0], out string assetPath)) AssetDatabase.DeleteAsset(assetPath);
                return loadedText;
            }

            foreach (var textAsset in textAssets)
            {
                if (IsDataPath(textAsset, out string assetPath)) loadedText = textAsset.text;
                else AssetDatabase.DeleteAsset(assetPath);
            }
            return loadedText;
        }

        public void SaveIfPending() { if (_savePending) Save(); }

        public static string dataPath
        {
            get
            {
                var fullDirectoryPath = Application.dataPath + RELATIVE_RESOURCES_DIR;
                var fileName = FILE_NAME + ".txt";
                var fullFilePath = fullDirectoryPath + fileName;
                if (!File.Exists(fullFilePath))
                {
                    var directories = Directory.GetDirectories(Application.dataPath,
                        "PluginMaster", SearchOption.AllDirectories);
                    if (directories.Length == 0) Directory.CreateDirectory(fullDirectoryPath);
                    else fullDirectoryPath = Directory.GetParent(directories[0]).FullName + RELATIVE_RESOURCES_DIR;
                    if (!Directory.Exists(fullDirectoryPath)) Directory.CreateDirectory(fullDirectoryPath);
                    fullFilePath = fullDirectoryPath + fileName;
                }
                fullFilePath = fullFilePath.Replace('\\', '/');
                return fullFilePath;
            }
        }

        public string thumbnailsPath
        {
            get
            {
                var path = _rootDirectory + RELATIVE_RESOURCES_DIR + "Thumbnails/";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }
        public string documentationPath
        {
            get
            {
                var absolutePath = _rootDirectory + RELATIVE_TOOL_DIR
                    + "Documentation/Prefab World Builder Documentation.pdf";
                var relativepath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                return relativepath;

            }
        }
    }

    [InitializeOnLoad]
    public static class ApplicationEventHandler
    {
        static ApplicationEventHandler()
        {
            EditorApplication.playModeStateChanged += OnStateChanged;
            EditorApplication.quitting += PWBCore.staticData.Save;
        }

        private static void OnStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
                PWBCore.staticData.SaveIfPending();
        }
    }

    [InitializeOnLoad]
    public static class AutoSave
    {
        private static int _quickSaveCount = 3;

        static AutoSave()
        {
            PeriodicSave();
            PeriodicQuickSave();
        }

        private async static void PeriodicSave()
        {
            if (PWBCore.staticDataWasInitialized)
            {
                await Task.Delay(PWBCore.staticData.autoSavePeriodMinutes * 60000);
                PWBCore.staticData.SaveIfPending();
            }
            else await Task.Delay(60000);
            PeriodicSave();
        }

        private async static void PeriodicQuickSave()
        {
            await Task.Delay(300);
            ++_quickSaveCount;
            if (_quickSaveCount == 3 && PWBCore.staticDataWasInitialized) PWBCore.staticData.Save();
            PeriodicQuickSave();
        }

        public static void QuickSave() => _quickSaveCount = 0;
    }
    #region VERSION
    [Serializable]
    public class PWBDataVersion
    {
        [SerializeField] public string _version;
        public bool IsOlderThan(string value)
        {
            var intArray = GetIntArray(_version);
            var otherIntArray = GetIntArray(value);
            var minLength = Mathf.Min(intArray.Length, otherIntArray.Length);
            for (int i = 0; i < minLength; ++i) if (intArray[i] < otherIntArray[i]) return true;
            return false;
        }
        private static int[] GetIntArray(string value)
        {
            var stringArray = value.Split('.');
            if (stringArray.Length == 0) return new int[] { 1, 0 };
            var intArray = new int[stringArray.Length];
            for (int i = 0; i < intArray.Length; ++i) intArray[i] = int.Parse(stringArray[i]);
            return intArray;
        }
    }
    #endregion

    #region DATA 1.9
    [Serializable]
    public class V1_9_LineData
    {
        [SerializeField] public LinePoint[] _controlPoints;
        [SerializeField] public bool _closed;
    }

    [Serializable]
    public class V1_9_PersistentLineData
    {
        [SerializeField] public long _id;
        [SerializeField] public long _initialBrushId;
        [SerializeField] public V1_9_LineData _data;
        [SerializeField] public LineSettings _settings;
        [SerializeField] public ObjectPose[] _objectPoses;
    }

    [Serializable]
    public class V1_9_SceneLines
    {
        [SerializeField] public string _sceneGUID;
        [SerializeField] public V1_9_PersistentLineData[] _lines;
    }

    [Serializable]
    public class V1_9_Profile
    {
        [SerializeField] public V1_9_SceneLines[] _sceneLines;
    }

    [Serializable]
    public class V1_9_LineManager
    {
        [SerializeField] public V1_9_Profile _unsavedProfile;
    }

    [Serializable]
    public class V1_9_PWBData
    {
        [SerializeField] public V1_9_LineManager _lineManager;
    }
    #endregion
}