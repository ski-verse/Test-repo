using UnityEngine;

public class SkiErgGameBootstrap : MonoBehaviour
{
    private const string PlayerTagName = "Player";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildPrototypeScene()
    {
        if (Object.FindFirstObjectByType<PlayerSpeedController>() != null)
        {
            return;
        }

        CreateRoad();
        var player = CreatePlayer();
        CreateCamera(player.transform);
        CreateLight();
    }

    private static void CreateRoad()
    {
        var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Training Road";
        road.transform.position = new Vector3(0f, -0.05f, 60f);
        road.transform.localScale = new Vector3(8f, 0.1f, 140f);

        var renderer = road.GetComponent<Renderer>();
        renderer.material.color = new Color(0.16f, 0.18f, 0.2f);
    }

    private static GameObject CreatePlayer()
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "SkiErg Player";
        player.tag = PlayerTagName;
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.transform.localScale = new Vector3(0.9f, 1.2f, 0.9f);

        var renderer = player.GetComponent<Renderer>();
        renderer.material.color = new Color(0.1f, 0.55f, 0.95f);

        var controller = player.AddComponent<PlayerSpeedController>();
        controller.CurrentSpeed = 4f;

        return player;
    }

    private static void CreateCamera(Transform target)
    {
        var cameraObject = new GameObject("Follow Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        cameraObject.transform.position = target.position + new Vector3(0f, 4f, -8f);
        cameraObject.transform.LookAt(target.position + Vector3.up * 1.2f);

        var followCamera = cameraObject.AddComponent<FollowCamera>();
        followCamera.target = target;

        Camera.main?.gameObject.SetActive(false);
    }

    private static void CreateLight()
    {
        var lightObject = new GameObject("Sun Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
}
