using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviour
{
    public GameObject bullet, spawner;
    public float force;
    public int ammo, clipSize, score;
    public Text ammoT, clipSizeT, scoreT;
    // Start is called before the first frame update
    void Start()
    {
        ammo = clipSize;
        ammoT.text = ammo.ToString();
        clipSizeT.text = clipSize.ToString();
        scoreT.text = score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && ammo > 0)
        {
            InstantiateProjectile(spawner.transform);
            ammo--;
            ammoT.text = ammo.ToString();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ammo = clipSize;
            ammoT.text = ammo.ToString();
        }
    }

    void InstantiateProjectile(Transform firepoint)
    {
        var projectileObj = Instantiate(bullet, firepoint.position, Quaternion.identity) as GameObject;
        Vector3 direction = transform.forward * force;
        projectileObj.GetComponent<Rigidbody>().AddForce(direction, ForceMode.Impulse);

    }

}
