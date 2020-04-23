using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Bullet
{
    public bool hitPlayer;
    private void FixedUpdate()
    {
        if (inUse)
        {
            KillOnRoundEnd();
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1) * speed;
            Grid.instance.GetCollisionWithObject(gameObject, ref occupiedTile, Grid.Tile.TileState.OccupiedByAlienLaser);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (inUse && !collision.gameObject.CompareTag("Alien"))
        {
            HitTarget(collision, "Wall", false);
            if (HitTarget(collision, "Player"))
            {
                RoundManager.instance.EndRound(true);
                GeneticAlien._instance.ResetPlayerAI();
            }
        }
        //Just write a fucntion to kill the alien, retard
    }

    override public void ResetBullet()
    {
        transform.position = startPosition;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        //inUse = false;
        occupiedTile.currentTileState = Grid.Tile.TileState.Empty;
        StartCoroutine(KillLaser());
    }

    IEnumerator KillLaser()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
}
