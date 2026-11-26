using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Object = UnityEngine.Object;

// Week 11 講義 4.1 ~ 4.4：鑰匙、鑰匙插孔、鑰匙解鎖拉桿
// 選單：Tools > Week 11 > Build Key And Socket
public static class Week11KeySocketSetup
{
    [MenuItem("Tools/Week 11/Build Key And Socket")]
    public static void Build()
    {
        if (GameObject.Find("Key") != null || GameObject.Find("KeySocket") != null)
        {
            EditorUtility.DisplayDialog("Week 11", "場景裡已經有 Key 或 KeySocket，沒有重複建立。", "OK");
            return;
        }

        GameObject stand = GameObject.Find("KeyStand");
        GameObject lever = GameObject.Find("Lever");
        if (stand == null || lever == null)
        {
            EditorUtility.DisplayDialog("Week 11", "找不到 KeyStand 或 Lever。", "OK");
            return;
        }

        XRGrabInteractable leverGrab = lever.GetComponent<XRGrabInteractable>();
        if (leverGrab == null)
        {
            EditorUtility.DisplayDialog("Week 11", "Lever 上沒有 XR Grab Interactable。", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Build Key And Socket");
        int group = Undo.GetCurrentGroup();

        // 黃色材質
        const string matPath = "Assets/Key_Yellow.mat";
        Material yellow = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (yellow == null)
        {
            Material src = stand.GetComponent<Renderer>().sharedMaterial;
            yellow = new Material(src != null ? src.shader : Shader.Find("Standard"));
            yellow.color = new Color(1f, 0.85f, 0.1f);
            AssetDatabase.CreateAsset(yellow, matPath);
        }

        // ---------- 4.1 Key ----------
        Bounds sb = stand.GetComponent<Renderer>().bounds;
        GameObject key = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(key, "Create Key");
        key.name = "Key";
        key.transform.localScale = new Vector3(0.1f, 0.1f, 0.2f);
        key.transform.position = new Vector3(sb.center.x, sb.max.y + 0.05f, sb.center.z);
        key.GetComponent<Renderer>().sharedMaterial = yellow;
        Undo.AddComponent<Rigidbody>(key);
        Undo.AddComponent<XRGrabInteractable>(key);

        // ---------- 4.2 KeySocket ----------
        GameObject socket = new GameObject("KeySocket");
        Undo.RegisterCreatedObjectUndo(socket, "Create KeySocket");
        socket.transform.position = new Vector3(-0.95f, 1.2f, 0.15f);
        XRSocketInteractor socketInteractor = Undo.AddComponent<XRSocketInteractor>(socket);
        SphereCollider sc = Undo.AddComponent<SphereCollider>(socket);
        sc.isTrigger = true;
        sc.radius = 0.1f;

        // ---------- 4.3 可見的小球 ----------
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Undo.RegisterCreatedObjectUndo(sphere, "Create Socket Sphere");
        sphere.name = "Sphere";
        sphere.transform.SetParent(socket.transform, false);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        sphere.GetComponent<Renderer>().sharedMaterial = yellow;
        UnityEngine.Object.DestroyImmediate(sphere.GetComponent<SphereCollider>());

        // ---------- 4.4 鑰匙解鎖拉桿 ----------
        Undo.RecordObject(leverGrab, "Disable Lever Grab");
        leverGrab.enabled = false;

        var setEnabled = typeof(Behaviour).GetProperty("enabled").GetSetMethod();
        var action = (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), leverGrab, setEnabled);

        Undo.RecordObject(socketInteractor, "Socket Events");
        UnityEventTools.AddBoolPersistentListener(socketInteractor.selectEntered, action, true);
        UnityEventTools.AddBoolPersistentListener(socketInteractor.selectExited, action, false);
        EditorUtility.SetDirty(socketInteractor);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(socket.scene);
        Selection.activeGameObject = socket;
        Debug.Log("Week 11: Key、KeySocket 建立完成，記得 Ctrl + S 存檔。");
    }

    [MenuItem("Tools/Week 11/Add Head Collision Blocker")]
    public static void AddHeadBlocker()
    {
        var origin = Object.FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (origin == null)
        {
            EditorUtility.DisplayDialog("Week 11", "找不到 XR Origin。", "OK");
            return;
        }
        if (origin.GetComponent<HeadCollisionBlocker>() == null)
            Undo.AddComponent<HeadCollisionBlocker>(origin.gameObject);
        EditorSceneManager.MarkSceneDirty(origin.gameObject.scene);
        Selection.activeGameObject = origin.gameObject;
        Debug.Log("Week 11: 已在 XR Origin 加上 Head Collision Blocker，記得 Ctrl + S 存檔。");
    }
}
