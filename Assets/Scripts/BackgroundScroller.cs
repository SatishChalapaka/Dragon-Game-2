
using System;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Serializable]
    private class BackgroundLayer
    {
        public Transform[] segments;

        public float moveSpeed = 8f;
        public float segmentLength = 32f;
    }

    [Header("References")]
    [SerializeField] private Transform dragon;

    [Header("Background Layers")]
    [SerializeField] private List<BackgroundLayer> layers =
        new List<BackgroundLayer>();

    private void Update()
    {
        if (dragon == null)
            return;

        foreach (BackgroundLayer layer in layers)
        {
            ScrollLayer(layer);
        }
    }

    private void ScrollLayer(BackgroundLayer layer)
    {
        if (layer.segments == null ||
            layer.segments.Length < 2 ||
            layer.segmentLength <= 0f)
            return;

        float movement = layer.moveSpeed * Time.deltaTime;

        // Move every terrain segment.
        foreach (Transform segment in layer.segments)
        {
            if (segment != null)
            {
                segment.position += Vector3.back * movement;
            }
        }

        // Find the segment furthest ahead.
        float furthestZ = float.MinValue;

        foreach (Transform segment in layer.segments)
        {
            if (segment != null)
            {
                furthestZ = Mathf.Max(
                    furthestZ,
                    segment.position.z
                );
            }
        }

        // Reposition segments that have passed behind the dragon.
        foreach (Transform segment in layer.segments)
        {
            if (segment == null)
                continue;

            if (segment.position.z <=
                dragon.position.z - layer.segmentLength)
            {
                Vector3 position = segment.position;

                furthestZ += layer.segmentLength;
                position.z = furthestZ;

                segment.position = position;
            }
        }
    }
}
