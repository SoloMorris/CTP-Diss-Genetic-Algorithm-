using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    Vector2 startPosition = new Vector2(-20, -20);
    [SerializeField] [Range(1,30)] private float speed = 15.0f;
    public bool inUse = false;
    [SerializeField] private GameObject player;
    public bool hitPlayer;
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
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1) * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (inUse)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                ResetBullet();
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                GeneticAlien._instance.ResetPlayerAI();
                GeneticAlien._instance.ResetAliens();
                GeneticAlien._instance.EndRound();
                ResetBullet();
            }
        }
        //Just write a fucntion to kill the alien, retard
    }

    public void ResetBullet()
    {
        transform.position = startPosition;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        //inUse = false;
        Destroy(gameObject);

    }
}
