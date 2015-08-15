using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class behavior_boids : MonoBehaviour
{
    List<GameObject> Boids = new List<GameObject>();
    List<GameObject> Predators = new List<GameObject>();
    public GameObject Boid, Predator, destination;
    public float randomMin = 1;
    public float randomMax = 30;
    public float boidCount = 0;
    public float separation = 1;
    public float speedLimit = 1;

    public float cohesionCoefficient = 0.35f;
    public float seperationCoefficient = 0.22f;
    public float alignmentCoefficient = 0.5f;
    public float tendToPlaceCoefficient = 0.06f;

    //UI elements
    private UnityEngine.UI.Slider cohSlider;// = GameObject.Find("slider_cohesion").GetComponent<UnityEngine.UI.Slider>();
    private UnityEngine.UI.Slider sepSlider;
    private UnityEngine.UI.Slider alignSlider;
    private UnityEngine.UI.Slider tendSlider;

    //BoundPosition Parameters
    //  Bounds & -Bounds == max & min
    public int xBounds, yBounds, zBounds;
    float groundLevel = 0f;

    bool playState = false;
    bool arePredatorsAround = false;

    void Start()
    {
        //set UI elements
        cohSlider = GameObject.Find("slider_Cohesion").GetComponent<UnityEngine.UI.Slider>();
        sepSlider = GameObject.Find("slider_Separation").GetComponent<UnityEngine.UI.Slider>();
        alignSlider = GameObject.Find("slider_Alignment").GetComponent<UnityEngine.UI.Slider>();
        tendSlider = GameObject.Find("slider_TendToPlace").GetComponent<UnityEngine.UI.Slider>();

        cohSlider.value = cohesionCoefficient;
        sepSlider.value = seperationCoefficient;
        alignSlider.value = alignmentCoefficient;
        tendSlider.value = tendToPlaceCoefficient;

        Vector3 pos = new Vector3(0, 0, 0);

        //instantiate prefab boids
        for (int i = 0; i < boidCount; ++i)
        {
            GameObject g = Instantiate(Boid, pos, Quaternion.identity) as GameObject;
            g.name = "Boid" + i;
            Boids.Add(g);
            //Boids.Add(Instantiate(Boid, pos, Quaternion.identity) as GameObject);
        }            

        foreach (GameObject b in Boids)
        {
            //set parent gameobject
            b.transform.parent = gameObject.transform;
            //set random position
            b.transform.position = new Vector3(Random.Range(randomMin, randomMax),
                                   Random.Range(randomMin, randomMax), Random.Range(randomMin, randomMax));
            //set initial velocity
            b.GetComponent<Boid>().velocity = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
        }

        //add populate Predators list with predators in the scene
        foreach (GameObject g in FindObjectsOfType<GameObject>())
        {
            if (g.GetComponent<Predator>())
                Predators.Add(g);
        }
    }

    void Update()
    {
        //UI elements
        cohesionCoefficient = cohSlider.value;
        seperationCoefficient = sepSlider.value;
        alignmentCoefficient = alignSlider.value;
        tendToPlaceCoefficient = tendSlider.value;

        #region
        /*
        if (Input.GetKeyDown(KeyCode.Space) && playState)
            playState = false;
        else if (Input.GetKeyDown(KeyCode.Space) && !playState)
            playState = true;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 v1, v2, v3, v4, v5, v6;
            v1 = v2 = v3 = v4 = v5 = v6 = Vector3.zero;

            foreach (GameObject o in Boids)
            {
                v1 = Cohesion(o) * cohesionCoefficient;
                v2 = Separation(o) * seperationCoefficient;
                v3 = Alignment(o) * alignmentCoefficient;

                v4 = BoundPosition(o);
                v5 = TendToPlace(o);

                //Debug.Log("Cohesion:   " + v1);
                //Debug.Log("Separation: " + v2);
                //Debug.Log("Alignment:  " + v3);
                Debug.Log(o.name + " initial velocity: " + o.GetComponent<Boid>().velocity);
                o.GetComponent<Boid>().velocity += v1;// + v2 + v3;// + v4 + v5;
                Debug.Log(o.name + " velocity after cohesion: " + o.GetComponent<Boid>().velocity);
                //Debug.Log(o.name + " velocity before limiting: " + o.GetComponent<Boid>().velocity);
                LimitVelocity(o);
                Debug.Log(o.name + " velocity after limiting: " + o.GetComponent<Boid>().velocity);
                Debug.Log(o.name + " initial position: " + o.transform.position);
                o.transform.position += o.GetComponent<Boid>().velocity;
                Debug.Log(o.name + " final position: " + o.transform.position);
                o.transform.forward = o.GetComponent<Boid>().velocity.normalized;
            }
        }
        */
        #endregion

        foreach (GameObject p in Predators)
        {
            if (p.transform.position.x < xBounds /2 && p.transform.position.x > -xBounds /2 ||
               p.transform.position.y < yBounds /2 && p.transform.position.y > -yBounds /2 ||
               p.transform.position.z < zBounds /2 && p.transform.position.z > -zBounds /2)
            {
                arePredatorsAround = true;
                break;
            }
            else
                arePredatorsAround = false;
        }
        MoveAll_NewPositions(arePredatorsAround);        
    }

    void MoveAll_NewPositions( bool predatorAround)
    {
        Vector3 v1, v2, v3, v4, v5, v6;
        v1 = v2 = v3 = v4 = v5 = v6 = Vector3.zero;

        if (predatorAround)
        {
            foreach (GameObject o in Boids)
            {
                v6 = flee(o);
                if (o.GetComponent<Boid>().isPerching)
                {
                    if (o.GetComponent<Boid>().perchTimer < o.GetComponent<Boid>().perchDelay)
                    {
                        o.GetComponent<Boid>().perchTimer += Time.deltaTime;
                    }
                    else
                    {
                        o.GetComponent<Boid>().isPerching = false;
                        o.GetComponent<Boid>().perchTimer = 0;
                    }
                }
                else
                {
                    v1 = Cohesion(o) * cohesionCoefficient;
                    v2 = Separation(o) * seperationCoefficient;
                    v3 = Alignment(o) * alignmentCoefficient;
                    v4 = BoundPosition(o);
                    v5 = TendToPlace(o) * tendToPlaceCoefficient;

                    o.GetComponent<Boid>().velocity += v1 + v2 + v3 + v4 + v5 + v6;
                    LimitVelocity(o);
                    o.transform.position += o.GetComponent<Boid>().velocity;
                    o.transform.forward = Vector3.Normalize(o.GetComponent<Boid>().velocity);
                }
            }
        }
        else
        {
            foreach (GameObject o in Boids)
            {
                if (o.GetComponent<Boid>().isPerching)
                {
                    if (o.GetComponent<Boid>().perchTimer < o.GetComponent<Boid>().perchDelay)
                    {
                        o.GetComponent<Boid>().perchTimer += Time.deltaTime;
                    }
                    else
                    {
                        o.GetComponent<Boid>().isPerching = false;
                        o.GetComponent<Boid>().perchTimer = 0;
                    }
                }
                else
                {
                    v1 = Cohesion(o) * cohesionCoefficient;
                    v2 = Separation(o) * seperationCoefficient;
                    v3 = Alignment(o) * alignmentCoefficient;
                    v4 = BoundPosition(o);
                    v5 = TendToPlace(o) * tendToPlaceCoefficient;

                    o.GetComponent<Boid>().velocity += v1 + v2 + v3 + v4 + v5;
                    LimitVelocity(o);
                    o.transform.position += o.GetComponent<Boid>().velocity;
                    o.transform.forward = Vector3.Normalize(o.GetComponent<Boid>().velocity);
                }
            }
        }


    }

    //flee
    void flee(GameObject o)
    {
        foreach (GameObject p in Predators)
        {
            if (Vector3.Magnitude(p.transform.position - o.transform.position) < separation)
            {
                o.GetComponent<Boid>().isPerching = false;
                o.GetComponent<Boid>().perchTimer = 0;
                alignmentCoefficient *= -1;
                cohesionCoefficient *= -1;
            }
            else if(alignmentCoefficient < 0 && cohesionCoefficient < 0)
            {
                alignmentCoefficient *= -1;
                cohesionCoefficient *= -1;
            }
        }
    }
    //rules
    Vector3 Cohesion(GameObject o)
    {
        Vector3 v = Vector3.zero;

        foreach (GameObject b in Boids)
        {
            if (b != o)
                v += b.transform.position;
        }

        v /= (Boids.Count - 1);

        v = v - o.transform.position;

        return v / 100f;
    }

    Vector3 Separation(GameObject o)
    {
        Vector3 v = Vector3.zero;

        //loop boids
        foreach (GameObject b in Boids)
        {
            if (b != o)
                if (Vector3.Magnitude(b.transform.position - o.transform.position) < separation)
                    v -= (b.transform.position - o.transform.position);
        }

        //loop all physical game objects
        //AllGameObjects.Remove(o);

        //foreach (GameObject b in AllGameObjects)
        //{
        //        if (Vector3.Magnitude(b.transform.position - o.transform.position) < separation)
        //            v -= (b.transform.position - o.transform.position);
        //}
        //AllGameObjects.Add(o);

        return v / 100f;
    }

    Vector3 Alignment(GameObject o)
    {
        Vector3 v = Vector3.zero;

        foreach (GameObject b in Boids)
        {
            if (b != o)
                v += b.GetComponent<Boid>().velocity;
        }

        v /= (Boids.Count - 1);

        return (v - o.GetComponent<Boid>().velocity) / 100f;
    }

    void LimitVelocity(GameObject o)
    {
        if (o.GetComponent<Boid>().velocity.magnitude > speedLimit)
            o.GetComponent<Boid>().velocity = (o.GetComponent<Boid>().velocity /
                o.GetComponent<Boid>().velocity.magnitude) * speedLimit;

        //if (v.magnitude > speedLimit)
        //    v = (v / v.magnitude) * speedLimit;
    }

    //bound position & perching
    Vector3 BoundPosition(GameObject o)
    {
        Vector3 v = new Vector3(0, 0, 0);

        if (o.transform.position.x < -xBounds)
            v.x = 10;
        else if (o.transform.position.x > xBounds)
            v.x = -10;
        if (o.transform.position.y < groundLevel + 0.5f)
            v.y = 0.5f;
        else if (o.transform.position.y > yBounds)
            v.y = -10;
        if (o.transform.position.z < -zBounds)
            v.z = 10;
        else if (o.transform.position.z > zBounds)
            v.z = -10;
        if (o.transform.position.y < groundLevel + 0.5f)
        {
            o.transform.position = new Vector3(o.transform.position.x, (groundLevel + 0.5f), o.transform.position.z);
            o.GetComponent<Boid>().isPerching = true;
        }

        return v;
    }

    Vector3 StrongWind(GameObject o)
    {
        Vector3 v = Vector3.zero;

        return v;
    }

    //tendency towards a particular place
    Vector3 TendToPlace(GameObject o)
    {
        Vector3 v = destination.transform.position;

        return (v - o.transform.position) / 100f;
    }

    //predators


}
