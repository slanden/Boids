using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class behavior_boids : MonoBehaviour
{
    List<GameObject> Boids = new List<GameObject>();
    List<GameObject> Predators = new List<GameObject>();
    public GameObject Boid, Predator, destination;
    public float randomMin = 10;
    public float randomMax = 30;
    public float boidCount = 100;
    public float separation = 6;
    public float speedLimit = 1;

    public float cohesionCoefficient = 0.6f;
    public float seperationCoefficient = 0.5f;
    public float alignmentCoefficient = 0.23f;
    public float tendToPlaceCoefficient = 0.07f;
    public float flockHeight = 20;
    private float groundLevel = 0;

    //UI elements
    private UnityEngine.UI.Slider cohSlider;
    private UnityEngine.UI.Slider sepSlider;
    private UnityEngine.UI.Slider alignSlider;
    private UnityEngine.UI.Slider tendSlider;

    //BoundPosition Parameters
    //  Bounds & -Bounds == max & min
    public int xBounds = 100;
    public int yBounds = 100;
    public int zBounds = 10;

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

        //populate Predators list with predators in the scene
        foreach (GameObject g in FindObjectsOfType<GameObject>())
        {
            if (g.GetComponent<Predator>())
                Predators.Add(g);
        }

        //instantiate prefab boids
        for (int i = 0; i < boidCount; ++i)
        {
            GameObject g = Instantiate(Boid, Vector3.zero, Quaternion.identity) as GameObject;
            //g.name = "Boid" + i;
            //parent boids to a game object, set random positions and velocities
            g.transform.parent = gameObject.transform;
            g.transform.position = new Vector3(Random.Range(randomMin, randomMax),
                                   Random.Range(randomMin, randomMax), Random.Range(randomMin, randomMax));

            g.GetComponent<Boid>().velocity = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));

            Boids.Add(g);
        }
    }

    void Update()
    {
        //UI elements
        cohesionCoefficient = cohSlider.value;
        seperationCoefficient = sepSlider.value;
        alignmentCoefficient = alignSlider.value;
        tendToPlaceCoefficient = tendSlider.value;

        //for all predators, check if one is within boids' bounderies. (in the future consider a volume check)
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

        NewPositions(arePredatorsAround);        
    }

    // the boids' positions are calculated here using all the rules below
    Vector3 v1, v2, v3, v4, v5, v6;
    void NewPositions(bool predatorAround)
    {
        v1 = v2 = v3 = v4 = v5 = v6 = Vector3.zero;

        //  If a predator is within boid bounderies, run algorithm with the 'evade()' function
        //  to check for predators.
        if (predatorAround)
        {
            foreach (GameObject o in Boids)
            {
                v6 = evade(o);
                CalcPosition(o);
            }
        }
        else
        {
            //Boid movement w/o predator checks (if predator is not within range, no sense calculating for them)
            foreach (GameObject o in Boids)
                CalcPosition(o);
        }

    }
    void CalcPosition(GameObject o)
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

                //reopen bat wings
                o.transform.GetChild(1).localScale += new Vector3(0.9f, 0, 0);
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

    // rules
    //////////////////////////////////////////////////////////////////

      //    Find center mass of other boids by everaging their positions,
      //  then move current boid toward center mass.
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

        // move boid by 1% of the distance between center mass of boid group and current boid
        return v / 100f;
    }

    //    if boids are too close to each other, subtract the distance 
    //  between the two boids from the current boid's velocity
    Vector3 Separation(GameObject o)
    {
        Vector3 v = Vector3.zero;

        //loop boids
        foreach (GameObject b in Boids)
        {
            if (b != o)
            {
                if (Vector3.Magnitude(b.transform.position - o.transform.position) < separation)
                {
                    v -= (b.transform.position - o.transform.position);
                }
            }
        }

        //move boid by 1%
        return v / 100f;
    }

    //    Align boids' velocities by averaging them and subtracting
    //  the current boid's velocity from the average
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

    //normalize velocity, then apply speed factor to a keep steady pace
    void LimitVelocity(GameObject o)
    {
        if (o.GetComponent<Boid>().velocity.magnitude > speedLimit)
        {
            o.GetComponent<Boid>().velocity = (o.GetComponent<Boid>().velocity /
                o.GetComponent<Boid>().velocity.magnitude) * speedLimit;
        }
    }

    // push boids away from boundaries if they're exceeding them
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

        // check if boid should perch on the ground
        if (o.transform.position.y < groundLevel + 0.5f)
        {
            o.transform.position = new Vector3(o.transform.position.x, (groundLevel + 0.5f), o.transform.position.z);
            o.GetComponent<Boid>().isPerching = true;
            //cheap way to simulate bat's wings being closed
            o.transform.GetChild(1).localScale -= new Vector3(0.9f,0,0);
        }

        return v;
    }

    // apply a force in a given direction
    //      (not yet implemented)
    Vector3 StrongWind(GameObject o)
    {
        Vector3 v = Vector3.zero;

        return v;
    }

    // move boids toward a certain position
    Vector3 TendToPlace(GameObject o)
    {
        Vector3 v = destination.transform.position;
        v.y += flockHeight;

        //1% at a time
        return (v - o.transform.position) / 100f;
    }

    // evasive maneuver when a predator is too close
    Vector3 evade(GameObject o)
    {
        Vector3 v = Vector3.zero;
        foreach (GameObject p in Predators)
        {
            //    if predator is near boid, break boid's perching simulation 
            //  and move boid away from 'TendToPlace()' destination
            if (Vector3.Magnitude(p.transform.position - o.transform.position) < separation + 4)
            {
                o.GetComponent<Boid>().isPerching = false;
                o.GetComponent<Boid>().perchTimer = 0;
                tendToPlaceCoefficient *= -1;
                v -= (p.transform.position - o.transform.position);
            }
            // if the above condition was previously met, move toward 'TendToPlace()' destination
            else if (tendToPlaceCoefficient < 0)
            {
                tendToPlaceCoefficient *= -1;
            }
        }
        return v / 100;
    }

}
