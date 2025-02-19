using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class checkProductCollision : MonoBehaviour
{
    // Start is called before the first frame update

    private GameObject[] products;
    public int numbObjOnEdge = 0;
    public Collider[] m_Collider;
    public Text panelNumbProdValue;

    // Update is called once per frame
    void Update()
    {
        numbObjOnEdge = 0;
        products = GameObject.FindGameObjectsWithTag("product");
        foreach (GameObject prd in products)
        {
            Vector3 prodPos = prd.transform.position;

            foreach (Collider cld in m_Collider)
            {
                if (cld.bounds.Contains(prodPos))
                {
                    numbObjOnEdge++;
                    
                }
            }

            panelNumbProdValue.text = numbObjOnEdge.ToString();
            Debug.Log("Bounds contain the amount of products : " + numbObjOnEdge);
        }
    }
}
