using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FlyEggFrameWork.Tools
{
    public static class CommonTool
    {

        public static int[,] DictionaryToArray2(Dictionary<int, int> dic) { 
            int[,] array2 = new int[dic.Count, 2];

            for (int i = 0; i < dic.Count; i++) {
                array2[i,0] = dic.ElementAt(i).Key;
                array2[i,1] = dic.ElementAt(i).Value;
            }

            return array2;
        }


        public static Dictionary<TKey, TValue> DeepCopySimple<TKey, TValue>(
    Dictionary<TKey, TValue> source)
        {
            var newDict = new Dictionary<TKey, TValue>();
            foreach (var kv in source)
            {
                newDict[kv.Key] = kv.Value; // TValue 是 struct / string / 基础类型，可以直接复制
            }
            return newDict;
        }

        public static Dictionary<int, int> Array2ToDictionary(int[,] arr) {

            Dictionary<int, int>  dic= new Dictionary<int, int>();
            if (arr == null)
            {
                return dic;
            }
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                dic.Add(arr[i, 0], arr[i, 1]);
            }

            return dic;
        }

        public static int[] Vector3IntToArray(Vector3Int vector3Int)
        {
            int[] arr = new int[3];
            arr[0] = vector3Int.x;
            arr[1] = vector3Int.y;
            arr[2] = vector3Int.z;

            return arr;
        }
        public static int[] Vector2IntToArray(Vector2Int vector2Int)
        {
            int[] arr = new int[2];
            arr[0] = vector2Int.x;
            arr[1] = vector2Int.y;
            return arr;
        }

        public static int[,] Vector3IntArrayToArray2(Vector3Int[] vector3IntArray)
        {
            int[,] array2 = new int[vector3IntArray.Length, 3];
            for (int i = 0; i < vector3IntArray.Length; i++)
            {
                Vector3Int ver3 = vector3IntArray[i];
                array2[i, 0] = ver3.x;
                array2[i, 1] = ver3.y;
                array2[i, 2] = ver3.z;

            }
            return array2;
        }
        public static Vector2Int[] Array2ToVector2IntArray(int[,] arr2)
        {
            Vector2Int[] vector2IntArr = new Vector2Int[arr2.GetLength(0)];

            for (int i = 0; i < vector2IntArr.Length; i++)
            {
                vector2IntArr[i] = new Vector2Int(arr2[i, 0], arr2[i, 1]);
            }

            return vector2IntArr;
        }

        public static Vector3Int[] Array2ToVector3IntArray(int[,] arr2)
        {
            Vector3Int[] vector3IntArr = new Vector3Int[arr2.GetLength(0)];


            for (int i = 0; i < vector3IntArr.Length; i++)
            {
                vector3IntArr[i] = new Vector3Int(arr2[i, 0], arr2[i, 1], arr2[i, 2]);
            }

            return vector3IntArr;
        }

        public static Vector2Int ArrayToVector2Int(int[] arr)
        {
            Vector2Int vector2Int = new Vector2Int();
            vector2Int.x = arr[0];
            vector2Int.y = arr[1];
            return vector2Int;
        }


        public static Vector3Int ArrayToVector3Int(int[] arr)
        {
            Vector3Int vector3Int = new Vector3Int();
            vector3Int.x = arr[0];
            vector3Int.y = arr[1];
            vector3Int.z = arr[2];

            return vector3Int;
        }

       
        public static Vector3 GetRandomPositionInBox(Transform boxObj)
        {

            Vector3 randomPoint = boxObj.position;

            BoxCollider boxCollider = boxObj.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Console.Error.WriteLine(boxObj.ToString() + " does not have box collider component.");
            }

            Bounds bounds = boxCollider.bounds;

            randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            return randomPoint;
        }
        public static T FindComponentInAncestor<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            Transform parent = obj.transform.parent;

            while (parent != null && component == null)
            {
                component = parent.GetComponent<T>();
                parent = parent.parent;
            }

            return component;
        }

        public static T FindComponentInDescendant<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            foreach (Transform child in obj.transform)
            {
                T result = FindComponentInDescendant<T>(child.gameObject);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static Transform FindDescendantFromName(Transform parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindDescendantFromName(child, name);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static void DeleteAllChildren(Transform parent)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                children.Add(child);
            }
            for (int i = 0; i < children.Count; i++)
            {
                GameObject.Destroy(children[i].gameObject);
            }

        }
        public static void DeleteAllChildrenImmediate(Transform parent)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                children.Add(child);
            }
            for (int i = 0; i < children.Count; i++)
            {
                GameObject.DestroyImmediate(children[i].gameObject);
            }
        }

        public static void DeleteAllChildrenOtherThanFirst(Transform parent)
        {
            if (parent.childCount == 0)
            {
                return;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                Transform child = parent.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }

        public static Transform[] GetAllChildren(Transform parent)
        {
            Transform[] children = new Transform[parent.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = parent.GetChild(i);
            }

            return children;
        }

        internal static int[,] DictionaryToArray(Dictionary<int, int> demandItemMap)
        {
            int[,] arr = new int[demandItemMap.Count, 2];

            for (int i = 0; i < demandItemMap.Count; i++)
            {
                arr[i, 0] = demandItemMap.ElementAt(i).Key;
                arr[i, 1] = demandItemMap.ElementAt(i).Value;
            }


            return arr;
        }
    }

    public static class AnimationTool
    {

        public static void ThrowAnimation(Transform transform, Vector3 startPosition, Vector3 targetPosition, float height = 1f, float duration = 1.5f)
        {
            Vector3 centerPosition = (startPosition + targetPosition) / 2;
            centerPosition.y = startPosition.y + height;

            var path = new Vector3[] { startPosition, centerPosition, targetPosition };
            var throwMove = transform.DOPath(path, duration, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.OutQuad);

            throwMove.Play();
        }
    }

    public static class RigTool
    {
        public static void BindToBone(SkinnedMeshRenderer from, SkinnedMeshRenderer to)
        {
            SkinnedMeshRenderer fromSkin = from;
            SkinnedMeshRenderer toSkin = to;

            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            foreach (Transform bone in toSkin.bones)
                boneMap[bone.gameObject.name] = bone;

            SkinnedMeshRenderer myRenderer = fromSkin;
            Transform[] newBones = new Transform[myRenderer.bones.Length];
            for (int i = 0; i < myRenderer.bones.Length; ++i)
            {
                GameObject bone = myRenderer.bones[i].gameObject;
                if (!boneMap.TryGetValue(bone.name, out newBones[i]))
                {
                    Debug.Log("Unable to map bone \"" + bone.name + "\" to target skeleton.");
                    break;
                }
            }
            myRenderer.bones = newBones;
        }

    }


    public static class EnumUtils
    {
        public static string GetStringValue(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                var attr = Attribute.GetCustomAttribute(field, typeof(StringValueAttribute)) as StringValueAttribute;
                if (attr != null)
                {
                    return attr.Value;
                }
            }
            return null;
        }
    }


}


