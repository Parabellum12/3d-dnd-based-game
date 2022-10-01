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
using UnityEngine.Rendering;

namespace PluginMaster
{
    #region DATA & SETTINGS
    [Serializable]
    public class ShapeData : PersistentData<ShapeToolName, ShapeSettings, ControlPoint>
    {
        [SerializeField] private float _radius = 0f;
        [SerializeField] private float _arcAngle = 360f;
        [SerializeField] private Vector3 _normal = Vector3.up;
        [SerializeField] private Plane _plane;
        [SerializeField] private int _firstVertexIdxAfterIntersection = 2;
        [SerializeField] private int _lastVertexIdxBeforeIntersection = 1;
        [SerializeField] private Vector3[] _arcIntersections = new Vector3[2];
        private int _circleSideCount = 8;

        public Vector3 normal
        {
            get => _normal;
            set
            {
                if (_normal == value) return;
                _normal = value;
            }
        }
        public int circleSideCount => _circleSideCount;
        public void SetCenter(Vector3 value, Vector3 normal)
        {
            if (pointsCount == 0)
            {
                AddPoint(value);
                AddPoint(value);
            }
            else if (points[0] != value)
            {
                SetPoint(0, value, false);
                SetPoint(1, value, false);
            }
            _normal = normal;
            _plane = new Plane(_normal, points[0]);
            if (_settings.projectInNormalDir) _settings.UpdateProjectDirection(-_normal);
        }

        public void SetRadius(Vector3 point)
        {
            SetPoint(1, point, false);
            _radius = (points[1] - points[0]).magnitude;
            if (_settings.shapeType == ShapeSettings.ShapeType.CIRCLE) UpdateCircleSideCount();
        }
        public float radius => _radius;
        public Vector3 radiusPoint => points[1];
        public Plane plane => _plane;
        public Vector3 center => points[0];
        public float arcAngle => _arcAngle;

        public Vector3 GetArcIntersection(int idx) => _arcIntersections[idx];
        public int firstVertexIdxAfterIntersection => _firstVertexIdxAfterIntersection;
        public int lastVertexIdxBeforeIntersection => _lastVertexIdxBeforeIntersection;

        public void SetHandlePoints(Vector3[] vertices)
        {
            if (pointsCount > 2) PointsRemoveRange(2, pointsCount - 2);
            var midPoints = new List<Vector3>();
            for (int i = 1; i < vertices.Length; ++i)
            {
                AddPoint(vertices[i]);
                if (_settings.shapeType == ShapeSettings.ShapeType.POLYGON)
                    midPoints.Add((vertices[i] - vertices[i - 1]) / 2 + vertices[i - 1]);
            }
            if (_settings.shapeType == ShapeSettings.ShapeType.POLYGON)
            {
                midPoints.Add((vertices[vertices.Length - 1] - vertices[0]) / 2 + vertices[0]);
                AddPointRange(ControlPoint.VectorArrayToPointArray(midPoints.ToArray()));
            }
            var arcPoint = points[1] + (points[1] - points[0]);
            AddPoint(arcPoint);
            AddPoint(arcPoint);
            _arcIntersections[0] = points[1];
            _arcIntersections[1] = points[1];
        }
        public Vector3[] vertices => ControlPoint.PointArrayToVectorArray(PointsGetRange(1,
            _settings.shapeType == ShapeSettings.ShapeType.POLYGON ? _settings.sidesCount : _circleSideCount));

        public Quaternion planeRotation
        {
            get
            {
                var forward = Vector3.Cross(_normal, Vector3.right);
                if (forward.sqrMagnitude < 0.000001) forward = Vector3.Cross(_normal, Vector3.down);
                return Quaternion.LookRotation(forward, _normal);
            }
        }

        private static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1,
            Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
            float planarFactor = Mathf.Abs(90 - Vector3.Angle(lineVec3, crossVec1and2));
            if (planarFactor < 0.01f && crossVec1and2.sqrMagnitude > 0.001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineVec1 * s);
                var min = Vector3.Max(Vector3.Min(linePoint1, linePoint1 + lineVec1),
                    Vector3.Min(linePoint2, linePoint2 + lineVec2));
                var max = Vector3.Min(Vector3.Max(linePoint1, linePoint1 + lineVec1),
                    Vector3.Max(linePoint2, linePoint2 + lineVec2));
                var tolerance = Vector3.one * 0.001f;
                var minComp = intersection + tolerance - min;
                var maxComp = max + tolerance - intersection;
                var result = minComp.x >= 0 && minComp.y >= 0 && minComp.z >= 0
                    && maxComp.x >= 0 && maxComp.y >= 0 && maxComp.z >= 0;
                return result;
            }
            else
            {
                intersection = Vector3.zero;
                return false;
            }
        }

        public void UpdateIntersections()
        {
            if (state < ToolManager.ToolState.EDIT) return;
            var centerToArc1 = GetPoint(-1) - center;
            var centerToArc2 = GetPoint(-2) - center;

            bool firstPointFound = false;
            bool lastPointFound = false;
            var sidesCount = _settings.shapeType == ShapeSettings.ShapeType.POLYGON
                ? _settings.sidesCount : _circleSideCount;
            int GetNextVertexIdx(int currentIdx) => currentIdx == sidesCount ? 1 : currentIdx + 1;
            for (int i = 1; i <= sidesCount; ++i)
            {
                var startPoint = GetPoint(i);
                var endIdx = GetNextVertexIdx(i);
                var endPoint = GetPoint(endIdx);
                var startToEnd = endPoint - startPoint;
                if (!firstPointFound)
                {
                    if (LineLineIntersection(out Vector3 intersection, center, centerToArc1,
                        startPoint, startToEnd))
                    {
                        firstPointFound = true;
                        _firstVertexIdxAfterIntersection = endIdx;
                        _arcIntersections[0] = intersection;
                    }
                }
                if (!lastPointFound)
                {
                    if (LineLineIntersection(out Vector3 intersection, center, centerToArc2,
                        startPoint, startToEnd))
                    {
                        lastPointFound = true;
                        _lastVertexIdxBeforeIntersection = i;
                        _arcIntersections[1] = intersection;
                    }
                }
                if (firstPointFound && lastPointFound) break;
            }
        }

        public Quaternion rotation
        {
            get
            {
                var radiusVector = radiusPoint - center;
                return Quaternion.LookRotation(radiusVector, _normal);
            }
            set
            {
                var prevRadiusVector = radiusPoint - center;
                var prev = Quaternion.LookRotation(prevRadiusVector, normal);
                _plane.normal = _normal = value * Vector3.up;
                var delta = value * Quaternion.Inverse(prev);
                for (int i = 0; i < pointsCount - 2; ++i) SetPoint(i, delta * (points[i] - center) + center, false);
                SetPoint(pointsCount - 1, delta * (points[pointsCount - 1] - center).normalized
                    * _radius * 2f + center, false);
                SetPoint(pointsCount - 2, delta * (points[pointsCount - 2] - center).normalized
                    * _radius * 2f + center, false);
                UpdateIntersections();
                if (_settings.projectInNormalDir) _settings.UpdateProjectDirection(-_normal);
            }
        }

        public void MovePoint(int idx, Vector3 position)
        {
            if (position == points[idx]) return;
            var delta = position - points[idx];
            if (idx == 0)
            {
                for (int i = 0; i < pointsCount; ++i) SetPoint(i, points[i] + delta, true);
                _arcIntersections[0] += delta;
                _arcIntersections[1] += delta;
            }
            else
            {
                var normalDelta = Vector3.Project(delta, _normal);
                var centerToPoint = points[idx] - center;
                var radiusDelta = Vector3.Project(delta, centerToPoint);
                var newRadius = position - center - normalDelta;
                var angle = Vector3.SignedAngle(centerToPoint, newRadius, _normal);
                var rotation = Quaternion.AngleAxis(angle, _normal);
                if ((_settings.shapeType == ShapeSettings.ShapeType.CIRCLE && idx == 1)
                  || (_settings.shapeType == ShapeSettings.ShapeType.POLYGON
                  && idx <= _settings.sidesCount * 2))
                {
                    _radius = newRadius.magnitude;
                    var radiusScale = _radius < 0.1f ? 1f : 1f + radiusDelta.magnitude / _radius
                        * (Vector3.Dot(centerToPoint, radiusDelta) >= 0 ? 1f : -1f);
                    for (int i = 0; i < pointsCount - 2; ++i)
                        SetPoint(i, rotation * (points[i] - center) * radiusScale + normalDelta + center, false);
                    SetPoint(pointsCount - 1, rotation * (points[pointsCount - 1] - center).normalized
                        * _radius * 2f + center + normalDelta, false);
                    SetPoint(pointsCount - 2, rotation * (points[pointsCount - 2] - center).normalized
                        * _radius * 2f + center + normalDelta, true);
                    if (_settings.shapeType == ShapeSettings.ShapeType.CIRCLE)
                        if (UpdateCircleSideCount()) Update(false);
                }
                else
                {
                    SetPoint(idx, rotation * (points[idx] - center) + center, true);
                    if (normalDelta != Vector3.zero)
                    {
                        for (int i = 0; i < pointsCount; ++i) SetPoint(i, points[i] + normalDelta, true);
                    }
                    _arcAngle = Vector3.SignedAngle(GetPoint(-1) - center, GetPoint(-2) - center, normal);
                    if (_arcAngle <= 0) _arcAngle += 360;
                }
                UpdateIntersections();
            }
        }

        public bool UpdateCircleSideCount()
        {
            var perimenter = 2 * Mathf.PI * _radius;
            var maxItemSize = 1f;
            if (PaletteManager.selectedBrush != null)
            {
                maxItemSize = float.MinValue;
                for (int i = 0; i < PaletteManager.selectedBrush.itemCount; ++i)
                    maxItemSize = Mathf.Max(BrushstrokeManager.GetLineSpacing(i, _settings), maxItemSize);
            }
            var prevCount = _circleSideCount;
            _circleSideCount = Mathf.FloorToInt(perimenter / maxItemSize);
            var sideLenght = 2 * _radius * Mathf.Sin(Mathf.PI / _circleSideCount);
            if (sideLenght <= maxItemSize) --_circleSideCount;
            _circleSideCount = Mathf.Max(_circleSideCount, 8);
            return prevCount != _circleSideCount;
        }

        protected override void Initialize()
        {
            base.Initialize();
            _arcIntersections[0] = _arcIntersections[1] = Vector3.zero;
            _radius = 0f;
            _arcAngle = 360f;
            _normal = Vector3.up;
            _plane = new Plane();
            _firstVertexIdxAfterIntersection = 2;
            _lastVertexIdxBeforeIntersection = 1;
            _circleSideCount = 8;
        }

        public void Update(bool clearSelection)
        {
            if (pointsCount < 2) return;
            ToolProperties.RegisterUndo(COMMAND_NAME);
            if (clearSelection) selectedPointIdx = -1;
            var arcPoints = PointsGetRange(pointsCount - 2, 2);
            var center = points[0];
            var polygonVertices = PWBIO.GetPolygonVertices(this);
            _controlPoints.Clear();
            _controlPoints.Add(center);
            _controlPoints.AddRange(ControlPoint.VectorArrayToPointArray(polygonVertices));
            if (_settings.shapeType == ShapeSettings.ShapeType.POLYGON)
            {
                for (int i = 1; i < polygonVertices.Length; ++i)
                    _controlPoints.Add((polygonVertices[i] - polygonVertices[i - 1]) / 2 + polygonVertices[i - 1]);
                _controlPoints.Add((polygonVertices[polygonVertices.Length - 1]
                    - polygonVertices[0]) / 2 + polygonVertices[0]);
            }
            _controlPoints.AddRange(arcPoints);
            UpdatePoints();
        }

        public ShapeData() : base() { }

        public ShapeData(GameObject[] objects, long initialBrushId, ShapeData shapeData)
            : base(objects, initialBrushId, shapeData) { }

        private static ShapeData _instance = null;
        public static ShapeData instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ShapeData();
                    _instance._settings = ShapeManager.settings;
                }
                return _instance;
            }
        }

        private void CopyShapeData(ShapeData other)
        {
            _radius = other._radius;
            _arcAngle = other._arcAngle;
            _normal = other._normal;
            _plane = other._plane;
            _firstVertexIdxAfterIntersection = other._firstVertexIdxAfterIntersection;
            _lastVertexIdxBeforeIntersection = other._lastVertexIdxBeforeIntersection;
            _arcIntersections = other._arcIntersections.ToArray();
            _circleSideCount = other._circleSideCount;
        }

        public override void Copy(PersistentData<ShapeToolName, ShapeSettings, ControlPoint> other)
        {
            base.Copy(other);
            var otherShapeData = other as ShapeData;
            if (otherShapeData == null) return;
            CopyShapeData(otherShapeData);
        }

        public ShapeData Clone()
        {
            var clone = new ShapeData();
            base.Clone(clone);
            clone.CopyShapeData(this);
            return clone;
        }
    }

    [Serializable]
    public class ShapeSettings : LineSettings
    {
        public enum ShapeType { CIRCLE, POLYGON }
        [SerializeField] private ShapeType _shapeType = ShapeType.POLYGON;
        [SerializeField] private int _sidesCount = 5;
        [SerializeField] private bool _axisNormalToSurface = true;
        [SerializeField] private Vector3 _normal = Vector3.up;
        [SerializeField] private bool _projectInNormalDir = true;

        public ShapeType shapeType
        {
            get => _shapeType;
            set
            {
                if (_shapeType == value) return;
                _shapeType = value;
                OnDataChanged();
            }
        }
        public int sidesCount
        {
            get => _sidesCount;
            set
            {
                value = Mathf.Max(value, 3);
                if (_sidesCount == value) return;
                _sidesCount = value;
                OnDataChanged();
            }
        }
        public bool axisNormalToSurface
        {
            get => _axisNormalToSurface;
            set
            {
                if (_axisNormalToSurface == value) return;
                _axisNormalToSurface = value;
                OnDataChanged();
            }
        }
        public Vector3 normal
        {
            get => _normal;
            set
            {
                if (_normal == value) return;
                _normal = value;
                OnDataChanged();
            }
        }
        public bool projectInNormalDir
        {
            get => _projectInNormalDir;
            set
            {
                if (_projectInNormalDir == value) return;
                _projectInNormalDir = value;
                OnDataChanged();
            }
        }
        public override void Copy(IToolSettings other)
        {
            base.Copy(other);
            var otherShapeSettings = other as ShapeSettings;
            if (otherShapeSettings == null) return;
            _shapeType = otherShapeSettings._shapeType;
            _sidesCount = otherShapeSettings._sidesCount;
            _axisNormalToSurface = otherShapeSettings._axisNormalToSurface;
            _normal = otherShapeSettings._normal;
            _projectInNormalDir = otherShapeSettings._projectInNormalDir;
        }

        public override void Clone(ICloneableToolSettings clone)
        {
            if (clone == null || !(clone is ShapeSettings)) clone = new ShapeSettings();
            clone.Copy(this);
        }

        public override void DataChanged()
        {
            base.DataChanged();
            if (!ToolManager.editMode) ShapeData.instance.Update(true);
            else PWBIO.OnShapeSettingsChanged();
        }
    }

    public class ShapeToolName : IToolName { public string value => "Shape"; }

    [Serializable]
    public class ShapeSceneData : SceneData<ShapeToolName, ShapeSettings, ControlPoint, ShapeData>
    {
        public ShapeSceneData() : base() { }
        public ShapeSceneData(string sceneGUID) : base(sceneGUID) { }
    }

    [Serializable]
    public class ShapeManager
        : PersistentToolManagerBase<ShapeToolName, ShapeSettings, ControlPoint, ShapeData, ShapeSceneData>
    { }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static ShapeData _shapeData = ShapeData.instance;
        private static bool _editingPersistentShape = true;
        private static ShapeData _initialPersistentShapeData = null;
        private static ShapeData _selectedPersistentShapeData = null;

        private static void ShapeInitializeOnLoad()
        {
            ShapeManager.settings.OnDataChanged += OnShapeSettingsChanged;
        }

        private static void DeselectPersistentShapes()
        {
            var persistentShapes = ShapeManager.instance.GetPersistentItems();
            foreach (var s in persistentShapes)
            {
                s.selectedPointIdx = -1;
                s.ClearSelection();
            }
        }

        private static void ResetSelectedPersistentShape()
        {
            _editingPersistentShape = false;
            if (_initialPersistentShapeData == null) return;
            var selectedShape = ShapeManager.instance.GetItem(_initialPersistentShapeData.id);
            if (selectedShape == null) return;
            selectedShape.ResetPoses(_initialPersistentShapeData);
            selectedShape.selectedPointIdx = -1;
            selectedShape.ClearSelection();
        }

        private static void OnShapeToolModeChanged()
        {
            DeselectPersistentShapes();
            if (!ToolManager.editMode)
            {
                if (_createProfileName != null)
                    ToolProperties.SetProfile(new ToolProperties.ProfileData(ShapeManager.instance, _createProfileName));
                ToolProperties.RepainWindow();
                return;
            }
            ResetShapeState();
            ResetSelectedPersistentShape();
        }

        public static void OnShapeSettingsChanged()
        {
            if (!ToolManager.editMode) return;
            if (_selectedPersistentShapeData == null) return;
            _selectedPersistentShapeData.settings.Copy(ShapeManager.settings);
            _selectedPersistentShapeData.Update(false);
            PreviewPersistenShape(_selectedPersistentShapeData);
        }

        public static void ResetShapeState(bool askIfWantToSave = true)
        {
            if (askIfWantToSave)
            {
                void Save()
                {
                    if (SceneView.lastActiveSceneView != null)
                        ShapeStrokePreview(SceneView.lastActiveSceneView.camera, ShapeData.nextHexId);
                    CreateShape();
                }
                AskIfWantToSave(_shapeData.state, Save);
            }
            _snappedToVertex = false;
            _shapeData.Reset();
        }

        private static void OnUndoShape()
        {
            _paintStroke.Clear();
            BrushstrokeManager.ClearBrushstroke();
            if (ToolManager.editMode)
            {
                PreviewPersistenShape(_selectedPersistentShapeData);
                SceneView.RepaintAll();
            }
        }

        private static void ShapeDuringSceneGUI(SceneView sceneView)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (_shapeData.state == ToolManager.ToolState.EDIT && _shapeData.selectedPointIdx > 0)
                    _shapeData.selectedPointIdx = -1;
                else if (_shapeData.state == ToolManager.ToolState.NONE) ToolManager.DeselectTool();
                else ResetShapeState(false);
                OnUndoShape();
                UpdateStroke();
                BrushstrokeManager.ClearBrushstroke();
            }
            ShapeToolEditMode(sceneView);
            if (ToolManager.editMode) return;
            switch (_shapeData.state)
            {
                case ToolManager.ToolState.NONE:
                    ShapeStateNone(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.PREVIEW:
                    ShapeStateRadius(sceneView.in2DMode, _shapeData);
                    break;
                case ToolManager.ToolState.EDIT:
                    ShapeStateEdit(sceneView.camera);
                    break;
            }
        }

        private static void ShapeToolEditMode(SceneView sceneView)
        {
            var persistentItems = ShapeManager.instance.GetPersistentItems();
            var selectedItemId = _initialPersistentShapeData == null ? -1 : _initialPersistentShapeData.id;
            foreach (var shapeData in persistentItems)
            {
                shapeData.UpdateObjects();
                if (shapeData.objectCount == 0)
                {
                    ShapeManager.instance.RemovePersistentItem(shapeData.id);
                    continue;
                }
                DrawShapeLines(shapeData);
                if (ShapeControlPoints(shapeData, out bool clickOnPoint, out bool wasEditted,
                     ToolManager.editMode, out Vector3 delta))
                {
                    if (clickOnPoint)
                    {
                        _editingPersistentShape = true;
                        if (selectedItemId != shapeData.id)
                        {
                            ApplySelectedPersistentShape(false);
                            if (selectedItemId == -1)
                                _createProfileName = ShapeManager.instance.selectedProfileName;
                            ShapeManager.instance.CopyToolSettings(shapeData.settings);
                            ToolProperties.RepainWindow();
                        }
                        _selectedPersistentShapeData = shapeData;
                        if (_initialPersistentShapeData == null) _initialPersistentShapeData = shapeData.Clone();
                        else if (_initialPersistentShapeData.id != shapeData.id)
                            _initialPersistentShapeData = shapeData.Clone();

                        foreach (var i in persistentItems)
                        {
                            if (i == shapeData) continue;
                            i.selectedPointIdx = -1;
                        }
                    }
                    if (wasEditted)
                    {
                        _editingPersistentShape = true;
                        PreviewPersistenShape(shapeData);
                        PWBCore.SetSavePending();
                    }
                }
            }

            if (!ToolManager.editMode) return;

            if (_editingPersistentShape && _selectedPersistentShapeData != null)
            {
                if (_updateStroke)
                {
                    PreviewPersistenShape(_selectedPersistentShapeData);
                    _updateStroke = false;
                    PWBCore.SetSavePending();
                }
                ShapeStrokePreview(sceneView.camera, _selectedPersistentShapeData.hexId);
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                DeleteDisabledObjects();
                ApplySelectedPersistentShape(true);
                ToolProperties.SetProfile(new ToolProperties.ProfileData(ShapeManager.instance, _createProfileName));
                DeleteDisabledObjects();
                ToolProperties.RepainWindow();
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.shift
                && Event.current.keyCode == KeyCode.T && _selectedPersistentShapeData != null)
            {
                var parent = _selectedPersistentShapeData.GetParent();
                if (parent != null) Selection.activeGameObject = parent;
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.alt && Event.current.keyCode == KeyCode.Delete)
            {
                ToolProperties.RegisterUndo("Delete Shape");
                ShapeManager.instance.DeletePersistentItem(_selectedPersistentShapeData.id);
            }
        }

        private static void PreviewPersistenShape(ShapeData shapeData)
        {
            Pose[] objPoses = null;
            var objList = shapeData.objectList;
            BrushstrokeManager.UpdatePersistentShapeBrushstroke(shapeData, objList, out objPoses);
            _disabledObjects = objList.ToList();
            var settings = shapeData.settings;
            for (int objIdx = 0; objIdx < objPoses.Length; ++objIdx)
            {
                var obj = objList[objIdx];
                obj.SetActive(true);
                var bounds = BoundsUtils.GetBoundsRecursive(obj.transform, obj.transform.rotation);
                var size = bounds.size;
                var height = Mathf.Max(size.x, size.y, size.z) * 2;
                Vector3 segmentDir = Vector3.zero;
                var normal = -shapeData.settings.projectionDirection;

                var itemRotation = Quaternion.identity;
                var itemPosition = objPoses[objIdx].position;

                var ray = new Ray(itemPosition + normal * height, -normal);
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit, out GameObject collider,
                        height * 2f, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider))
                    {
                        itemPosition = itemHit.point;
                        normal = itemHit.normal;
                    }
                    else if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }
                BrushSettings brushSettings = PaletteManager.GetBrushById(shapeData.initialBrushId);
                if (shapeData.settings.overwriteBrushProperties) brushSettings = shapeData.settings.brushSettings;
                else if (PaletteManager.selectedBrush != null) brushSettings = PaletteManager.selectedBrush;

                if (brushSettings.rotateToTheSurface)
                {
                    var itemTangent = GetTangent(normal);
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * brushSettings.surfaceDistance;
                }
                else itemPosition += normal * brushSettings.surfaceDistance;
                itemRotation *= objPoses[objIdx].rotation;
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    var itemForward = itemRotation * Vector3.forward;
                    if (settings.parallelToTheSurface)
                    {
                        var plane = new Plane(normal, itemPosition);
                        itemForward = plane.ClosestPointOnPlane(itemForward + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(itemForward, normal);
                    }
                }
                if (brushSettings.embedInSurface && settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    var itemForward = itemRotation * Vector3.forward;
                    if (settings.parallelToTheSurface)
                    {
                        var plane = new Plane(normal, itemPosition);
                        itemForward = plane.ClosestPointOnPlane(itemForward + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(itemForward, normal);
                    }
                    var TRS = Matrix4x4.TRS(itemPosition, itemRotation, Vector3.one);
                    var bottomVertices = BoundsUtils.GetBottomVertices(obj.transform);
                    var bottomDistanceToSurfce = GetBottomDistanceToSurface(bottomVertices, TRS, height,
                        shapeData.settings.paintOnPalettePrefabs, shapeData.settings.paintOnMeshesWithoutCollider);
                    var bottomMagnitude = BoundsUtils.GetBottomMagnitude(obj.transform);
                    if (!brushSettings.embedAtPivotHeight) bottomDistanceToSurfce -= bottomMagnitude;
                    itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
                }
                itemPosition += itemRotation * brushSettings.localPositionOffset;
                Undo.RecordObject(obj.transform, ShapeData.COMMAND_NAME);
                obj.transform.position = itemPosition;
                obj.transform.rotation = itemRotation;
                _disabledObjects.Remove(obj);
            }
            foreach (var obj in _disabledObjects) obj.SetActive(false);
        }

        private static Vector3 ClosestPointOnPlane(Vector3 point, ShapeData shapeData)
        {
            var plane = new Plane(shapeData.planeRotation * Vector3.up, shapeData.center);
            return plane.ClosestPointOnPlane(point);
        }

        private static void ApplySelectedPersistentShape(bool deselectPoint)
        {
            if (!ApplySelectedPersistentObject(deselectPoint, ref _editingPersistentShape, ref _initialPersistentShapeData,
               ref _selectedPersistentShapeData, ShapeManager.instance)) return;
            if (_initialPersistentShapeData == null) return;
            var selected = ShapeManager.instance.GetItem(_initialPersistentShapeData.id);
            _initialPersistentShapeData = selected.Clone();
        }
        private static void ShapeStateNone(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
                _shapeData.state = ToolManager.ToolState.PREVIEW;
            if (MouseDot(out Vector3 point, out Vector3 normal, ShapeManager.settings.mode, in2DMode,
                ShapeManager.settings.paintOnPalettePrefabs, ShapeManager.settings.paintOnMeshesWithoutCollider, false))
            {
                point = SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                   ShapeManager.settings.paintOnPalettePrefabs, ShapeManager.settings.paintOnMeshesWithoutCollider, false);
                _shapeData.SetCenter(point,
                    ShapeManager.settings.axisNormalToSurface ? normal : ShapeManager.settings.normal);
            }
            if (_shapeData.pointsCount > 0) DrawDotHandleCap(_shapeData.GetPoint(0));
        }
        private static void ShapeStateRadius(bool in2DMode, ShapeData shapeData)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                shapeData.SetHandlePoints(GetPolygonVertices());
                shapeData.state = ToolManager.ToolState.EDIT;
                _updateStroke = true;
                return;
            }
            var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (_snapToVertex)
            {
                if (SnapToVertex(mouseRay, out RaycastHit closestVertexInfo, in2DMode))
                    mouseRay.origin = closestVertexInfo.point - mouseRay.direction;
            }

            var radiusPoint = shapeData.center;
            if (shapeData.plane.Raycast(mouseRay, out float distance))
                radiusPoint = mouseRay.GetPoint(distance);
            radiusPoint = SnapAndUpdateGridOrigin(radiusPoint, SnapManager.settings.snappingEnabled,
                   shapeData.settings.paintOnPalettePrefabs, shapeData.settings.paintOnMeshesWithoutCollider, false);
            radiusPoint = ClosestPointOnPlane(radiusPoint, shapeData);
            shapeData.SetRadius(radiusPoint);
            DrawShapeLines(shapeData);
            DrawDotHandleCap(shapeData.center);
            DrawDotHandleCap(shapeData.radiusPoint);
        }


        private static bool ShapeControlPoints(ShapeData shapeData, out bool clickOnPoint,
            out bool wasEdited, bool showHandles, out Vector3 delta)
        {
            delta = Vector3.zero;
            clickOnPoint = false;
            wasEdited = false;
            var isCircle = shapeData.settings.shapeType == ShapeSettings.ShapeType.CIRCLE;
            var isPolygon = shapeData.settings.shapeType == ShapeSettings.ShapeType.POLYGON;
            bool leftMouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;

            DrawDotHandleCap(shapeData.center);
            if (isPolygon) foreach (var vertex in shapeData.vertices) DrawDotHandleCap(vertex);
            else DrawDotHandleCap(shapeData.radiusPoint);
            if (shapeData.selectedPointIdx >= 0) DrawDotHandleCap(shapeData.selectedPoint, 1f, 1.2f);
            DrawDotHandleCap(shapeData.GetPoint(-1));
            DrawDotHandleCap(shapeData.GetPoint(-2));

            for (int i = 0; i < shapeData.pointsCount; ++i)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (!clickOnPoint)
                {
                    if (showHandles)
                    {
                        float distFromMouse = HandleUtility.DistanceToRectangle(shapeData.GetPoint(i),
                       shapeData.planeRotation, 0f);
                        HandleUtility.AddControl(controlId, distFromMouse);
                        if (HandleUtility.nearestControl != controlId) continue;
                        if (isPolygon) DrawDotHandleCap(shapeData.GetPoint(i));
                        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
                        {
                            shapeData.selectedPointIdx = i;
                            clickOnPoint = true;
                            Event.current.Use();
                        }
                    }
                }
            }
            if (showHandles && shapeData.selectedPointIdx >= 0)
            {
                var prevPosition = shapeData.selectedPoint;
                var snappedPoint = Handles.PositionHandle(prevPosition, shapeData.planeRotation);
                snappedPoint = SnapAndUpdateGridOrigin(snappedPoint, SnapManager.settings.snappingEnabled,
                   shapeData.settings.paintOnPalettePrefabs, shapeData.settings.paintOnMeshesWithoutCollider,
                   false);
                snappedPoint = ClosestPointOnPlane(snappedPoint, shapeData);
                if (prevPosition != snappedPoint)
                {
                    shapeData.MovePoint(shapeData.selectedPointIdx, snappedPoint);
                    wasEdited = true;
                }
                if (shapeData.selectedPointIdx == 0)
                {
                    var prevRotation = shapeData.rotation;
                    var rotation = Handles.RotationHandle(prevRotation, shapeData.center);
                    if (prevRotation != rotation)
                    {
                        shapeData.rotation = rotation;
                        wasEdited = true;
                    }
                }
            }
            if (!showHandles) return false;
            return clickOnPoint || wasEdited;
        }

        private static void CreateShape()
        {
            var nextShapeId = ShapeData.nextHexId;
            var objDic = Paint(ShapeManager.settings, PAINT_CMD, true, false, nextShapeId);
            var objs = objDic[nextShapeId].ToArray();
            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            var sceneGUID = AssetDatabase.AssetPathToGUID(scenePath);
            var initialBrushId = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.id : -1;
            var persistentData = new ShapeData(objs, initialBrushId, _shapeData);
            ShapeManager.instance.AddPersistentItem(sceneGUID, persistentData);
        }
        private static void ShapeStateEdit(Camera camera)
        {
            var isCircle = ShapeManager.settings.shapeType == ShapeSettings.ShapeType.CIRCLE;
            var isPolygon = ShapeManager.settings.shapeType == ShapeSettings.ShapeType.POLYGON;

            if (_updateStroke)
            {
                BrushstrokeManager.UpdateShapeBrushstroke();
                _updateStroke = false;
            }
            ShapeStrokePreview(camera, ShapeData.nextHexId);

            DrawShapeLines(_shapeData);
            DrawDotHandleCap(_shapeData.center);
            if (isPolygon)
                foreach (var vertex in _shapeData.vertices) DrawDotHandleCap(vertex);
            else DrawDotHandleCap(_shapeData.radiusPoint);
            if (_shapeData.selectedPointIdx >= 0)
                DrawDotHandleCap(_shapeData.selectedPoint, 1f, 1.2f);
            DrawDotHandleCap(_shapeData.GetPoint(-1));
            DrawDotHandleCap(_shapeData.GetPoint(-2));

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                CreateShape();
                ResetShapeState(false);
            }
            else if (Event.current.button == 1 && Event.current.type == EventType.MouseDrag
                && Event.current.shift && Event.current.control)
            {
                var deltaSign = Mathf.Sign(Event.current.delta.x + Event.current.delta.y);
                ShapeManager.settings.gapSize += Mathf.PI * _shapeData.radius * deltaSign * 0.001f;
                ToolProperties.RepainWindow();
                Event.current.Use();
            }

            bool clickOnPoint = false;
            for (int i = 0; i < _shapeData.pointsCount; ++i)
            {
                if (isCircle && i == 2) i = _shapeData.pointsCount - 2;
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (!clickOnPoint)
                {
                    float distFromMouse = HandleUtility.DistanceToRectangle(_shapeData.GetPoint(i),
                        _shapeData.planeRotation, 0f);
                    HandleUtility.AddControl(controlId, distFromMouse);
                    if (HandleUtility.nearestControl != controlId) continue;
                    if (isPolygon) DrawDotHandleCap(_shapeData.GetPoint(i));
                    if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
                    {
                        _shapeData.selectedPointIdx = i;
                        clickOnPoint = true;
                        Event.current.Use();
                    }
                }
            }

            if (_shapeData.selectedPointIdx >= 0)
            {
                var prevPosition = _shapeData.selectedPoint;
                var snappedPoint = Handles.PositionHandle(prevPosition, _shapeData.planeRotation);
                snappedPoint = SnapAndUpdateGridOrigin(snappedPoint, SnapManager.settings.snappingEnabled,
                   ShapeManager.settings.paintOnPalettePrefabs, ShapeManager.settings.paintOnMeshesWithoutCollider,
                   false);
                snappedPoint = ClosestPointOnPlane(snappedPoint, _shapeData);
                if (prevPosition != snappedPoint)
                {
                    _shapeData.MovePoint(_shapeData.selectedPointIdx, snappedPoint);
                    _updateStroke = true;
                }
                if (_shapeData.selectedPointIdx == 0)
                {
                    var prevRotation = _shapeData.rotation;
                    var rotation = Handles.RotationHandle(prevRotation, _shapeData.center);
                    if (prevRotation != rotation)
                    {
                        _shapeData.rotation = rotation;
                        _updateStroke = true;
                    }
                }
            }
        }

        public static Vector3[] GetPolygonVertices(ShapeData shapeData)
        {
            var tangent = Vector3.Cross(Vector3.left, shapeData.normal);
            if (tangent.sqrMagnitude < 0.000001) tangent = Vector3.Cross(Vector3.forward, shapeData.normal);
            var bitangent = Vector3.Cross(shapeData.normal, tangent);

            var polygonSides = shapeData.settings.shapeType == ShapeSettings.ShapeType.CIRCLE
                ? shapeData.circleSideCount : shapeData.settings.sidesCount;

            var periPoints = new List<Vector3>();
            var centerToRadius = shapeData.radiusPoint - shapeData.center;
            var sign = Vector3.Dot(Vector3.Cross(tangent, centerToRadius), shapeData.normal) > 0 ? 1f : -1f;
            float mouseAngle = Vector3.Angle(tangent, centerToRadius) * Mathf.Deg2Rad * sign;

            for (int i = 0; i < polygonSides; ++i)
            {
                var radians = TAU * i / polygonSides + mouseAngle;
                var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var worldDir = TangentSpaceToWorld(tangent, bitangent, tangentDir).normalized;
                periPoints.Add(shapeData.center + (worldDir * shapeData.radius));
            }
            return periPoints.ToArray();
        }

        public static Vector3[] GetPolygonVertices() => GetPolygonVertices(_shapeData);

        public static Vector3[] GetArcVertices(float radius, ShapeData shapeData)
        {
            var tangent = Vector3.Cross(Vector3.left, shapeData.normal);
            if (tangent.sqrMagnitude < 0.000001) tangent = Vector3.Cross(Vector3.forward, shapeData.normal);
            var bitangent = Vector3.Cross(shapeData.normal, tangent);

            const float polygonSideSize = 0.3f;
            const int minPolygonSides = 8;
            const int maxPolygonSides = 60;
            var polygonSides = Mathf.Clamp((int)(TAU * radius / polygonSideSize), minPolygonSides, maxPolygonSides);

            var periPoints = new List<Vector3>();
            var centerToRadius = shapeData.GetPoint(-1) - shapeData.center;
            var sign = Vector3.Dot(Vector3.Cross(tangent, centerToRadius), shapeData.normal) > 0 ? 1 : -1;
            float firstAngle = Vector3.Angle(tangent, centerToRadius) * Mathf.Deg2Rad * sign;
            var sideDelta = TAU / polygonSides * Mathf.Sign(shapeData.arcAngle);

            for (int i = 0; i <= polygonSides; ++i)
            {
                var delta = sideDelta * i;
                bool arcComplete = false;
                if (Mathf.Abs(delta * Mathf.Rad2Deg) > Mathf.Abs(shapeData.arcAngle))
                {
                    delta = shapeData.arcAngle * Mathf.Deg2Rad;
                    arcComplete = true;
                }
                var radians = delta + firstAngle;
                var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var worldDir = TangentSpaceToWorld(tangent, bitangent, tangentDir).normalized;
                periPoints.Add(shapeData.center + (worldDir * radius));
                if (arcComplete) break;
            }
            return periPoints.ToArray();
        }

        private static void DrawShapeLines(ShapeData shapeData)
        {
            if (shapeData.radius < 0.0001) return;
            var points = new List<Vector3>(shapeData.state == ToolManager.ToolState.PREVIEW
                ? GetPolygonVertices(shapeData) : shapeData.vertices);
            points.Add(points[0]);
            var pointsArray = points.ToArray();
            Handles.zTest = CompareFunction.Always;
            Handles.color = new Color(0f, 0f, 0f, 0.7f);
            Handles.DrawAAPolyLine(8, pointsArray);
            Handles.color = new Color(1f, 1f, 1f, 0.7f);
            Handles.DrawAAPolyLine(4, pointsArray);
            if (shapeData.state < ToolManager.ToolState.EDIT) return;
            var arcLines = new Vector3[] { shapeData.GetPoint(-1), shapeData.center, shapeData.GetPoint(-2) };
            Handles.color = new Color(0f, 0f, 0f, 0.7f);
            Handles.DrawAAPolyLine(4, arcLines);
            Handles.color = new Color(1f, 1f, 1f, 0.7f);
            Handles.DrawAAPolyLine(2, arcLines);
            var arcPoints = GetArcVertices(shapeData.radius * 1.5f, shapeData);
            Handles.color = new Color(0f, 0f, 0f, 0.7f);
            Handles.DrawAAPolyLine(4, arcPoints);
            Handles.color = new Color(1f, 1f, 1f, 0.7f);
            Handles.DrawAAPolyLine(2, arcPoints);
        }

        private static void ShapeStrokePreview(Camera camera, string hexId)
        {
            BrushstrokeItem[] brushstroke;
            if (PreviewIfBrushtrokestaysTheSame(out brushstroke, camera)) return;
            _paintStroke.Clear();
            var settings = ShapeManager.settings;
            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];

                var prefab = strokeItem.settings.prefab;
                if (prefab == null) continue;
                var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform);
                var size = bounds.size;
                var height = Mathf.Max(size.x, size.y, size.z) * 2;

                var normal = -settings.projectionDirection;

                var itemRotation = Quaternion.identity;
                var itemPosition = strokeItem.tangentPosition;

                var ray = new Ray(itemPosition + normal * height, -normal);
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit, out GameObject collider,
                        height * 2f, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider))
                    {
                        itemPosition = itemHit.point;
                        normal = itemHit.normal;
                    }
                    else if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }

                BrushSettings brushSettings = strokeItem.settings;
                if (settings.overwriteBrushProperties) brushSettings = settings.brushSettings;
                if (brushSettings.rotateToTheSurface)
                {
                    var itemTangent = GetTangent(normal);
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * brushSettings.surfaceDistance;
                }
                else itemPosition += normal * brushSettings.surfaceDistance;
                itemRotation *= Quaternion.Euler(strokeItem.additionalAngle);
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    var itemForward = itemRotation * Vector3.forward;
                    if (settings.parallelToTheSurface)
                    {
                        var plane = new Plane(normal, itemPosition);
                        itemForward = plane.ClosestPointOnPlane(itemForward + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(itemForward, normal);
                    }
                }
                if (brushSettings.embedInSurface && settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    var itemForward = itemRotation * Vector3.forward;
                    if (settings.parallelToTheSurface)
                    {
                        var plane = new Plane(normal, itemPosition);
                        itemForward = plane.ClosestPointOnPlane(itemForward + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(itemForward, normal);
                    }
                    var TRS = Matrix4x4.TRS(itemPosition, itemRotation, strokeItem.scaleMultiplier);
                    var bottomDistanceToSurfce = GetBottomDistanceToSurface(
                        strokeItem.settings.bottomVertices, TRS,
                        strokeItem.settings.height * strokeItem.scaleMultiplier.y,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider);
                    if (!brushSettings.embedAtPivotHeight)
                        bottomDistanceToSurfce -= strokeItem.settings.bottomMagnitude;
                    itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
                }
                itemPosition += itemRotation * brushSettings.localPositionOffset;
                var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, strokeItem.scaleMultiplier)
                    * Matrix4x4.Translate(-prefab.transform.position);
                var itemScale = Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier);
                var layer = settings.overwritePrefabLayer ? settings.layer : prefab.layer;
                Transform parentTransform = settings.parent;

                var paintItem = new PaintStrokeItem(prefab, itemPosition,
                    itemRotation * Quaternion.Euler(prefab.transform.eulerAngles),
                    itemScale, layer, parentTransform);
                paintItem.persistentParentId = hexId;

                _paintStroke.Add(paintItem);

                PreviewBrushItem(prefab, rootToWorld, layer, camera);
                _previewData.Add(new PreviewData(prefab, rootToWorld, layer));
            }
        }
    }
    #endregion
}
