using UnityEngine;
using System.Collections;

public class PredatorController : MonoBehaviour 
{
    public GameObject predator;
    public float speed;

    void Update()
    {
        GetInput();
    }

    Vector3 GetInput()
    {
        Vector3 v = new Vector3(0, 0, 0);

        if(Input.GetKey(KeyCode.A))
        {
            v.x += 1;
        }
        if(Input.GetKey(KeyCode.D))
        {
            v.x -= 1;
        }

        return predator.transform.position += v * speed;
    }
    

    //public GameObject cannon, projectile;

    //void fire()
    //{
    //    if(Input.GetKeyDown(KeyCode.Space))
    //    {
    //        GameObject g = Instantiate(projectile, gameObject.transform.position, Quaternion.identity) as GameObject;
    //        g.GetComponent<Predator>().velocity = Vector3.left;
    //        g.transform.forward += g.GetComponent<Predator>().velocity;
    //    }
    //}
}
