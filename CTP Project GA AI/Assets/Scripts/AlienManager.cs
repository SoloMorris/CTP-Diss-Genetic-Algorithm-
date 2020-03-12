using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class AlienManager : MonoBehaviour
//{
//    public static AlienManager _instance;
//    [SerializeField] private int alienCount;
//    [SerializeField] private int alienCap;
//    [SerializeField] GameObject alienPrefab;
//    [SerializeField] Vector2 startPoint;
//    public float xGap;
//    public float yGap;
//    public int rowLength;
//    public int rowCount;
//    public List<Alien> aliens = new List<Alien>();

//    public GameObject wall1;
//    public GameObject wall2;
//    Collider2D alienCollision;

//    public class Alien : MonoBehaviour
//    {
//        public GameObject instance;
//        public bool alive;
//        public float distanceTraveled;
//        public int speed;
//        public bool commander;

//        //To track when the alien is moving left or right
//        public enum MoveDir
//        {
//            Left,
//            Right
//        }
//        public MoveDir movingTo;

//    }
//    private void Awake()
//    {
//        //Will be accessed by other scripts, so singleton
//        if (_instance == null)
//        {
//            _instance = this;
//        }
//        alienCount = rowLength * rowCount;
//    }
//    // Start is called before the first frame update
//    void Start()
//    {
//        for (int i = 0; i < alienCap; i++)
//        {
//            aliens.Add(new Alien());
//            aliens[i].instance = Instantiate(alienPrefab);
//            aliens[i].instance.gameObject.SetActive(false);
//            aliens[i].commander = false;
//            aliens[i].instance.GetComponent<AlienController>().uid = i;

//        }

//    }

//    private void Update()
//    {
//        for (int i = 0; i < aliens.Count; i++)
//        {
//            if (aliens[i].instance.GetComponent<AlienController>().myCollision != new Collider2D())
//            {
//                if (aliens[i].instance.GetComponent<AlienController>().myCollision.gameObject == wall1 ||
//                    aliens[i].instance.GetComponent<AlienController>().myCollision.gameObject == wall2)
//                {
//                    if (aliens[i].instance.GetComponent<AlienController>().myCollision.gameObject == wall1)
//                    {
//                        wall1.gameObject.SetActive(false);
//                        wall2.gameObject.SetActive(true);
//                        //for (int h = 0; h < aliens.Count; h++)
//                        //{
//                        //    aliens[h].GetComponent<AlienController>().myCollision = new Collider2D();
//                        //}
//                    }
//                    else if (aliens[i].instance.GetComponent<AlienController>().myCollision.gameObject == wall2)
//                    {
//                        wall2.gameObject.SetActive(false);
//                        wall1.gameObject.SetActive(true);
//                        //for (int h = 0; h < aliens.Count; h++)
//                        //{
//                        //    aliens[h].instance.GetComponent<AlienController>().myCollision = new Collider2D();
//                        //}
//                    }
//                    else
//                    {
//                        Debug.LogWarning("Wall errror");
//                    }
//                    for (int j = 0; j < aliens.Count; j++)
//                    {
//                        aliens[i].instance.transform.position = new Vector3(
//                            aliens[i].instance.transform.position.x,
//                            aliens[i].instance.transform.position.y - yGap);
//                    }

//                }
//            }
//        }
//    }

//    public void InitAliens()
//    {
//        var loops = 0;
//        //Fill the screen with aliens.
//        //Initialise their base values, posiiton and assign a GameObject
//        for (int j = 0; j < rowCount; j++)
//        {
//            for (int i = 0; i < rowLength; i++)
//            {
//                var h = i + loops;
//                var id = ((h) + 1 + (loops / 10));

//                //Place the aliens along each row
//                aliens[id].instance.transform.position = new Vector2(
//                    startPoint.x + (xGap * i), startPoint.y - (yGap * j));

//                //Scale up the aliens at the front
//                var scl = aliens[id].instance.transform.localScale;
//                aliens[id].instance.transform.localScale = new Vector3(scl.x + (j * 0.05f), scl.y);

//                //Set up their base values and ID
//                aliens[id].alive = true;
//                aliens[id].instance.gameObject.SetActive(true);
//                aliens[id].distanceTraveled = 0.0f;

//            }
//            loops += 10;
//        }
//    }

//    public bool CheckAliensAlive()
//    {
//        for (int i = 0; i < aliens.Count; i++)
//        {
//            if (aliens[i].alive)
//                return true;
//        }
//        return false;
//    }
//}
