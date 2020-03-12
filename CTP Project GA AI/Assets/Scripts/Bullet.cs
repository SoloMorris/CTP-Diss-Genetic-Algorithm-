using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Vector2 startPosition = new Vector2(-20, -20);
    private float speed = 15.0f;
    public bool inUse = false;
    [SerializeField] private GameObject player;
    public bool hitAlien;
    public bool miss;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (inUse)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1) * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (inUse)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                ResetBullet();
                Stats.instance.MissedShot();
            }
            else if (collision.gameObject.CompareTag("Alien"))
            {
                var hitAlien = GeneticAlien._instance.GetAlienList()[collision.gameObject.GetComponent<AlienController>().uid].GetComponent<GeneticAlien.Alien>();

                GeneticAlien._instance.GetAlienList()[collision.gameObject.GetComponent<AlienController>().uid].
                      GetComponent<GeneticAlien.Alien>().instance.GetComponent<AlienController>().deathPosition = hitAlien.instance.transform.position;

                hitAlien.instance.GetComponent<AlienController>().killed = true;

                GeneticAlien._instance.GetAlienList()[collision.gameObject.GetComponent<AlienController>().uid].
                      GetComponent<GeneticAlien.Alien>().alive = false;

                Stats.instance.HitShot();
                ResetBullet();
            }
        }
        //Just write a function to kill the alien, retard
    }

    virtual public void ResetBullet()
    {
        transform.position = startPosition;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        inUse = false;

    }

    virtual public void HitTarget(Collider2D target)
    {

        ResetBullet();
    }
}
