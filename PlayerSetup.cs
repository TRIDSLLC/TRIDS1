//-------------------------------------
// Responsible for setting up the player.
// This includes adding/removing him correctly on the network.
//-------------------------------------

using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : NetworkBehaviour {

	[SerializeField]
	Behaviour[] componentsToDisable;

	[SerializeField]
	string remoteLayerName = "RemotePlayer";

	[SerializeField]
	string dontDrawLayerName = "DontDraw";
	[SerializeField]
	GameObject playerGraphics;

	[SerializeField]
	GameObject playerUIPrefab;
	[HideInInspector]
	public GameObject playerUIInstance;

	void Start ()
	{
		if (!isLocalPlayer)
		{
			DisableComponents();
			AssignRemoteLayer();
		}
		else
		{
			// Disable player graphics for local player
			SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

			// Create PlayerUI
			playerUIInstance = Instantiate(playerUIPrefab);
			playerUIInstance.name = playerUIPrefab.name;

			// Configure PlayerUI
			PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
			if (ui == null)
				Debug.LogError("No PlayerUI component on PlayerUI prefab.");
			ui.SetController(GetComponent<PlayerController>());

		}

		GetComponent<Player>().Setup();
	}

	void SetLayerRecursively (GameObject obj, int newLayer)
	{
		obj.layer = newLayer;

		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}

    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    void AssignRemoteLayer ()
	{
		gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
	}

	void DisableComponents ()
	{
		for (int i = 0; i < componentsToDisable.Length; i++)
		{
			componentsToDisable[i].enabled = false;
		}
	}

	void OnDisable ()
	{
		Destroy(playerUIInstance);

		GameManager.instance.SetSceneCameraActive(true);
		
        GameManager.UnRegisterPlayer(transform.name);
	}

}