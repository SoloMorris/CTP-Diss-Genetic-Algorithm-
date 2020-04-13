using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected Vector2 startPosition = new Vector2(-20, -20);
    [SerializeField] [Range(1, 30)] protected float speed = 15.0f;
    public bool inUse = false;
    [SerializeField] protected GameObject player;
    public bool hitAlien;
    public bool miss;

    [SerializeField] protected bool bulletsCollide;
    protected Rigidbody2D myBody;
    public Grid.Tile occupiedTile;
    // Start is called before the first frame update
    void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        if (inUse)
        {
            KillOnRoundEnd();
            myBody.velocity = new Vector2(0, 1) * speed;
            Grid.instance.GetCollisionWithObject(gameObject, ref occupiedTile, Grid.Tile.TileState.OccupiedByBullet);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (inUse)
        {
            HitTarget(collision, "Wall", false);
            if (HitTarget(collision, "Alien"))
            { 
                var hitAlien = GeneticAlien._instance.GetAlienList()[collision.gameObject.
                    GetComponent<AlienController>().uid].GetComponent<GeneticAlien.Alien>();
                var hitAlienCont = hitAlien.instance.GetComponent<AlienController>();

                hitAlienCont.deathPosition = hitAlien.instance.transform.position;
                hitAlienCont.killed = true;
            }
        }
        //Just write a function to kill the alien, retard
        
    }

    protected bool KillOnRoundEnd()
    {
        if (!RoundManager.instance.roundActive)
        {
            ResetBullet();
            return true;
        }
        return false;
    }
    virtual public void ResetBullet()
    {
        transform.position = startPosition;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        if (occupiedTile != null)
            occupiedTile.currentTileState = Grid.Tile.TileState.Empty;
        inUse = false;
    }

    virtual protected bool BulletLaserCollision()
    {
        if (bulletsCollide)
            return true;
        else
            return false;
    }
    virtual protected bool HitTarget(Collider2D target, string tag, bool valid = true)
    {
        if (target.gameObject.CompareTag(tag) && valid)
        {
            Stats.instance.HitShot();
            ResetBullet();
            return true;
        }
        else if (target.gameObject.CompareTag(tag) && !valid)
        {
            Stats.instance.MissedShot();
            ResetBullet();
            return false;
        }

        return false;
    }
}
