using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class Guns : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float firerate;
    public float bloom;
    public float recoil;
    public float kickback;
    public float aimSpeed;
    public int ammo;
    public int clipSize;
    public float reloadTime;
    public GameObject prefab;

    private int clip;
    private int stash;

    public void Initialize()
    {
        stash = ammo;
        clip = clipSize;
    }

    public bool FireBullet()
    {
        if(clip>=1)
        {
            clip -= 1;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Reload()
    {
        stash += clip;
        clip = Mathf.Min(clipSize,stash);
        stash -= clip;
    }

    public int GetStash() { return stash; }
    public int GetClip() { return clip; }
}
