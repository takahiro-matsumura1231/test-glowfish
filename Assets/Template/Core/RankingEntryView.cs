using TMPro;
using UnityEngine;

namespace Template.Core
{
	public class RankingEntryView : MonoBehaviour
	{
		[SerializeField] private TMP_Text rankText;
		[SerializeField] private TMP_Text scoreText;
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private GameObject trophyIcon;

		public void Bind(int rank, int score, string playerName)
		{
			if (rankText != null) rankText.text = rank.ToString();
			if (scoreText != null) scoreText.text = score.ToString();
			if (nameText != null) nameText.text = string.IsNullOrEmpty(playerName) ? "Guest" : playerName;
			
			// Show trophy icon only for top 3 ranks
			if (trophyIcon != null)
			{
				trophyIcon.SetActive(rank <= 3);
			}
		}
	}
}
