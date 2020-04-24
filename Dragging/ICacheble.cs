namespace Assets.Scripts.Dragging
{
    using UnityEngine;

    public interface ICacheble
    {
        RectTransform TransformCache { get; set; }

        bool IsCached { get; set; }

        void CacheInDragAndDropPanel();
    }
}