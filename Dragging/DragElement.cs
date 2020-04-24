namespace Assets.Scripts.Dragging
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using System;
    using UnityEngine.UI;

    public class DragElement : MonoBehaviour, ICacheble, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// Clone itself on dragging.
        /// </summary>
        public bool cloneOnDragging = true;

        /// <summary>
        /// Differentiate the cloning element of toolbox against the normal elements
        /// </summary>
        [HideInInspector]
        public bool isClonning = false;

        /// <summary>
        /// Type of operator. For ex, Minus, Plus, Multiply, Divide.
        /// </summary>
        public int typeOfOperator = 0;

        public DropObject lastDropObject = null;



        /// <summary>
        /// DragElement Transform cached reference. Workaround GetComponent method for better performance.
        /// </summary>
        public RectTransform TransformCache { get; set; }

        /// <summary>
        /// Unique Id for this DragElement.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Official event link callback for DragAndDrop when user begin drag that object. Do not rewrite because break main logic. 
        /// </summary>
        public Action OnBeginDragCallback { get; set; }

        /// <summary>
        /// Official event link callback for DragAndDrop when user continue dragging that object. Do not rewrite because break main logic. 
        /// </summary>
        public Action OnDragCallback { get; set; }

        /// <summary>
        /// Official event link callback for DragAndDrop when user stop and drop that object. Do not rewrite because break main logic. 
        /// </summary>
        public Action OnEndDragCallback { get; set; }

        /// <summary>
        /// Position of this DragElement when drag is starting.
        /// </summary>
        public Vector2 LastPosition { get; private set; }

        /// <summary>
        /// Reference to last parent in that moment when drag is started. 
        /// </summary>
        public Transform LastParent { get; private set; }

        /// <summary>
        /// Check whether this DragElement is dragging in that moment.
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// Check whether when that DragElement is already cached in DragAndDrop collections and do not check it again.
        /// </summary>
        public bool IsCached { get; set; }

        [ExecuteInEditMode]
        public void Awake()
        {
            this.Id = Guid.NewGuid();
            this.TransformCache = this.transform.GetComponent<RectTransform>();
        }

        public void Start()
        {
            this.LastParent = this.TransformCache.parent;
            CacheInDragAndDropPanel();
        }

        /// <summary>
        /// Cache in main DragAndDrop <see cref="DragAndDrop.DragElementsCache"/> when is not cached.
        /// </summary>
        public void CacheInDragAndDropPanel()
        {
            if (!this.IsCached)
            {
                DragAndDrop dragAndDrop = this.TransformCache.root.GetComponentInChildren<DragAndDrop>();
                dragAndDrop.CacheDragElement(this);
            }
        }

        public void Update()
        {
            if (this.IsDragging)
            {
                this.TransformCache.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        /// <summary>
        /// Trigger event when drag is beginning.
        /// </summary>
        /// <param name="eventData">Data from that event.</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (cloneOnDragging)
            {
                // This element is in the toolbox
                DragElement cloned = Instantiate(this.gameObject, this.TransformCache.parent).GetComponent<DragElement>();
                cloned.TransformCache.SetSiblingIndex(this.TransformCache.GetSiblingIndex());
                cloned.name = this.name;

                // This element is the dragging element
                this.cloneOnDragging = false;
                this.isClonning = true;
            }

            this.IsDragging = true;
            this.LastPosition = this.TransformCache.position;
            this.GetComponent<Graphic>().raycastTarget = false;

            this.OnBeginDragCallback?.Invoke();
        }

        /// <summary>
        /// Trigger event when drag continue.
        /// </summary>
        /// <param name="eventData">Data from that event.</param>
        public void OnDrag(PointerEventData eventData)
        {
            this.OnDragCallback?.Invoke();
        }

        /// <summary>
        /// Trigger event when drag is ended and object is dropped.
        /// </summary>
        /// <param name="eventData">Data from that event.</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            this.IsDragging = false;
            this.GetComponent<Graphic>().raycastTarget = true;

            this.OnEndDragCallback?.Invoke();
        }

        /// <summary>
        /// Return to last known position before drag is started.
        /// </summary>
        public void ReturnToLastPosition()
        {
            this.TransformCache.position = this.LastPosition;
        }

        /// <summary>
        /// Return to last known parent before drag is started.
        /// </summary>
        public void ReturnToLastParent()
        {
            this.TransformCache.SetParent(this.LastParent);
        }

        /// <summary>
        /// Set last known parent. Used to set before drag is started.
        /// </summary>
        public void SetLastParent()
        {
            this.LastParent = this.TransformCache.parent;
        }

        public void PlaceOnDropObject(DropObject dropObject)
        {
            TransformCache.SetParent(dropObject.TransformCache);
            TransformCache.localPosition = Vector2.zero;
            isClonning = false;
            if (lastDropObject != null)
                lastDropObject.isEmpty = true;

            dropObject.isEmpty = false;
            lastDropObject = dropObject;
        }

        public void DestroyWithoutAnimation()
        {
            if (lastDropObject != null)
                lastDropObject.isEmpty = true;
            Destroy(gameObject);
        }

        public void DestroyWithAnimation()
        {
            // Animate 
            BetweenGlobalPosition between = gameObject.AddComponent<BetweenGlobalPosition>();
            between.Duration = 0.3f;
            between.From = TransformCache.position;
            between.To = LastPosition;
            between.OnFinish.AddListener(() =>
            {
                if (lastDropObject != null)
                    lastDropObject.isEmpty = true;
                // Destroy game object
                Destroy(gameObject);
            });

            between.Play(true);
        }

        public void ReturnWithAnimation()
        {
            // Animate 
            BetweenGlobalPosition between = gameObject.AddComponent<BetweenGlobalPosition>();
            between.Duration = 0.3f;
            between.From = TransformCache.position;
            between.To = LastPosition;
            between.OnFinish.AddListener(() =>
            {
                // Destroy animation component
                Destroy(between);
            });

            between.Play(true);
        }
    }
}
