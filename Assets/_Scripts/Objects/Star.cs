using UnityEngine;

public class Star : MonoBehaviour
{
    [SerializeField] private GameObject starModel;
    [SerializeField] private Material unclaimedMaterial;
    [SerializeField] private Material claimedMaterial;

    public void Claim()
    {
        starModel.transform.GetChild(0).GetComponent<MeshRenderer>().material = claimedMaterial;
        starModel.transform.GetChild(1).GetComponent<MeshRenderer>().material = claimedMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            Claim();
        }
    }
}
