using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoDrop : MonoBehaviour
{
    // Start is called before the first frame update
    public float cooldown, maxTime;
    public GameObject ammo;
    public bool pickedUp;

    // Update is called once per frame
    void Update()
    {
        if(pickedUp)
        {
            ammo.SetActive(false);
            cooldown -= Time.deltaTime;
        }
        if(cooldown <= 0)
        {
            pickedUp = false;
            ammo.SetActive(true);
            cooldown = maxTime;
        }
    }
}
