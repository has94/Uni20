namespace Assets.Scripts.Dragging
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;

    /// <summary>
    /// DragAndDrop orchestrate all DragElement and DropObject instances on current scene. That instances can be dynamically created through play mode and in Editor mode.
    /// </summary>
    public class DragAndDrop : MonoBehaviour
    {
        private const string DragAndDropString = "[DragAndDrop]: ";

        /// <summary>
        /// Show/Hide logs in editor only. 
        /// </summary>
        public bool debugLogs;

        /// <summary>
        /// DragElements shown in Unity inspector to watch automatically assigned DragElements from scene.
        /// </summary>
        public DragElement[] dragElements;

        /// <summary>
        /// DragElements shown in Unity inspector to watch automatically assigned DropObjects from scene.
        /// </summary>
        public DropObject[] dropObjects;

        private static bool enableLogs;

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement is begin drag on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onBeginDrag = new UnityEvent();

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement is being dragged on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onDrag = new UnityEvent();

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement was dropped on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onEndDrag = new UnityEvent();

        /// <summary>
        /// Store client defined list of events which will executes when some GameObject with attached DragElement was dropped outside on GameObject with attached DropObject on scene.
        /// </summary>
        [SerializeField]
        public UnityEvent onDropElementOutside = new UnityEvent();

        /// <summary>
        /// Automatically assigned DragElements from scene presented in cache mode with fast and easy way to get DragElement from the collection.
        /// </summary>
        public Dictionary<Guid, DragElement> DragElementsCache { get; private set; } = new Dictionary<Guid, DragElement>();

        /// <summary>
        /// Automatically assigned DropObjects from scene presented in cache mode with fast and easy way to get DropObjects from the collection.
        /// </summary>
        public Dictionary<Guid, DropObject> DropObjectsCache { get; private set; } = new Dictionary<Guid, DropObject>();

        /// <summary>
        /// Current DragElement witch selected on moment on begin drag event to that moment when drop it.
        /// </summary>
        public DragElement SelectedDragElement { get; private set; }

        /// <summary>
        /// Last DragElement witch selected before new selected DragElement.
        /// </summary>
        public DragElement LastSelectedDragElement { get; private set; }

        /// <summary>
        /// Current hovered (pointer/touch enter boundaries) DropObject.
        /// </summary>
        public DropObject HoveredDropObject { get; private set; }

        /// <summary>
        /// Add new GameObject with attached DragElement on specific place in hierarchy.
        /// </summary>
        /// <param name="parent">Parent for new GameObject.</param>
        /// <param name="prefab">Original object to copy.</param>
        /// <param name="name">Specific name for new Game object.</param>
        /// <returns>Created instance of DragElement.</returns>
        public DragElement AddDragElement(Transform parent, DragElement prefab = null, string name = "NewDragElement")
        {
            DragElement dragElement;
            if (prefab == null)
            {
                dragElement = new GameObject(name, typeof(RectTransform), typeof(DragElement)).GetComponent<DragElement>();
            }
            else
            {
                dragElement = Instantiate(prefab, parent);
                dragElement.name = name;
            }

            dragElement.transform.SetParent(parent);
            CacheDragElement(dragElement);

            return dragElement;
        }

        /// <summary>
        /// Add new GameObject with attached DropObject on specific place in hierarchy.
        /// </summary>
        /// <param name="parent">Parent for new GameObject.</param>
        /// <param name="prefab">Original object to copy.</param>
        /// <param name="name">Specific name for new Game object.</param>
        /// <returns>Created instance of DropObject.</returns>
        public DropObject AddDropObject(Transform parent, DropObject prefab = null, string name = "NewDropObject")
        {
            DropObject dropObject;
            if (prefab == null)
            {
                dropObject = new GameObject(name, typeof(RectTransform), typeof(DropObject), typeof(Image)).GetComponent<DropObject>();
            }
            else
            {
                dropObject = Instantiate(prefab, parent);
                dropObject.name = name;
            }

            dropObject.transform.SetParent(parent);
            CacheDropObject(dropObject);

            return dropObject;
        }

        /// <summary>
        /// Destroy GameObject and remove from cache specific instance of DragElement
        /// </summary>
        /// <param name="dragElement"></param>
        public void Destroy(DragElement dragElement)
        {
            if (!DragElementsCache.ContainsKey(dragElement.Id))
            {
                Destroy(dragElement.gameObject);
                return;
            }

            Destroy(DragElementsCache[dragElement.Id].gameObject);
            DragElementsCache.Remove(dragElement.Id);
        }

        /// <summary>
        /// Destroy GameObject and remove from cache specific instance of DropObject
        /// </summary>
        /// <param name="dropObject"></param>
        public void Destroy(DropObject dropObject)
        {
            if (!DropObjectsCache.ContainsKey(dropObject.Id))
            {
                Destroy(dropObject.gameObject);
                return;
            }

            Destroy(DropObjectsCache[dropObject.Id].gameObject);
            DropObjectsCache.Remove(dropObject.Id);
        }

        /// <summary>
        /// Cache all DragElements which are stored in <see cref="dragElements"/> on start.
        /// </summary>
        private void CacheDragElements()
        {
            for (int i = 0; i < dragElements.Length; i++)
            {
                CacheDragElement(dragElements[i]);
            }
        }

        /// <summary>
        /// Cache all DropObjects which are stored in <see cref="dropObjects"/> on start.
        /// </summary>
        private void CacheDropObjects()
        {
            for (int i = 0; i < dropObjects.Length; i++)
            {
                CacheDropObject(dropObjects[i]);
            }
        }

        /// <summary>
        /// Attach on specific DragElement all officially drag events.
        /// </summary>
        /// <param name="dragElement">Specific DragElement for preparing.</param>
        private void PrepareDragEvents(DragElement dragElement)
        {
            dragElement.OnBeginDragCallback = () => { LoadBeginDragEvents(dragElement); };

            dragElement.OnDragCallback = () => LoadDragEvents();

            dragElement.OnEndDragCallback = () => LoadEndDragEvents();

            void LoadBeginDragEvents(DragElement element)
            {
                SelectedDragElement = element;
                SelectedDragElement.TransformCache.SetParent(transform);

                // Make all DragElements transparent
                foreach (var entry in DropObjectsCache)
                {
                    DropObject dropObject = entry.Value;
                    dropObject.GetComponent<Graphic>().raycastTarget = false;
                }

                onBeginDrag.Invoke();
            }

            void LoadDragEvents()
            {
                onDrag.Invoke();

                // Revert all DragElements transparent
                foreach (var entry in DropObjectsCache)
                {
                    DropObject dropObject = entry.Value;
                    dropObject.GetComponent<Graphic>().raycastTarget = true;
                }
            }

            void LoadEndDragEvents()
            {
                if (SelectedDragElement.isClonning)
                {
                    if (HoveredDropObject != null)
                    {
                        if (HoveredDropObject.isEmpty)
                        {
                            SelectedDragElement.PlaceOnDropObject(HoveredDropObject);

                            Debug.Log("Cloning: HoveredDropObject : Empty");
                        }
                        else
                        {
                            SelectedDragElement.DestroyWithAnimation();

                            Debug.Log("Cloning: HoveredDropObject : Not empty");
                        }
                    }
                    else if (HoveredDropObject == null)
                    {
                        SelectedDragElement.DestroyWithAnimation();

                        Debug.Log("Cloning: HoveredDropObject : NULL");
                    }
                }
                else
                {
                    if (HoveredDropObject != null)
                    {
                        if (HoveredDropObject.isEmpty)
                        {
                            SelectedDragElement.PlaceOnDropObject(HoveredDropObject);

                            Debug.Log("Grid: HoveredDropObject : Empty");
                        }
                        else
                        {
                            SelectedDragElement.ReturnWithAnimation();

                            Debug.Log("Grid: HoveredDropObject : Not empty");
                        }
                    }
                    else if (HoveredDropObject == null)
                    {
                        SelectedDragElement.DestroyWithoutAnimation();

                        Debug.Log("Grid: HoveredDropObject : NULL");
                    }
                }

                LastSelectedDragElement = SelectedDragElement;
                SelectedDragElement = null;

                onEndDrag.Invoke();
            }
        }

        /// <summary>
        /// Attach on specific DropObject all officially drop events.
        /// </summary>
        /// <param name="dropObject">Specific DropObject for preparing.</param>
        private void PrepareDropEvents(DropObject dropObject)
        {
            dropObject.OnPointerEnterCallback = () => { LoadPointerEnterEvents(dropObject); };

            dropObject.OnPointerExitCallback = () => { LoadPointerExitEvents(); };

            void LoadPointerEnterEvents(DropObject оbject)
            {
                HoveredDropObject = оbject;
            }

            void LoadPointerExitEvents()
            {
                HoveredDropObject = null;
            }
        }

        /// <summary>
        /// Attach events and Cache specific DragElement in <see cref="DragElementsCache"/>
        /// </summary>
        /// <param name="dragElement"></param>
        public void CacheDragElement(DragElement dragElement)
        {
            PrepareDragEvents(dragElement);

            if (!DragElementsCache.ContainsKey(dragElement.Id))
            {
                DragElementsCache.Add(dragElement.Id, null);
            }

            DragElementsCache[dragElement.Id] = dragElement;
            DragElementsCache[dragElement.Id].IsCached = true;
        }

        /// <summary>
        /// Attach events and Cache specific DropObject in <see cref="DropObjectsCache"/>
        /// </summary>
        /// <param name="dropObject"></param>
        public void CacheDropObject(DropObject dropObject)
        {
            PrepareDropEvents(dropObject);

            Guid id = dropObject.Id;
            if (!DropObjectsCache.ContainsKey(id))
            {
                DropObjectsCache.Add(id, null);
            }

            DropObjectsCache[id] = dropObject;
            DropObjectsCache[id].IsCached = true;
        }

        /// <summary>
        /// Add component when that component is missing from specific GameObject.
        /// </summary>
        /// <typeparam name="T">Type of new checked component.</typeparam>
        /// <param name="gameObject">GameObject which will check for missing component.</param>
        /// <returns></returns>
        public static T AddMissingComponent<T>(GameObject gameObject) where T : Component
        {
            T found = gameObject.GetComponent<T>();

            if (found == null)
            {
                return gameObject.AddComponent<T>();
            }

            return found;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            enableLogs = debugLogs;

            if (transform.root.GetComponentsInChildren<DragAndDrop>().Length > 1)
            {
                Log("More than one DragAndDrop instance has detected. You may have logical problems. Please centralize DragAndDrop logic on one script.", LogType.Warning, gameObject);
            }

            dragElements = transform.root.GetComponentsInChildren<DragElement>(true);
            dropObjects = transform.root.GetComponentsInChildren<DropObject>(true);
        }

        /// <summary>
        /// Centralized log message on unity Debug console.
        /// </summary>
        /// <param name="message">Specific message for print.</param>
        /// <param name="logType">Type of the message.</param>
        /// <param name="GameObjectToPoint"></param>
        public static void Log(string message, LogType logType, GameObject GameObjectToPoint = null)
        {
            if (!enableLogs)
                return;

            switch (logType)
            {
                case LogType.Info:
                    Debug.Log($"{DragAndDropString}{message}", GameObjectToPoint);
                    break;
                case LogType.Warning:
                    Debug.LogWarning($"{DragAndDropString}{message}", GameObjectToPoint);
                    break;
                case LogType.Error:
                    Debug.LogError($"{DragAndDropString}{message}", GameObjectToPoint);
                    break;
            }
        }

        /// <summary>
        /// Message log types.
        /// </summary>
        public enum LogType
        {
            Info,
            Warning,
            Error
        }
#endif
    }
}
