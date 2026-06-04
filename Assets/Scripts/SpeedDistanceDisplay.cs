using UnityEngine;
using UnityEngine.UI;

public class SpeedDistanceDisplay : MonoBehaviour
{
    public PlayerSpeedController player;
    public Text speedText;
    public Text distanceText;

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (player == null)
        {
            return;
        }

        if (speedText != null)
        {
            speedText.text = $"Speed: {player.SpeedKmh:0.0} km/h";
        }

        if (distanceText != null)
        {
            distanceText.text = $"Distance: {player.DistanceKm:0.00} km";
        }
    }
}
