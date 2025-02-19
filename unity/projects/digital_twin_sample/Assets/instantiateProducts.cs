using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class instantiateProducts : MonoBehaviour
{
    public int numberProducts = 0;
    public GameObject spline;
    public GameObject prod;
    private List<GameObject> instProd = new List<GameObject>();
    private int numberInstantiatedProducts = 0;
    private float timeDelayNextProd = 0.0f;


    // Update is called once per frame
    void Update()
    {
        timeDelayNextProd += Time.deltaTime;

        if (numberInstantiatedProducts < numberProducts && timeDelayNextProd > 0.1f)
        {
            instProd.Add(Instantiate(prod, prod.transform.position, prod.transform.rotation) as GameObject);
            instProd[numberInstantiatedProducts].SetActive(true);
            numberInstantiatedProducts++;

            timeDelayNextProd = 0f;
        }
        else if ((numberInstantiatedProducts > numberProducts) && (timeDelayNextProd > 0.1f))
        {
            instProd[0].SetActive(false);
            instProd.RemoveAt(0);
            numberInstantiatedProducts--;

            timeDelayNextProd = 0f;
        }
    }
}
