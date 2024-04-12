using TMPro;
using UnityEngine;

public class FavorSystem : MonoBehaviour
{
    public TextMeshProUGUI favorText;
    private readonly int startFavor = 50;
    private readonly int minFavor = 0;
    private readonly int maxFavor = 100;
    private int favor;

    void Start()
    {
        favor = startFavor;
        UpdateFavorText();
    }

    public void AddFavor(int amount)
    {
        favor += amount;
        if (favor > maxFavor)
            favor = maxFavor;
        else if (favor < minFavor)
            favor = minFavor;

        if (amount < 0 || amount > 0)
        {
            favorText.color = (amount < 0) ? Color.red : Color.green;
            Invoke(nameof(ResetFavorTextColor), 1.0f);
        }

        UpdateFavorText();
    }

    public int GetFavor()
    {
        return favor;
    }

    private void UpdateFavorText()
    {
        favorText.text = "Favor: " + favor;
    }

    private void ResetFavorTextColor()
    {
        favorText.color = Color.white;
    }
}
