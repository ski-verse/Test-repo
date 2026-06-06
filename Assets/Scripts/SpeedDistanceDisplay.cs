using TMPro;
using UnityEngine;

public class SpeedDistanceDisplay : MonoBehaviour
{
    public PlayerSpeedController player;
    public TMP_Text speedText;
    public TMP_Text distanceText;
    public TMP_Text lapText;
    public TMP_Text gradientText;

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

        if (lapText != null)
        {
            lapText.text = $"Lap: {player.CurrentLapNumber}";
        }

        if (gradientText != null)
        {
            gradientText.text = $"Gradient: {player.CurrentGradientPercent:0.0}%";
        }
    }
}
