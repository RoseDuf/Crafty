using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUDs.Gameplay
{
    public class StarBar : MonoBehaviour
    {
        [SerializeField] private Sprite starSprite;
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private GameObject fiveStarPrefab;
        [Range(0, 35)]
        [SerializeField] private int starPoints = 0;

        public int StarPoints
        {
            get { return starPoints; }
            set
            {
                starPoints = value;
                AdjustStars();
            }
        }

        private void Awake()
        {
            StarPoints = starPoints;
        }

        private void AdjustStars()
        {
            int starCount = 0;
            int fiveStarCount = 0;
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (this.transform.GetChild(i).GetComponent<Image>().sprite.name.Equals(starSprite.name))
                    starCount++;
                else
                    fiveStarCount++;
            }

            while ((starCount + (fiveStarCount * 5)) != starPoints)
            {
                int totalStarCount = (starCount + (fiveStarCount * 5));
                if (totalStarCount - starPoints < 0)
                {
                    if (Mathf.Abs(totalStarCount - starPoints) < 5)
                    {
                        if (Mathf.Abs(totalStarCount - starPoints) + starCount >= 5)
                        {
                            GameObject go = Instantiate(fiveStarPrefab, this.transform);
                            go.transform.SetAsFirstSibling();
                            fiveStarCount++;
                        }
                        else
                        {
                            Instantiate(starPrefab, this.transform);
                            starCount++;
                        }
                    }
                    else
                    {
                        GameObject go = Instantiate(fiveStarPrefab, this.transform);
                        go.transform.SetAsFirstSibling();
                        fiveStarCount++;
                    }
                }
                else
                {
                    int lastIndex = (this.transform.childCount - 1);
                    if (this.transform.GetChild(lastIndex).GetComponent<Image>().sprite.name.Equals(starSprite.name))
                    {
                        Destroy(this.transform.GetChild(lastIndex).gameObject);
                        this.transform.GetChild(lastIndex).SetParent(this.transform.parent);
                        starCount--;
                    }
                    else
                    {
                        Destroy(this.transform.GetChild(lastIndex).gameObject);
                        this.transform.GetChild(lastIndex).SetParent(this.transform.parent);
                        fiveStarCount--;
                    }
                }
            }
        }
    }
}
