using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D player;

    [SerializeField] [Range(0, 20)] private float moveSpeed = 8;

    float horizontalInput;
    bool shootInput = false;
    [SerializeField] private GameObject bullet;
    private List<GameObject> bullets = new List<GameObject>();
    [SerializeField] public bool automatePlayerMovement;

    private GeneticAlien.Alien target = new GeneticAlien.Alien();

    public float fireTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Rigidbody2D>();

        for (int i = 0; i < 1; i++)
        {
            bullets.Add(bullet);
            bullets[i] = Instantiate(bullet);
            bullets[i].GetComponent<Bullet>().inUse = false;
        }

        if (automatePlayerMovement)
            moveSpeed = moveSpeed / 2;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        shootInput = Input.GetButtonDown("Jump");
        if (shootInput)
            Fire();

        if (automatePlayerMovement && GeneticAlien._instance.roundActive)
        {
            AIControlPlayer();
            fireTimer += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (horizontalInput == 0)
        {
            player.velocity = Vector2.zero;
        }
        else if (horizontalInput != 0)
        {
            player.velocity = new Vector2(horizontalInput, 0) * moveSpeed;
        }
       
    }

    private void Fire()
    {
        if (shootInput)
        {
            print("got shoot input");
            var currentBullet = FindAvailableBullet();
            if (currentBullet >= 0)
            {
                bullets[currentBullet].transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
                bullets[currentBullet].GetComponent<Bullet>().inUse = true;
            }
        }
    }

    private int FindAvailableBullet()
    {
        for (int i = 0; i < bullets.Count; i++)
        {
            if (!bullets[i].GetComponent<Bullet>().inUse)
            {
                print("found bullet");
                return i;
            }
        }
        print("didnt find bullet");
        return -1;
    }

    void AIControlPlayer()
    {
        if (!target.alive)
        {
            for (int i = 0; i < GeneticAlien._instance.GetAlienList().Count; i++)
            {
                print("searching for an alien");
                var _alien = GeneticAlien._instance.GetAlienList()[i].GetComponent<GeneticAlien.Alien>();
                if (_alien.alive)
                {
                    target = _alien;
                    break;
                }
            }
        }
        else
        {
            if (transform.position.x < (target.instance.transform.position.x-0.15f))
            {
                horizontalInput = 1;
            }
            else if (transform.position.x > (target.instance.transform.position.x + 0.15f))
            {
                horizontalInput = -1;
            }

            if (fireTimer >= 1f)
            {
                shootInput = true;
                Fire();
                fireTimer = 0;
            }
        }
    }

    public void ResetPlayerAI()
    {
        target = new GeneticAlien.Alien();
    }

    public GeneticAlien.Alien GetTarget()
    {
        return target;
    }

}
