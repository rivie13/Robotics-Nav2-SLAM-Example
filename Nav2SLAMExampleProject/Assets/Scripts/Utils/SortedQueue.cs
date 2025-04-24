using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortedQueue
{
    public static int minIndex(ref Queue<Vector3> q, int sortedIndex)
    {
        int min_index = -1;
        float min_val = float.MaxValue;
        int n = q.Count;
        for (int i = 0; i < n; i++)
        {
            Vector3 curr = q.Peek();
            q.Dequeue();
            float magnitude = curr.magnitude; // Euclidean magnitude

            // Only consider elements up to sortedIndex
            if (magnitude <= min_val && i <= sortedIndex)
            {
                min_index = i;
                min_val = magnitude;
            }
            q.Enqueue(curr); // Re-enqueue to preserve queue
        }
        return min_index;
    }

    public static void insertMinToRear(ref Queue<Vector3> q, int min_index)
    {
        Vector3 min_val = Vector3.zero;
        int n = q.Count;
        for (int i = 0; i < n; i++)
        {
            Vector3 curr = q.Peek();
            q.Dequeue();
            if (i != min_index)
            {
                q.Enqueue(curr);
            }
            else
            {
                min_val = curr;
            }
        }
        q.Enqueue(min_val); // Place minimum at rear
    }

    public static void sortQueue(ref Queue<Vector3> q)
    {
        for (int i = 1; i <= q.Count; i++)
        {
            int min_index = minIndex(ref q, q.Count - i);
            insertMinToRear(ref q, min_index);
        }
    }


}
