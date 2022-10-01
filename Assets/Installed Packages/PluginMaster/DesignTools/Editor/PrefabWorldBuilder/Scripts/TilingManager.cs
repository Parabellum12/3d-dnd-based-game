/*
Copyright (c) 2021 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2021.

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
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PluginMaster
{
    #region DATA & SETTINGS
    [Serializable]
    public class TilingSettings : PaintOnSurfaceToolSettings, IPaintToolSettings
    {
        #region TILING SETTINGS

        public enum CellSizeType
        {
            SMALLEST_OBJECT,
            BIGGEST_OBJECT,
            CUSTOM
        }

        [SerializeField] private CellSizeType _cellSizeType = CellSizeType.SMALLEST_OBJECT;
        [SerializeField] private Vector2 _cellSize = Vector2.one;
        [SerializeField] private Quaternion _rotation = Quaternion.identity;
        [SerializeField] private Vector2 _spacing = Vector2.zero;
        [SerializeField] private AxesUtils.SignedAxis _axisAlignedWithNormal = AxesUtils.SignedAxis.UP;
        public Action<float, Vector3> OnRotationChanged;
        public Quaternion rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                var prevRotation = _rotation;
                _rotation = value;
                OnDataChanged();
                if (OnRotationChanged != null)
                {
                    var angle = Quaternion.Angle(prevRotation, _rotation);
                    var axis = Vector3.Cross(prevRotation * Vector3.forward,_rotation * Vector3.forward);
                    if (axis == Vector3.zero) axis = Vector3.Cross(prevRotation * Vector3.up, _rotation * Vector3.up);
                    axis.Normalize();
                    OnRotationChanged(angle, axis);
                }
            }
        }
        public CellSizeType cellSizeType
        {
            get => _cellSizeType;
            set
            {
                if (_cellSizeType == value) return;
                _cellSizeType = value;
                UpdateCellSize();
            }
        }
        public Vector2 cellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize == value) return;
                _cellSize = value;
                OnDataChanged();
            }
        }
        public Vector2 spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                OnDataChanged();
            }
        }
        public AxesUtils.SignedAxis axisAlignedWithNormal
        {
            get => _axisAlignedWithNormal;
            set
            {
                if (_axisAlignedWithNormal == value) return;
                _axisAlignedWithNormal = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public void UpdateCellSize()
        {
            if (ToolManager.tool != ToolManager.PaintTool.TILING) return;
            if (_cellSizeType != CellSizeType.CUSTOM)
            {
                if (PaletteManager.selectedBrush == null) return;
                var cellSize = Vector3.one * (_cellSizeType == CellSizeType.SMALLEST_OBJECT
                    ? float.MaxValue : float.MinValue);
                foreach (var item in PaletteManager.selectedBrush.items)
                {
                    var prefab = item.prefab;
                    if (prefab == null) continue;
                    var scaleMultiplier = _cellSizeType == CellSizeType.SMALLEST_OBJECT
                        ? item.minScaleMultiplier : item.maxScaleMultiplier;
                    var itemSize = Vector3.Scale(BoundsUtils.GetBoundsRecursive(prefab.transform).size,
                        scaleMultiplier);
                    cellSize = _cellSizeType == CellSizeType.SMALLEST_OBJECT
                        ? Vector3.Min(cellSize, itemSize) : Vector3.Max(cellSize, itemSize);
                }
                if (_axisAlignedWithNormal.axis == AxesUtils.Axis.Y) cellSize.y = cellSize.z;
                else if (_axisAlignedWithNormal.axis == AxesUtils.Axis.X)
                {
                    cellSize.x = cellSize.y;
                    cellSize.y = cellSize.z;
                }
                if (cellSize.x == 0) cellSize.x = 0.5f;
                if (cellSize.y == 0) cellSize.y = 0.5f;
                if (cellSize.z == 0) cellSize.z = 0.5f;
                _cellSize = cellSize;
                ToolProperties.RepainWindow();
                SceneView.RepaintAll();
            }
            OnDataChanged();
        }
        #endregion

        #region ON DATA CHANGED
        public TilingSettings() : base() => _paintTool.OnDataChanged += DataChanged;

        public override void DataChanged()
        {
            base.DataChanged();
            PWBIO.UpdateStroke();
        }
        #endregion

        #region PAINT TOOL
        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public Transform parent { get => _paintTool.parent; set => _paintTool.parent = value; }
        public bool overwritePrefabLayer
        {
            get => _paintTool.overwritePrefabLayer;
            set => _paintTool.overwritePrefabLayer = value;
        }
        public int layer { get => _paintTool.layer; set => _paintTool.layer = value; }
        public bool autoCreateParent { get => _paintTool.autoCreateParent; set => _paintTool.autoCreateParent = value; }
        public bool createSubparentPerPalette
        {
            get => _paintTool.createSubparentPerPalette;
            set => _paintTool.createSubparentPerPalette = value;
        }
        public bool createSubparentPerTool
        {
            get => _paintTool.createSubparentPerTool;
            set => _paintTool.createSubparentPerTool = value;
        }
        public bool createSubparentPerBrush
        {
            get => _paintTool.createSubparentPerBrush;
            set => _paintTool.createSubparentPerBrush = value;
        }
        public bool createSubparentPerPrefab
        {
            get => _paintTool.createSubparentPerPrefab;
            set => _paintTool.createSubparentPerPrefab = value;
        }
        public bool overwriteBrushProperties
        {
            get => _paintTool.overwriteBrushProperties;
            set => _paintTool.overwriteBrushProperties = value;
        }
        public BrushSettings brushSettings => _paintTool.brushSettings;
        #endregion

        public override void Copy(IToolSettings other)
        {
            var otherTilingSettings = other as TilingSettings;
            base.Copy(other);
            _paintTool.Copy(otherTilingSettings._paintTool);
            _cellSizeType = otherTilingSettings._cellSizeType;
            _cellSize = otherTilingSettings._cellSize;
            _rotation = otherTilingSettings._rotation;
            _spacing = otherTilingSettings._spacing;
            _axisAlignedWithNormal = otherTilingSettings._axisAlignedWithNormal;
        }

        public TilingSettings Clone()
        {
            var clone = new TilingSettings();
            clone.Copy(this);
            return clone;
        }
    }

    public class TilingToolName : IToolName { public string value => "Tiling"; }

    [Serializable]
    public class TilingData : PersistentData<TilingToolName, TilingSettings, ControlPoint>
    {
        [NonSerialized] private List<Vector3> _tilingCenters = new List<Vector3>();
        public List<Vector3> tilingCenters => _tilingCenters;

        public TilingData() : base() { }
        public TilingData(GameObject[] objects, long initialBrushId, TilingData tilingData)
        : base(objects, initialBrushId, tilingData) { }

        private static TilingData _instance = null;
        public static TilingData instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TilingData();
                    _instance._settings = TilingManager.settings;
                }
                return _instance;
            }
        }
        protected override void Initialize()
        {
            base.Initialize();
            const int pointCount = 9;
            for (int i = 0; i < pointCount; i++) _controlPoints.Add(new ControlPoint());
            _pointPositions = new Vector3[pointCount];
        }
        public TilingData Clone()
        {
            var clone = new TilingData();
            base.Clone(clone);
            clone._tilingCenters = _tilingCenters.ToList();
            return clone;
        }
    }

    [Serializable]
    public class TilingSceneData : SceneData<TilingToolName, TilingSettings, ControlPoint, TilingData>
    {
        public TilingSceneData() : base() { }
        public TilingSceneData(string sceneGUID) : base(sceneGUID) { }
    }

    [Serializable]
    public class TilingManager
        : PersistentToolManagerBase<TilingToolName, TilingSettings, ControlPoint, TilingData, TilingSceneData>
    { }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static TilingData _tilingData = TilingData.instance;
        private static TilingData _initialPersistentTilingData = null;
        private static TilingData _selectedPersistentTilingData = null;
        private static bool _editingPersistentTiling = true;

        private static void TilingInitializeOnLoad()
        {
            TilingManager.settings.OnDataChanged += OnTilingSettingsChanged;
            TilingManager.settings.OnRotationChanged += OnTilingRotationChanged;
        }

        public static void ResetTilingState(bool askIfWantToSave = true)
        {
            if (askIfWantToSave)
            {
                void Save()
                {
                    if (SceneView.lastActiveSceneView != null)
                        TilingStrokePreview(SceneView.lastActiveSceneView.camera, TilingData.nextHexId);
                    CreateTiling();
                }
                AskIfWantToSave(_tilingData.state, Save);
            }
            _snappedToVertex = false;
            _tilingData.Reset();
            _paintStroke.Clear();
        }

        private static void OnUndoTiling()
        {
            _paintStroke.Clear();
            BrushstrokeManager.ClearBrushstroke();
            if (ToolManager.editMode)
            {
                _selectedPersistentTilingData.UpdatePoses();
                PreviewPersistentTiling(_selectedPersistentTilingData);
                SceneView.RepaintAll();
            }
        }

        private static void OnTilingToolModeChanged()
        {
            DeselectPersistentItems(TilingManager.instance);
            if (!ToolManager.editMode)
            {
                if (_createProfileName != null)
                    ToolProperties.SetProfile(new ToolProperties.ProfileData(TilingManager.instance, _createProfileName));
                ToolProperties.RepainWindow();
                return;
            }
            ResetTilingState();
            ResetSelectedPersistentObject(TilingManager.instance, ref _editingPersistentTiling, _initialPersistentTilingData);
        }

        private static void OnTilingSettingsChanged()
        {
            if (!ToolManager.editMode) return;
            if (_selectedPersistentTilingData == null) return;
            _selectedPersistentTilingData.settings.Copy(TilingManager.settings);
            PreviewPersistentTiling(_selectedPersistentTilingData);
        }

        private static void OnTilingRotationChanged(float angle, Vector3 axis)
        {
            if (!ToolManager.editMode) return;
            if (_selectedPersistentTilingData == null) return;
            RotateTiling(_selectedPersistentTilingData, angle, axis, false);
            DrawCells(_selectedPersistentTilingData);
            PreviewPersistentTiling(_selectedPersistentTilingData);
            repaint = true;
        }
        private static void TilingDuringSceneGUI(SceneView sceneView)
        {
            if (sceneView.in2DMode && _tilingData.state != ToolManager.ToolState.NONE)
            {
                ResetTilingState();
                TilingManager.settings.rotation = Quaternion.Euler(90, 0, 0);
                ToolProperties.RepainWindow();
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (_tilingData.state == ToolManager.ToolState.EDIT && _tilingData.selectedPointIdx > 0)
                    _tilingData.selectedPointIdx = -1;
                else if (_tilingData.state == ToolManager.ToolState.NONE) ToolManager.DeselectTool();
                else ResetTilingState(false);
            }
            TilingToolEditMode(sceneView);
            if (ToolManager.editMode) return;
            switch (_tilingData.state)
            {
                case ToolManager.ToolState.NONE:
                    TilingStateNone(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.PREVIEW:
                    TilingStateRectangle(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.EDIT:
                    TilingStateEdit(sceneView.camera);
                    break;
            }
        }

        private static void TilingToolEditMode(SceneView sceneView)
        {
            var persistentItems = TilingManager.instance.GetPersistentItems();
            var deselectedItems = new List<TilingData>(persistentItems);
            bool clickOnAnyPoint = false;
            bool selectedItemWasEdited = false;
            foreach (var itemData in persistentItems)
            {
                itemData.UpdateObjects();
                if (itemData.objectCount == 0)
                {
                    TilingManager.instance.RemovePersistentItem(itemData.id);
                    continue;
                }
                DrawCells(itemData);
                if (!ToolManager.editMode) continue;
                DrawTilingRectangle(itemData);

                var selectedTilingId = _initialPersistentTilingData == null ? -1 : _initialPersistentTilingData.id;
                if (DrawTilingControlPoints(itemData, out bool clickOnPoint, out bool wasEdited, out Vector3 delta))
                {
                    if (clickOnPoint)
                    {
                        clickOnAnyPoint = true;
                        _editingPersistentTiling = true;
                        if (selectedTilingId != itemData.id)
                        {
                            ApplySelectedPersistentTiling(false);
                            if (selectedTilingId == -1)
                                _createProfileName = TilingManager.instance.selectedProfileName;
                            TilingManager.instance.CopyToolSettings(itemData.settings);
                            ToolProperties.RepainWindow();
                        }
                        _selectedPersistentTilingData = itemData;
                        if (_initialPersistentTilingData == null) _initialPersistentTilingData = itemData.Clone();
                        else if (_initialPersistentTilingData.id != itemData.id) _initialPersistentTilingData = itemData.Clone();
                        deselectedItems.Remove(itemData);
                    }
                    if (wasEdited)
                    {
                        _editingPersistentTiling = true;
                        selectedItemWasEdited = true;
                        TilingManager.settings.Copy(_selectedPersistentTilingData.settings);
                    }
                }
            }
            if (clickOnAnyPoint)
            {
                foreach (var itemData in deselectedItems)
                {
                    itemData.selectedPointIdx = -1;
                    itemData.ClearSelection();
                }
            }
            if (!ToolManager.editMode) return;
            if (selectedItemWasEdited) PreviewPersistentTiling(_selectedPersistentTilingData);

            if (_editingPersistentTiling && _selectedPersistentTilingData != null)
                TilingStrokePreview(sceneView.camera, _selectedPersistentTilingData.hexId);

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                ApplySelectedPersistentTiling(true);
                ToolProperties.SetProfile(new ToolProperties.ProfileData(TilingManager.instance, _createProfileName));
                ToolProperties.RepainWindow();
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.shift
                && Event.current.keyCode == KeyCode.T && _selectedPersistentTilingData != null)
            {
                var parent = _selectedPersistentTilingData.GetParent();
                if (parent != null) Selection.activeGameObject = parent;
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.alt && Event.current.keyCode == KeyCode.Delete)
            {
                ToolProperties.RegisterUndo("Delete Tiling");
                TilingManager.instance.DeletePersistentItem(_selectedPersistentTilingData.id);
            }
            if (TilingShortcuts(_selectedPersistentTilingData))
            {
                DrawCells(_selectedPersistentTilingData);
                PreviewPersistentTiling(_selectedPersistentTilingData);
                repaint = true;
            }
        }

        private static void PreviewPersistentTiling(TilingData data)
        {
            Vector3[] objPos = null;
            var objList = data.objectList;
            var settings = data.settings;
            BrushstrokeManager.UpdatePersistentTilingBrushstroke(data.tilingCenters.ToArray(),
                settings, objList, out objPos, out Vector3[] strokePos);
            _disabledObjects.Clear();
            _disabledObjects.AddRange(data.objects.ToList());
            for (int objIdx = 0; objIdx < objPos.Length; ++objIdx)
            {
                var obj = objList[objIdx];
                obj.SetActive(true);

                Bounds bounds = BoundsUtils.GetBoundsRecursive(obj.transform);
                var size = bounds.size;
                var height = Mathf.Max(size.x, size.y, size.z) * 2;

                var itemPosition = objPos[objIdx];
                var normal = Vector3.up;
                var ray = new Ray(itemPosition + Vector3.up * height, Vector3.down);
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit,
                        out GameObject collider, height * 2f, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider))
                    {
                        itemPosition = itemHit.point;
                        normal = itemHit.normal;
                    }
                    else if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }
                var itemRotation = settings.rotation;
                Vector3 itemTangent = itemRotation * Vector3.forward;
                BrushSettings brushSettings = PaletteManager.GetBrushById(data.initialBrushId);
                if (settings.overwriteBrushProperties) brushSettings = settings.brushSettings;
                if (brushSettings.rotateToTheSurface
                    && settings.mode != PaintOnSurfaceToolSettings.PaintMode.ON_SHAPE)
                {
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * brushSettings.surfaceDistance;
                }
                else itemPosition += normal * brushSettings.surfaceDistance;
                var axisAlignedWithNormal = (Vector3)settings.axisAlignedWithNormal;
                if (settings.axisAlignedWithNormal.axis != AxesUtils.Axis.Y) axisAlignedWithNormal *= -1;
                itemRotation *= Quaternion.FromToRotation(Vector3.up, axisAlignedWithNormal);
                var previewRotation = itemRotation;
                if (brushSettings.embedInSurface)
                {
                    var TRS = Matrix4x4.TRS(itemPosition, itemRotation, Vector3.one);
                    var bottomVertices = BoundsUtils.GetBottomVertices(obj.transform);
                    var bottomDistanceToSurfce = GetBottomDistanceToSurface(bottomVertices, TRS, height,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider);
                    var bottomMagnitude = BoundsUtils.GetBottomMagnitude(obj.transform);
                    if (!brushSettings.embedAtPivotHeight) bottomDistanceToSurfce -= bottomMagnitude;
                    itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
                }
                itemPosition += itemRotation * (brushSettings.localPositionOffset);

                Undo.RecordObject(obj.transform, LineData.COMMAND_NAME);
                obj.transform.position = itemPosition;
                obj.transform.rotation = itemRotation;
                _disabledObjects.Remove(obj);
            }
            _disabledObjects = _disabledObjects.Where(i => i != null).ToList();
            foreach (var obj in _disabledObjects) obj.SetActive(false);
        }

        private static void ApplySelectedPersistentTiling(bool deselectPoint)
        {
            if (!ApplySelectedPersistentObject(deselectPoint, ref _editingPersistentTiling, ref _initialPersistentTilingData,
                ref _selectedPersistentTilingData, TilingManager.instance)) return;
            if (_initialPersistentTilingData == null) return;
            var selectedTiling = TilingManager.instance.GetItem(_initialPersistentTilingData.id);
            _initialPersistentTilingData = selectedTiling.Clone();
        }
        private static void TilingStateNone(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                _tilingData.state = ToolManager.ToolState.PREVIEW;
                TilingManager.settings.UpdateCellSize();
            }
            if (MouseDot(out Vector3 point, out Vector3 normal, TilingManager.settings.mode, in2DMode,
                TilingManager.settings.paintOnPalettePrefabs, TilingManager.settings.paintOnMeshesWithoutCollider, false))
            {
                point = SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                   TilingManager.settings.paintOnPalettePrefabs, TilingManager.settings.paintOnMeshesWithoutCollider
                   , false);
                _tilingData.SetPoint(2, point, false);
                _tilingData.SetPoint(0, point, false);
            }
            if (_tilingData.pointsCount > 0) DrawDotHandleCap(_tilingData.GetPoint(0));
        }

        private static void DrawTilingRectangle(TilingData data)
        {
            var settings = data.settings;
            var cornerPoints = new Vector3[] { data.GetPoint(0), data.GetPoint(1),
                data.GetPoint(2), data.GetPoint(3), data.GetPoint(0) };
            Handles.color = new Color(0f, 0f, 0f, 0.7f);
            Handles.DrawAAPolyLine(8, cornerPoints);
            Handles.color = new Color(1f, 1f, 1f, 0.7f);
            Handles.DrawAAPolyLine(4, cornerPoints);
        }

        private static void UpdateMidpoints(TilingData data)
        {
            for (int i = 0; i < 4; ++i)
            {
                var nextI = (i + 1) % 4;
                var point = data.GetPoint(i);
                var nextPoint = data.GetPoint(nextI);
                data.SetPoint(i + 4, point + (nextPoint - point) / 2, false);
            }
            data.SetPoint(8, data.GetPoint(0)
                + (data.GetPoint(2) - data.GetPoint(0)) / 2, false);
        }

        private static void DrawCells(TilingData data)
        {
            data.tilingCenters.Clear();
            var settings = data.settings;
            var tangentDir = data.GetPoint(1) - data.GetPoint(0);
            var tangentSize = tangentDir.magnitude;
            tangentDir.Normalize();
            var bitangentDir = data.GetPoint(3) - data.GetPoint(0);
            var bitangentSize = bitangentDir.magnitude;
            bitangentDir.Normalize();
            var cellTangent = tangentDir * Mathf.Abs(settings.cellSize.x);
            var cellBitangent = bitangentDir * Mathf.Abs(settings.cellSize.y);
            var vertices = new Vector3[] { Vector3.zero, cellTangent, cellTangent + cellBitangent, cellBitangent };
            var offset = data.GetPoint(0);
            void DrawCell()
            {
                var linePoints = new Vector3[5];
                for (int i = 0; i <= 4; ++i) linePoints[i] = vertices[i % 4] + offset;
                data.tilingCenters.Add(linePoints[0] + (linePoints[2] - linePoints[0]) / 2);
                Handles.color = new Color(0f, 0f, 0f, 0.3f);
                Handles.DrawAAPolyLine(6, linePoints);
                Handles.color = new Color(1f, 1f, 1f, 0.3f);
                Handles.DrawAAPolyLine(2, linePoints);
            }
            var area = tangentSize * bitangentSize;
            var minCellSize = settings.cellSize + settings.spacing;
            minCellSize = Vector2.Max(minCellSize, Vector2.one * 0.001f);
            var cellArea = minCellSize.x * minCellSize.y;
            var itemCount = area / cellArea;
            if (itemCount > 1024)
            {
                var minCellArea = area / 1024;
                var ratio = minCellSize.x / minCellSize.y;
                minCellSize.x = Mathf.Sqrt(minCellArea / ratio);
                minCellSize.y = minCellArea / minCellSize.x;
            }
            var cellSize = minCellSize - settings.spacing;
            float tangentOffset = 0;
            while (Mathf.Abs(tangentOffset) + Mathf.Abs(cellSize.x) < tangentSize)
            {
                float bitangentOffset = 0;
                while (Mathf.Abs(bitangentOffset) + Mathf.Abs(cellSize.y) < bitangentSize)
                {
                    DrawCell();
                    bitangentOffset += minCellSize.y;
                    offset = data.GetPoint(0) + tangentDir * Mathf.Abs(tangentOffset)
                        + bitangentDir * Mathf.Abs(bitangentOffset);
                }
                tangentOffset += minCellSize.x;
                offset = data.GetPoint(0) + tangentDir * Mathf.Abs(tangentOffset);
            }
        }

        private static void DrawTilingGrid(TilingData data)
        {
            DrawCells(data);
            DrawTilingRectangle(data);
        }

        private static bool DrawTilingControlPoints(TilingData data,
            out bool clickOnPoint, out bool wasEdited, out Vector3 delta)
        {
            delta = Vector3.zero;
            clickOnPoint = false;
            wasEdited = false;

            for (int i = 0; i < 9; ++i)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (!clickOnPoint)
                {
                    float distFromMouse
                        = HandleUtility.DistanceToRectangle(data.GetPoint(i), Quaternion.identity, 0f);
                    HandleUtility.AddControl(controlId, distFromMouse);
                    if (Event.current.button == 0 && Event.current.type == EventType.MouseDown
                        && HandleUtility.nearestControl == controlId)
                    {
                        data.selectedPointIdx = i;
                        clickOnPoint = true;
                        Event.current.Use();
                    }
                }
                if (Event.current.type != EventType.Repaint) continue;
                DrawDotHandleCap(data.GetPoint(i));
            }

            if (data.selectedPointIdx < 0) return false;

            var prevPosition = data.selectedPoint;
            data.SetPoint(data.selectedPointIdx,
                Handles.PositionHandle(data.selectedPoint, data.settings.rotation), true);
            var snappedPoint = SnapAndUpdateGridOrigin(data.selectedPoint, SnapManager.settings.snappingEnabled,
               data.settings.paintOnPalettePrefabs, data.settings.paintOnMeshesWithoutCollider,
               false);
            data.SetPoint(data.selectedPointIdx, snappedPoint, true);

            if (prevPosition != data.selectedPoint)
            {
                wasEdited = true;
                _updateStroke = true;
                delta = data.selectedPoint - prevPosition;
                if (data.selectedPointIdx < 4)
                {
                    var nextCornerIdx = (data.selectedPointIdx + 1) % 4;
                    var oppositeCornerIdx = (data.selectedPointIdx + 2) % 4;
                    var prevCornerIdx = (data.selectedPointIdx + 3) % 4;

                    var nextVector = data.GetPoint(nextCornerIdx) - prevPosition;
                    var prevVector = data.GetPoint(prevCornerIdx) - prevPosition;
                    var deltaNext = Vector3.Project(delta, nextVector);
                    var deltaPrev = Vector3.Project(delta, prevVector);
                    var deltaNormal = delta - deltaNext - deltaPrev;
                    data.AddValue(nextCornerIdx, deltaPrev + deltaNormal);
                    data.AddValue(prevCornerIdx, deltaNext + deltaNormal);
                    data.AddValue(oppositeCornerIdx, deltaNormal);
                }
                else if (data.selectedPointIdx < 8)
                {
                    var prevCornerIdx = data.selectedPointIdx - 4;
                    var nextCornerIdx = (data.selectedPointIdx - 3) % 4;
                    var oppositeSideIdx = (data.selectedPointIdx - 2) % 4 + 4;
                    var parallel = data.GetPoint(nextCornerIdx) - data.GetPoint(prevCornerIdx);
                    var perpendicular = data.GetPoint(oppositeSideIdx) - prevPosition;
                    var deltaParallel = Vector3.Project(delta, parallel);
                    var deltaPerpendicular = Vector3.Project(delta, perpendicular);
                    var deltaNormal = delta - deltaParallel - deltaPerpendicular;
                    for (int i = 0; i < 4; ++i) data.AddValue(i, deltaParallel + deltaNormal);
                    data.AddValue(prevCornerIdx, deltaPerpendicular);
                    data.AddValue(nextCornerIdx, deltaPerpendicular);
                }
                else for (int i = 0; i < 4; ++i) data.AddValue(i, delta);
                UpdateMidpoints(data);
            }
            if (data.selectedPointIdx == 8)
            {
                var prevRotation = data.settings.rotation;
                data.settings.rotation = Handles.RotationHandle(data.settings.rotation, data.GetPoint(8));
                if (data.settings.rotation != prevRotation)
                {
                    var angle = Quaternion.Angle(prevRotation, data.settings.rotation);
                    var axis = Vector3.Cross(prevRotation * Vector3.forward,
                        data.settings.rotation * Vector3.forward);
                    if (axis == Vector3.zero) axis = Vector3.Cross(prevRotation * Vector3.up,
                        data.settings.rotation * Vector3.up);
                    axis.Normalize();
                    RotateTiling(data, angle, axis, false);
                    ToolProperties.RepainWindow();
                    wasEdited = true;
                }
            }
            return clickOnPoint || wasEdited;
        }

        private static void TilingStateRectangle(bool in2DMode)
        {
            var settings = TilingManager.settings;
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                UpdateMidpoints(_tilingData);
                _tilingData.state = ToolManager.ToolState.EDIT;
                _updateStroke = true;
            }

            var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var plane = new Plane(settings.rotation * Vector3.up, _tilingData.GetPoint(0));

            if (plane.Raycast(mouseRay, out float distance))
            {
                var point = mouseRay.GetPoint(distance);
                _tilingData.SetPoint(2, point, false);
                var diagonal = point - _tilingData.GetPoint(0);
                var tangent = Vector3.Project(diagonal, settings.rotation * Vector3.right);
                var bitangent = Vector3.Project(diagonal, settings.rotation * Vector3.forward);
                _tilingData.SetPoint(1, _tilingData.GetPoint(0) + tangent, false);
                _tilingData.SetPoint(3, _tilingData.GetPoint(0) + bitangent, false);
                DrawTilingGrid(_tilingData);
                for (int i = 0; i < 4; ++i) DrawDotHandleCap(_tilingData.GetPoint(i));
                return;
            }
            DrawDotHandleCap(_tilingData.GetPoint(0));
        }

        private static void CreateTiling()
        {
            var nextTilingId = TilingData.nextHexId;
            var objDic = Paint(TilingManager.settings, PAINT_CMD, true, false, nextTilingId);
            if (objDic.Count != 1) 
                return;
            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            var sceneGUID = AssetDatabase.AssetPathToGUID(scenePath);
            var initialBrushId = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.id : -1;
            var objs = objDic[nextTilingId].ToArray();
            var persistentData = new TilingData(objs, initialBrushId, _tilingData);
            TilingManager.instance.AddPersistentItem(sceneGUID, persistentData);
        }

        private static bool TilingShortcuts(TilingData data)
        {
            if (data == null) return false;
            var keyCode = Event.current.keyCode;
            if (Event.current.button == 1
                && Event.current.type == EventType.MouseDrag && Event.current.shift)
            {
                var deltaSign = -Mathf.Sign(Event.current.delta.x + Event.current.delta.y);
                var otherAxes = AxesUtils.GetOtherAxes(AxesUtils.Axis.Y);
                var spacing = Vector3.zero;
                AxesUtils.SetAxisValue(ref spacing, otherAxes[0], data.settings.spacing.x);
                AxesUtils.SetAxisValue(ref spacing, otherAxes[1], data.settings.spacing.y);
                var axisIdx = Event.current.control ? 1 : 0;
                var size = data.GetPoint(2) - data.GetPoint(axisIdx);
                var axisSize = AxesUtils.GetAxisValue(size, otherAxes[axisIdx]);
                AxesUtils.AddValueToAxis(ref spacing, otherAxes[axisIdx], axisSize * deltaSign * 0.005f);
                data.settings.spacing = new Vector2(AxesUtils.GetAxisValue(spacing, otherAxes[0]),
                    AxesUtils.GetAxisValue(spacing, otherAxes[1]));
                ToolProperties.RepainWindow();
                Event.current.Use();
                return true;
            }
            if (Event.current.type != EventType.KeyDown) return false;
            if ((keyCode == KeyCode.J || keyCode == KeyCode.UpArrow)
                && Event.current.control && !Event.current.alt && Event.current.shift)
            {
                RotateTiling(data, 90, Vector3.right, true);
                return true;
            }
            if ((keyCode == KeyCode.U || keyCode == KeyCode.DownArrow)
                && Event.current.control && !Event.current.alt && Event.current.shift)
            {
                RotateTiling(data, 90, Vector3.left, true);
                return true;
            }
            if ((keyCode == KeyCode.L || keyCode == KeyCode.RightArrow)
                && Event.current.control && Event.current.alt && !Event.current.shift)
            {
                RotateTiling(data, 90, Vector3.up, true);
                return true;
            }
            if ((keyCode == KeyCode.K || keyCode == KeyCode.LeftArrow)
               && Event.current.control && Event.current.alt && !Event.current.shift)
            {
                RotateTiling(data, 90, Vector3.down, true);
                return true;
            }
            if ((keyCode == KeyCode.U || keyCode == KeyCode.UpArrow)
              && Event.current.control && Event.current.alt && !Event.current.shift)
            {
                RotateTiling(data, 90, Vector3.forward, true);
                return true;
            }
            if ((keyCode == KeyCode.J || keyCode == KeyCode.DownArrow)
              && Event.current.control && Event.current.alt && !Event.current.shift)
            {
                RotateTiling(data, 90, Vector3.back, true);
                return true;
            }
            return false;
        }

        private static void TilingStateEdit(Camera camera)
        {
            bool mouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
            TilingShortcuts(_tilingData);

            if (_updateStroke)
            {
                BrushstrokeManager.UpdateTilingBrushstroke(_tilingData.tilingCenters.ToArray());
                _updateStroke = false;
            }
            TilingStrokePreview(camera, TilingData.nextHexId);

            DrawTilingGrid(_tilingData);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                CreateTiling();
                ResetTilingState(false);
            }
            DrawTilingControlPoints(_tilingData, out bool clickOnPoint, out bool wasEdited, out Vector3 delta);
        }

        private static void RotateTiling(TilingData data, float angle, Vector3 axis, bool updateDataRotation)
        {
            _updateStroke = true;
            var delta = Quaternion.AngleAxis(angle, axis);
            for (int i = 0; i < 8; ++i)
            {
                var centerToPoint = data.GetPoint(i) - data.GetPoint(8);
                var rotatedPos = (delta * centerToPoint) + data.GetPoint(8);
                data.SetPoint(i, rotatedPos, false);
            }
            if (updateDataRotation) data.settings.rotation *= delta;
            DrawCells(data);
        }

        public static void UpdateTilingRotation(Quaternion delta)
        {
            _updateStroke = true;
            for (int i = 0; i < 8; ++i)
            {
                var centerToPoint = _tilingData.GetPoint(i) - _tilingData.GetPoint(8);
                var rotatedPos = (delta * centerToPoint) + _tilingData.GetPoint(8);
                _tilingData.SetPoint(i, rotatedPos, false);
            }
        }

        private static void TilingStrokePreview(Camera camera, string hexId)
        {
            BrushstrokeItem[] brushstroke;
            if (PreviewIfBrushtrokestaysTheSame(out brushstroke, camera)) return;
            _paintStroke.Clear();
            var settings = TilingManager.settings;

            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];

                var prefab = strokeItem.settings.prefab;
                if (prefab == null) continue;
                Bounds bounds = BoundsUtils.GetBoundsRecursive(prefab.transform);
                var size = bounds.size;
                var height = Mathf.Max(size.x, size.y, size.z) * 2;

                var itemPosition = strokeItem.tangentPosition;
                var normal = Vector3.up;
                var ray = new Ray(itemPosition + Vector3.up * height, Vector3.down);
                if (settings.mode != TilingSettings.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit,
                        out GameObject collider, height * 2f, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider))
                    {
                        itemPosition = itemHit.point;
                        normal = itemHit.normal;
                    }
                    else if (settings.mode == TilingSettings.PaintMode.ON_SURFACE) continue;
                }
                var itemRotation = settings.rotation;
                Vector3 itemTangent = itemRotation * Vector3.forward;
                BrushSettings brushSettings = strokeItem.settings;
                if (settings.overwriteBrushProperties) brushSettings = settings.brushSettings;
                if (brushSettings.rotateToTheSurface
                    && settings.mode != PaintOnSurfaceToolSettings.PaintMode.ON_SHAPE)
                {
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * brushSettings.surfaceDistance;
                }
                else itemPosition += normal * brushSettings.surfaceDistance;
                var axisAlignedWithNormal = (Vector3)settings.axisAlignedWithNormal;
                if (settings.axisAlignedWithNormal.axis != AxesUtils.Axis.Y) axisAlignedWithNormal *= -1;
                itemRotation *= Quaternion.FromToRotation(Vector3.up, axisAlignedWithNormal);

                itemRotation *= Quaternion.Euler(strokeItem.additionalAngle);
                var previewRotation = itemRotation;
                if (brushSettings.embedInSurface)
                {
                    var TRS = Matrix4x4.TRS(itemPosition, itemRotation, strokeItem.scaleMultiplier);
                    var bottomDistanceToSurfce = GetBottomDistanceToSurface(strokeItem.settings.bottomVertices,
                        TRS, strokeItem.settings.height * strokeItem.scaleMultiplier.y, settings.paintOnPalettePrefabs,
                        settings.paintOnMeshesWithoutCollider);
                    if (!brushSettings.embedAtPivotHeight)
                        bottomDistanceToSurfce -= strokeItem.settings.bottomMagnitude;
                    itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
                }
                itemPosition += itemRotation * (brushSettings.localPositionOffset);

                var centerToPivot = strokeItem.settings.prefab.transform.position - bounds.center;
                var centerToPivotRotated = Quaternion.FromToRotation(Vector3.up, axisAlignedWithNormal) * centerToPivot;
                centerToPivotRotated.y = 0;
                itemPosition += settings.rotation * centerToPivotRotated;

                var itemScale = Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier);

                var layer = settings.overwritePrefabLayer ? settings.layer : prefab.layer;
                Transform parentTransform = settings.parent;

                var paintItem = new PaintStrokeItem(prefab, itemPosition,
                    itemRotation * Quaternion.Euler(prefab.transform.eulerAngles),
                    itemScale, layer, parentTransform);
                paintItem.persistentParentId = hexId;

                _paintStroke.Add(paintItem);
                var previewRootToWorld = Matrix4x4.TRS(itemPosition, previewRotation, strokeItem.scaleMultiplier)
                    * Matrix4x4.Translate(-prefab.transform.position);
                PreviewBrushItem(prefab, previewRootToWorld, layer, camera);
                _previewData.Add(new PreviewData(prefab, previewRootToWorld, layer));
            }
        }
    }
    #endregion
}
