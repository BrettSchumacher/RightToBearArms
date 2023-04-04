using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    BearControllerSM player;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Duplicate GameManager found, deleting self");
            Destroy(this);
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = BearControllerSM.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void KillPlayer()
    {
        player.ResetController(RespawnPoint.GetSpawnPoint());
    }
}
