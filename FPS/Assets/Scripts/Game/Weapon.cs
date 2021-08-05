using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    [SerializeField] Guns[] loadouts;
    [SerializeField] Transform weaponParent;

    private GameObject currentWeapon;
    private int currentIndex;
    private float cooldown;

    [SerializeField] GameObject bulletHolePrefab;
    [SerializeField] LayerMask canBeShot;

    bool isReloading = false;
    
    void Start()
    {
        foreach (Guns a in loadouts)
            a.Initialize();
        Equip(0);
    }

    void Update()
    {
        //if (!photonView.IsMine) return;

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("Equip",RpcTarget.All,0);
        }

        if (currentWeapon != null)
        {
            if (photonView.IsMine)
            {
                Aim(Input.GetMouseButton(1));

                if (Input.GetMouseButtonDown(0) && cooldown <= 0)
                {
                    if (loadouts[currentIndex].FireBullet())
                        photonView.RPC("Shoot", RpcTarget.All);
                    else
                        StartCoroutine(Reload(loadouts[currentIndex].reloadTime));

                }

                if (Input.GetKeyDown(KeyCode.R))
                    StartCoroutine(Reload(loadouts[currentIndex].reloadTime));

            //ending cooldown while not shooting
            cooldown -= Time.deltaTime;
            }

            //weapon position elasticity
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition,
                                                                    Vector3.zero,
                                                                    Time.deltaTime*4f);

        }
    }

    IEnumerator Reload(float waitTime)
    {
        FindObjectOfType<AudioManager>().Play("Reload");
        isReloading = true;
        currentWeapon.SetActive(false);
        yield return new WaitForSeconds(waitTime);
        loadouts[currentIndex].Reload();
        currentWeapon.SetActive(true);
        isReloading = false;
    }

    public void RefreshAmmo(Text ammoText)
    {
        int t_clip = loadouts[currentIndex].GetClip();
        int t_stash = loadouts[currentIndex].GetStash();

        ammoText.text = t_clip.ToString() + " / " + t_stash.ToString();
    }

    [PunRPC]
    void Equip(int loadoutIndex)
    {
        if (currentWeapon != null)
        {
            if (isReloading) StopCoroutine("Reload");
            Destroy(currentWeapon);
        }
        currentIndex = loadoutIndex;
        GameObject weaponEquipped = Instantiate(loadouts[loadoutIndex].prefab,
                                                weaponParent.position,
                                                weaponParent.rotation,
                                                weaponParent) as GameObject;
        weaponEquipped.transform.localPosition = Vector3.zero;
        weaponEquipped.transform.localEulerAngles = Vector3.zero;
        weaponEquipped.GetComponent<WeaponSway>().isMine = photonView.IsMine;

        currentWeapon = weaponEquipped;
    }

    void Aim(bool isAiming)
    {
        Transform tempAnchor = currentWeapon.transform.Find("Anchor");
        Transform temp_ads = currentWeapon.transform.Find("States/ADS");
        Transform temp_hip = currentWeapon.transform.Find("States/Hipfire");

        if(isAiming)
        {
            tempAnchor.position = Vector3.Lerp(tempAnchor.position,temp_ads.position,Time.deltaTime*loadouts[currentIndex].aimSpeed);
        }
        else
        {
            tempAnchor.position = Vector3.Lerp(tempAnchor.position, temp_hip.position, Time.deltaTime * loadouts[currentIndex].aimSpeed);
        }
    }

    [PunRPC]
    void Shoot()
    {
        Transform rayOrigin = transform.Find("Cameras/Player Camera");

        //bloom
        Vector3 bloom = rayOrigin.position + rayOrigin.forward * 1000f;
        bloom += Random.Range(-loadouts[currentIndex].bloom,loadouts[currentIndex].bloom)*rayOrigin.up;
        bloom += Random.Range(-loadouts[currentIndex].bloom, loadouts[currentIndex].bloom) * rayOrigin.right;
        bloom -= rayOrigin.position;
        bloom.Normalize();

        //raycast
        RaycastHit hitInfo = new RaycastHit();
        if(Physics.Raycast(rayOrigin.position,bloom,out hitInfo,10000f,canBeShot))
        {
            GameObject newHole = Instantiate(bulletHolePrefab,
                                                hitInfo.point + hitInfo.normal * 0.001f,
                                                Quaternion.identity) as GameObject;
            newHole.transform.LookAt(hitInfo.point + hitInfo.normal);
            Destroy(newHole, 5f);

            if (photonView.IsMine)
            {
                if(hitInfo.collider.gameObject.layer==11)
                {
                    hitInfo.collider.gameObject.GetPhotonView().RPC("TakeDamage",RpcTarget.All, loadouts[currentIndex].damage);
                }
            }

        }
        FindObjectOfType<AudioManager>().Play("Shoot");
        //gun fx
        currentWeapon.transform.Rotate(-loadouts[currentIndex].recoil,0,0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadouts[currentIndex].kickback;

        //firing cooldown
        cooldown = loadouts[currentIndex].firerate;
    }

    [PunRPC] 
    private void TakeDamage(int damage)
    {
        GetComponent<PlayerMovement>().TakeDamage(damage);
    }
}
