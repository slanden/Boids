using UnityEngine;
using System.Collections;

public class Boid : MonoBehaviour
{
    public Vector3 velocity;
    public float speed = 1;
    public bool isPerching = false;
    public float perchTimer = 0f;
    public float perchDelay = 1.25f;
    //public Vector3 BVelocity
    //{
    //    set
    //    {
    //        velocity = value;
    //    }
    //    get
    //    {
    //        return velocity;
    //    }
    //}
}
